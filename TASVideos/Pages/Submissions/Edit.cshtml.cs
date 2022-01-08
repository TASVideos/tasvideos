using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TASVideos.Core.Services;
using TASVideos.Core.Services.ExternalMediaPublisher;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Data.Helpers;
using TASVideos.Extensions;
using TASVideos.MovieParsers;
using TASVideos.Pages.Submissions.Models;

namespace TASVideos.Pages.Submissions
{
	[RequirePermission(true, PermissionTo.SubmitMovies, PermissionTo.EditSubmissions)]
	public class EditModel : SubmissionBasePageModel
	{
		private readonly string _fileFieldName = $"{nameof(Submission)}.{nameof(SubmissionEditModel.MovieFile)}";
		private readonly IMovieParser _parser;
		private readonly IWikiPages _wikiPages;
		private readonly ExternalMediaPublisher _publisher;
		private readonly ITASVideosGrue _tasvideosGrue;
		private readonly IMovieFormatDeprecator _deprecator;
		private readonly ISubmissionService _submissionService;

		public EditModel(
			ApplicationDbContext db,
			IMovieParser parser,
			IWikiPages wikiPages,
			ExternalMediaPublisher publisher,
			ITASVideosGrue tasvideosGrue,
			IMovieFormatDeprecator deprecator,
			ISubmissionService submissionService)
			: base(db)
		{
			_parser = parser;
			_wikiPages = wikiPages;
			_publisher = publisher;
			_tasvideosGrue = tasvideosGrue;
			_deprecator = deprecator;
			_submissionService = submissionService;
		}

		[FromRoute]
		public int Id { get; set; }

		[BindProperty]
		public SubmissionEditModel Submission { get; set; } = new ();

		[Display(Name = "Status")]
		public IEnumerable<SubmissionStatus> AvailableStatuses { get; set; } = new List<SubmissionStatus>();

		public IEnumerable<SelectListItem> AvailableClasses { get; set; } = new List<SelectListItem>();

		public IEnumerable<SelectListItem> AvailableRejectionReasons { get; set; } = new List<SelectListItem>();

		public async Task<IActionResult> OnGet()
		{
			// TODO: set up auto-mapper and use ProjectTo<>
			Submission = await Db.Submissions
				.Where(s => s.Id == Id)
				.Select(s => new SubmissionEditModel // It is important to use a projection here to avoid querying the file data which not needed and can be slow
				{
					SystemDisplayName = s.System!.DisplayName,
					SystemCode = s.System.Code,
					GameName = s.GameName,
					GameVersion = s.GameVersion,
					RomName = s.RomName,
					Branch = s.Branch,
					Emulator = s.EmulatorVersion,
					FrameCount = s.Frames,
					FrameRate = s.SystemFrameRate!.FrameRate,
					RerecordCount = s.RerecordCount,
					CreateTimestamp = s.CreateTimestamp,
					Submitter = s.Submitter!.UserName,
					LastUpdateTimestamp = s.WikiContent!.LastUpdateTimestamp,
					LastUpdateUser = s.WikiContent.LastUpdateUserName,
					Status = s.Status,
					EncodeEmbedLink = s.EncodeEmbedLink,
					Markup = s.WikiContent.Markup,
					Judge = s.Judge != null ? s.Judge.UserName : "",
					Publisher = s.Publisher != null ? s.Publisher.UserName : "",
					PublicationClassId = s.IntendedClassId,
					RejectionReason = s.RejectionReasonId,
					AdditionalAuthors = s.AdditionalAuthors
				})
				.SingleOrDefaultAsync();

			if (Submission == null)
			{
				return NotFound();
			}

			Submission.Authors = await Db.SubmissionAuthors
				.Where(sa => sa.SubmissionId == Id)
				.OrderBy(sa => sa.Ordinal)
				.Select(sa => sa.Author!.UserName)
				.ToListAsync();

			var userName = User.Name();

			// If user can not edit submissions then they must be an author or the original submitter
			if (!User.Has(PermissionTo.EditSubmissions))
			{
				if (Submission.Submitter != userName
					&& !Submission.Authors.Contains(userName))
				{
					return AccessDenied();
				}
			}

			await PopulateDropdowns();

			AvailableStatuses = _submissionService.AvailableStatuses(
				Submission.Status,
				User.Permissions(),
				Submission.CreateTimestamp,
				Submission.Submitter == userName || Submission.Authors.Contains(userName),
				Submission.Judge == userName,
				Submission.Publisher == userName);

			return Page();
		}

