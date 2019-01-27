using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

using TASVideos.Data;
using TASVideos.Data.Constants;
using TASVideos.Data.Entity;
using TASVideos.Data.Helpers;
using TASVideos.Extensions;
using TASVideos.Models;
using TASVideos.MovieParsers;
using TASVideos.Services;

namespace TASVideos.Pages.Submissions
{
	[RequirePermission(true, PermissionTo.SubmitMovies, PermissionTo.EditSubmissions)]
	public class EditModel : BasePageModel
	{
		private readonly ApplicationDbContext _db;
		private readonly MovieParser _parser;
		private readonly IWikiPages _wikiPages;

		public EditModel(
			ApplicationDbContext db,
			MovieParser parser,
			IWikiPages wikiPages,
			UserManager userManager)
			: base(userManager)
		{
			_db = db;
			_parser = parser;
			_wikiPages = wikiPages;
		}

		[FromRoute]
		public int Id { get; set; }

		[BindProperty]
		public SubmissionEditModel Submission { get; set; } = new SubmissionEditModel();

		[Display(Name = "Status")]
		public IEnumerable<SubmissionStatus> AvailableStatuses { get; set; } = new List<SubmissionStatus>();

		public IEnumerable<SelectListItem> AvailableTiers { get; set; }

		public IEnumerable<SelectListItem> GameVersionOptions { get; set; } = SubmissionHelper.GameVersionOptions;

		public async Task<IActionResult> OnGet()
		{
			// TODO: set up auto-mapper and use ProjectTo<>
			Submission = await _db.Submissions
				.Where(s => s.Id == Id)
				.Select(s => new SubmissionEditModel // It is important to use a projection here to avoid querying the file data which not needed and can be slow
				{
					SystemDisplayName = s.System.DisplayName,
					SystemCode = s.System.Code,
					GameName = s.GameName,
					GameVersion = s.GameVersion,
					RomName = s.RomName,
					Branch = s.Branch,
					Emulator = s.EmulatorVersion,
					FrameCount = s.Frames,
					FrameRate = s.SystemFrameRate.FrameRate,
					RerecordCount = s.RerecordCount,
					CreateTimestamp = s.CreateTimeStamp,
					Submitter = s.Submitter.UserName,
					LastUpdateTimeStamp = s.WikiContent.LastUpdateTimeStamp,
					LastUpdateUser = s.WikiContent.LastUpdateUserName,
					Status = s.Status,
					EncodeEmbedLink = s.EncodeEmbedLink,
					Markup = s.WikiContent.Markup,
					Judge = s.Judge != null ? s.Judge.UserName : "",
					TierId = s.IntendedTierId
				})
				.SingleOrDefaultAsync();

			if (Submission == null)
			{
				return NotFound();
			}

			Submission.Authors = await _db.SubmissionAuthors
				.Where(sa => sa.SubmissionId == Id)
				.Select(sa => sa.Author.UserName)
				.ToListAsync();

			// If user can not edit submissions then they must be an author or the original submitter
			if (!UserHas(PermissionTo.EditSubmissions))
			{
				if (Submission.Submitter != User.Identity.Name
					&& !Submission.Authors.Contains(User.Identity.Name))
				{
					return AccessDenied();
				}
			}

			await PopulateAvailableTiers();

			AvailableStatuses = SubmissionHelper.AvailableStatuses(
				Submission.Status,
				UserPermissions,
				Submission.CreateTimestamp,
				Submission.Submitter == User.Identity.Name || Submission.Authors.Contains(User.Identity.Name),
				Submission.Judge == User.Identity.Name);

			return Page();
		}

		public async Task<IActionResult> OnPost()
		{
			if (UserHas(PermissionTo.ReplaceSubmissionMovieFile) && Submission.MovieFile != null)
			{
				if (!Submission.MovieFile.FileName.EndsWith(".zip")
					|| Submission.MovieFile.ContentType != "application/x-zip-compressed")
				{
					ModelState.AddModelError(nameof(SubmissionCreateModel.MovieFile), "Not a valid .zip file");
				}

				if (Submission.MovieFile.Length > 150 * 1024)
				{
					ModelState.AddModelError(
						nameof(SubmissionCreateModel.MovieFile),
						".zip is too big, are you sure this is a valid movie file?");
				}
			}
			else if (!UserHas(PermissionTo.ReplaceSubmissionMovieFile))
			{
				Submission.MovieFile = null;
			}

			// TODO: this is bad, an author can null out these values,
			// but if we treat null as no choice, then we have no way to unset these values
			if (!UserHas(PermissionTo.JudgeSubmissions))
			{
				Submission.TierId = null;
			}

			var subInfo = await _db.Submissions
				.Where(s => s.Id == Id)
				.Select(s => new
				{
					UserIsJudge = s.Judge != null && s.Judge.UserName == User.Identity.Name,
					UserIsAuthorOrSubmitter = s.Submitter.UserName == User.Identity.Name || s.SubmissionAuthors.Any(sa => sa.Author.UserName == User.Identity.Name),
					CurrentStatus = s.Status,
					CreateDate = s.CreateTimeStamp
				})
				.SingleAsync();

			var availableStatus = SubmissionHelper.AvailableStatuses(
				subInfo.CurrentStatus,
				UserPermissions,
				subInfo.CreateDate,
				subInfo.UserIsAuthorOrSubmitter,
				subInfo.UserIsJudge)
				.ToList();

			if (!Submission.TierId.HasValue
				&& (Submission.Status == SubmissionStatus.Accepted || Submission.Status == SubmissionStatus.PublicationUnderway))
			{
				ModelState.AddModelError(nameof(Submission.TierId), "A submission can not be accepted without a Tier");
			}

			if (ModelState.IsValid)
			{
				if (!availableStatus.Contains(Submission.Status))
				{
					ModelState.AddModelError(nameof(Submission.Status), $"Invalid status: {Submission.Status}");
				}

				// If user can not edit submissions then they must be an author or the original submitter
				if (!UserHas(PermissionTo.EditSubmissions))
				{
					if (!subInfo.UserIsAuthorOrSubmitter)
					{
						return AccessDenied();
					}
				}

				var result = await UpdateSubmission();
				if (result.Success)
				{
					return Redirect($"/{Id}S");
				}

				foreach (var error in result.Errors)
				{
					ModelState.AddModelError("", error);
				}
			}

			await PopulateAvailableTiers();
			AvailableStatuses = availableStatus;

			return Page();
		}

		private async Task PopulateAvailableTiers()
		{
			AvailableTiers = await _db.Tiers
				.ToDropdown()
				.ToListAsync();
		}

		// TODO: move this logic inline
		// Id, Submission, User.Identity.Name
		private async Task<SubmitResult> UpdateSubmission()
		{
			var submission = await _db.Submissions
				.Include(s => s.Judge)
				.Include(s => s.Publisher)
				.Include(s => s.System)
				.Include(s => s.SystemFrameRate)
				.Include(s => s.SubmissionAuthors)
				.ThenInclude(sa => sa.Author)
				.SingleAsync(s => s.Id == Id);

			// Parse movie file if it exists
			if (Submission.MovieFile != null)
			{
				// TODO: check warnings
				var parseResult = _parser.Parse(Submission.MovieFile.OpenReadStream());
				if (parseResult.Success)
				{
					submission.Frames = parseResult.Frames;
					submission.RerecordCount = parseResult.RerecordCount;
					submission.System = await _db.GameSystems.SingleOrDefaultAsync(g => g.Code == parseResult.SystemCode);
					if (submission.System == null)
					{
						return new SubmitResult($"Unknown system type of {parseResult.SystemCode}");
					}

					submission.SystemFrameRate = await _db.GameSystemFrameRates
						.SingleOrDefaultAsync(f => f.GameSystemId == submission.System.Id
							&& f.RegionCode == parseResult.Region.ToString());
				}
				else
				{
					return new SubmitResult(parseResult.Errors);
				}

				using (var memoryStream = new MemoryStream())
				{
					await Submission.MovieFile.CopyToAsync(memoryStream);
					submission.MovieFile = memoryStream.ToArray();
				}
			}

			// If a judge is claiming the submission
			if (Submission.Status == SubmissionStatus.JudgingUnderWay
				&& submission.Status != SubmissionStatus.JudgingUnderWay)
			{
				submission.Judge = await _db.Users.SingleAsync(u => u.UserName == User.Identity.Name);
			}
			else if (submission.Status == SubmissionStatus.JudgingUnderWay // If judge is unclaiming, remove them
				&& Submission.Status == SubmissionStatus.New
				&& submission.Judge != null)
			{
				submission.Judge = null;
			}

			if (Submission.Status == SubmissionStatus.PublicationUnderway
				&& submission.Status != SubmissionStatus.PublicationUnderway)
			{
				submission.Publisher = await _db.Users.SingleAsync(u => u.UserName == User.Identity.Name);
			}
			else if (submission.Status == SubmissionStatus.Accepted // If publisher is unclaiming, remove them
				&& Submission.Status == SubmissionStatus.PublicationUnderway)
			{
				submission.Publisher = null;
			}

			if (submission.Status != Submission.Status)
			{
				var history = new SubmissionStatusHistory
				{
					SubmissionId = submission.Id,
					Status = Submission.Status
				};
				submission.History.Add(history);
				_db.SubmissionStatusHistory.Add(history);
			}

			if (Submission.TierId.HasValue)
			{
				submission.IntendedTier = await _db.Tiers.SingleAsync(t => t.Id == Submission.TierId.Value);
			}
			else
			{
				submission.IntendedTier = null;
			}

			submission.GameVersion = Submission.GameVersion;
			submission.GameName = Submission.GameName;
			submission.EmulatorVersion = Submission.Emulator;
			submission.Branch = Submission.Branch;
			submission.RomName = Submission.RomName;
			submission.EncodeEmbedLink = Submission.EncodeEmbedLink;
			submission.Status = Submission.Status;

			var revision = new WikiPage
			{
				PageName = $"{LinkConstants.SubmissionWikiPage}{Id}",
				Markup = Submission.Markup,
				MinorEdit = Submission.MinorEdit,
				RevisionMessage = Submission.RevisionMessage,
			};
			await _wikiPages.Add(revision);

			submission.WikiContent = await _db.WikiPages.SingleAsync(wp => wp.Id == revision.Id);

			submission.GenerateTitle();
			await _db.SaveChangesAsync();

			return new SubmitResult(submission.Id);
		}
	}
}