		public async Task<IActionResult> OnPost()
		{
			if (User.Has(PermissionTo.ReplaceSubmissionMovieFile) && Submission.MovieFile is not null)
			{
				if (!Submission.MovieFile.IsZip())
				{
					ModelState.AddModelError(_fileFieldName, "Not a valid .zip file");
				}

				if (!Submission.MovieFile.LessThanMovieSizeLimit())
				{
					ModelState.AddModelError(_fileFieldName, ".zip is too big, are you sure this is a valid movie file?");
				}
			}
			else if (!User.Has(PermissionTo.ReplaceSubmissionMovieFile))
			{
				Submission.MovieFile = null;
			}

			// TODO: this has to be done anytime a string-list TagHelper is used, can we make this automatic with model binders?
			var subAuthors = Submission.Authors
				.Where(a => !string.IsNullOrWhiteSpace(a))
				.ToList();
			Submission.Authors = subAuthors;

			var userName = User.Name();

			// TODO: this is bad, an author can null out these values,
			// but if we treat null as no choice, then we have no way to unset these values
			if (!User.Has(PermissionTo.JudgeSubmissions))
			{
				Submission.PublicationClassId = null;
			}
			else if (Submission.PublicationClassId == null &&
				Submission.Status is SubmissionStatus.Accepted or SubmissionStatus.PublicationUnderway)
			{
				ModelState.AddModelError($"{nameof(Submission)}.{nameof(Submission.PublicationClassId)}", "A submission can not be accepted without a PublicationClass");
			}

			var subInfo = await Db.Submissions
				.Where(s => s.Id == Id)
				.Select(s => new
				{
					UserIsJudge = s.Judge != null && s.Judge.UserName == userName,
					UserIsPublisher = s.Publisher != null && s.Publisher.UserName == userName,
					UserIsAuthorOrSubmitter = s.Submitter!.UserName == userName || s.SubmissionAuthors.Any(sa => sa.Author!.UserName == userName),
					CurrentStatus = s.Status,
					CreateDate = s.CreateTimestamp
				})
				.SingleOrDefaultAsync();

			if (subInfo == null)
			{
				return NotFound();
			}

			var availableStatus = _submissionService.AvailableStatuses(
				subInfo.CurrentStatus,
				User.Permissions(),
				subInfo.CreateDate,
				subInfo.UserIsAuthorOrSubmitter,
				subInfo.UserIsJudge,
				subInfo.UserIsPublisher)
				.ToList();

			if (!availableStatus.Contains(Submission.Status))
			{
				ModelState.AddModelError($"{nameof(Submission)}.{nameof(Submission.Status)}", $"Invalid status: {Submission.Status}");
			}

			if (!ModelState.IsValid)
			{
				await PopulateDropdowns();
				AvailableStatuses = availableStatus;
				return Page();
			}

			// If user can not edit submissions then they must be an author or the original submitter
			if (!User.Has(PermissionTo.EditSubmissions))
			{
				if (!subInfo.UserIsAuthorOrSubmitter)
				{
					return AccessDenied();
				}
			}

			var submission = await Db.Submissions
				.Include(s => s.Topic)
				.Include(s => s.Judge)
				.Include(s => s.Publisher)
				.Include(s => s.System)
				.Include(s => s.SystemFrameRate)
				.Include(s => s.SubmissionAuthors)
				.ThenInclude(sa => sa.Author)
				.SingleAsync(s => s.Id == Id);

			if (Submission.MovieFile is not null)
			{
				var parseResult = await _parser.ParseZip(Submission.MovieFile.OpenReadStream());
				var deprecated = await _deprecator.IsDepcrecated("." + parseResult.FileExtension);
				if (deprecated)
				{
					ModelState.AddModelError(_fileFieldName, $".{parseResult.FileExtension} is no longer submittable");
					return Page();
				}

				await MapParsedResult(parseResult, submission);

				if (!ModelState.IsValid)
				{
					return Page();
				}

				submission.MovieFile = await Submission.MovieFile.ToBytes();
			}

			// If a judge is claiming the submission
			if (Submission.Status == SubmissionStatus.JudgingUnderWay
				&& submission.Status != SubmissionStatus.JudgingUnderWay)
			{
				submission.Judge = await Db.Users.SingleAsync(u => u.UserName == userName);
			}
			else if (submission.Status == SubmissionStatus.JudgingUnderWay // If judge is unclaiming, remove them
				&& Submission.Status == SubmissionStatus.New
				&& submission.Judge is not null)
			{
				submission.Judge = null;
			}

			if (Submission.Status == SubmissionStatus.PublicationUnderway
				&& submission.Status != SubmissionStatus.PublicationUnderway)
			{
				submission.Publisher = await Db.Users.SingleAsync(u => u.UserName == userName);
			}
			else if (submission.Status == SubmissionStatus.Accepted // If publisher is unclaiming, remove them
				&& Submission.Status == SubmissionStatus.PublicationUnderway)
			{
				submission.Publisher = null;
			}

			bool statusHasChanged = false;
			if (submission.Status != Submission.Status)
			{
				statusHasChanged = true;
				var history = new SubmissionStatusHistory
				{
					SubmissionId = submission.Id,
					Status = Submission.Status
				};

				Db.SubmissionStatusHistory.Add(history);

				if (Submission.Status != SubmissionStatus.Rejected &&
					Submission.Status != SubmissionStatus.Cancelled &&
					submission.Topic!.ForumId == SiteGlobalConstants.GrueFoodForumId)
				{
					submission.Topic.ForumId = SiteGlobalConstants.WorkbenchForumId;
				}
			}

			submission.RejectionReasonId = Submission.Status == SubmissionStatus.Rejected
				? Submission.RejectionReason
				: null;

			submission.IntendedClass = Submission.PublicationClassId.HasValue
				? await Db.PublicationClasses.SingleAsync(t => t.Id == Submission.PublicationClassId.Value)
				: null;

			submission.GameVersion = Submission.GameVersion;
			submission.GameName = Submission.GameName;
			submission.EmulatorVersion = Submission.Emulator;
			submission.Branch = Submission.Branch;
			submission.RomName = Submission.RomName;
			submission.EncodeEmbedLink = Submission.EncodeEmbedLink;
			submission.Status = Submission.Status;
			submission.AdditionalAuthors = Submission.AdditionalAuthors;

			var revision = new WikiPage
			{
				PageName = $"{LinkConstants.SubmissionWikiPage}{Id}",
				Markup = Submission.Markup,
				MinorEdit = Submission.MinorEdit,
				RevisionMessage = Submission.RevisionMessage,
				AuthorId = User.GetUserId()
			};
			var addResult = await _wikiPages.Add(revision);
			if (!addResult)
			{
				throw new InvalidOperationException("Unable to save wiki revision!");
			}

			submission.WikiContentId = revision.Id;

			submission.SubmissionAuthors.Clear();
			submission.SubmissionAuthors.AddRange(await Db.Users
				.Where(u => Submission.Authors.Contains(u.UserName))
				.Select(u => new SubmissionAuthor
				{
					SubmissionId = submission.Id,
					UserId = u.Id,
					Author = u,
					Ordinal = subAuthors.IndexOf(u.UserName)
				})
				.ToListAsync());

			submission.GenerateTitle();
			await Db.SaveChangesAsync();

			var topic = await Db.ForumTopics.FirstOrDefaultAsync(t => t.Id == submission.TopicId);
			if (topic is not null)
			{
				topic.Title = submission.Title;
				await Db.SaveChangesAsync();
			}

			if ((submission.Status == SubmissionStatus.Rejected || submission.Status == SubmissionStatus.Cancelled) && statusHasChanged)
			{
				await _tasvideosGrue.RejectAndMove(submission.Id);
			}

			if (!Submission.MinorEdit)
			{
				string title;
				if (statusHasChanged)
				{
					string statusStr = Submission.Status.ToString();
					if (Submission.Status == SubmissionStatus.Accepted)
					{
						var publicationClass = (await Db.PublicationClasses.SingleAsync(t => t.Id == Submission.PublicationClassId)).Name;

						if (publicationClass != "Standard")
						{
							statusStr += $" to {publicationClass}";
						}
					}

					title = $"{userName} set Submission {statusStr} on {submission.Title}";
				}
				else
				{
					title = $"{userName} edited Submission {submission.Title}";
				}

				await _publisher.SendSubmissionEdit(title, $"{Id}S", userName);
			}

			return RedirectToPage("View", new { Id });
		}

		public async Task<IActionResult> OnGetClaimForJudging()
		{
			if (!User.Has(PermissionTo.JudgeSubmissions))
			{
				return AccessDenied();
			}

			return await Claim(SubmissionStatus.New, SubmissionStatus.JudgingUnderWay, "judging", "Claiming for judging.", true);
		}

		public async Task<IActionResult> OnGetClaimForPublishing()
		{
			if (!User.Has(PermissionTo.PublishMovies))
			{
				return AccessDenied();
			}

			return await Claim(SubmissionStatus.Accepted, SubmissionStatus.PublicationUnderway, "publication", "Processing...", false);
		}

		private async Task<IActionResult> Claim(SubmissionStatus requiredStatus, SubmissionStatus newStatus, string action, string message, bool isJudge)
		{
			var submission = await Db.Submissions
				.Include(s => s.WikiContent)
				.SingleOrDefaultAsync(s => s.Id == Id);

			if (submission == null)
			{
				return NotFound();
			}

			if (submission.Status != requiredStatus)
			{
				return BadRequest("Submission can not be claimed");
			}

			var history = new SubmissionStatusHistory
			{
				SubmissionId = submission.Id,
				Status = Submission.Status
			};

			Db.SubmissionStatusHistory.Add(history);

			submission.Status = newStatus;
			var wikiPage = new WikiPage
			{
				PageName = submission.WikiContent!.PageName,
				Markup = submission.WikiContent.Markup += $"\n----\n[user:{User.Name()}]: {message}",
				RevisionMessage = $"Claimed for {action}",
				AuthorId = User.GetUserId()
			};
			await _wikiPages.Add(wikiPage);
			submission.WikiContentId = wikiPage.Id;

			if (isJudge)
			{
				submission.JudgeId = User.GetUserId();
			}
			else
			{
				submission.PublisherId = User.GetUserId();
			}

			var result = await ConcurrentSave(Db, "", "Unable to claim");
			if (result)
			{
				await _publisher.SendSubmissionEdit(
					$"Submission {submission.Title} set to {newStatus.EnumDisplayName()} by {User.Name()}",
					$"{Id}S",
					User.Name());
			}

			return RedirectToPage("View", new { Id });
		}

		private async Task PopulateDropdowns()
		{
			AvailableClasses = await Db.PublicationClasses
				.ToDropdown()
				.ToListAsync();

			AvailableRejectionReasons = await Db.SubmissionRejectionReasons
				.ToDropdown()
				.ToListAsync();
		}
	}
}
