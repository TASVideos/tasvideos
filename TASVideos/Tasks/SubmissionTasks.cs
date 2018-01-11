using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TASVideos.Constants;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Models;
using TASVideos.MovieParsers;
using TASVideos.WikiEngine;

namespace TASVideos.Tasks
{
    public class SubmissionTasks
    {
		private readonly ApplicationDbContext _db;
		private readonly MovieParser _parser;
		private readonly WikiTasks _wikiTasks;
		private readonly IMapper _mapper;
		public SubmissionTasks(
			ApplicationDbContext db,
			MovieParser parser,
			WikiTasks wikiTasks,
			IMapper mapper)
		{
			_db = db;
			_parser = parser;
			_wikiTasks = wikiTasks;
			_mapper = mapper;
		}

		// TODO: document - for reverifying a status can be set
		public async Task<SubmissionStatusValidationModel> GetStatusVerificationValues(int id, string userName)
		{
			return await _db.Submissions
				.Where(s => s.Id == id)
				.Select(s => new SubmissionStatusValidationModel
				{
					UserIsJudge = s.Judge != null && s.Judge.UserName == userName,
					UserIsAuhtorOrSubmitter = s.Submitter.UserName == userName || s.SubmissionAuthors.Any(sa => sa.Author.UserName == userName),
					CurrentStatus = s.Status,
					CreateDate = s.CreateTimeStamp
				})
				.SingleAsync();
		}

		/// <summary>
		/// Returns data for a <see cref="Submission"/> with the given <see cref="id" />
		/// for the purpose of display
		/// If a submission can not be found, null is returned
		/// </summary>
		public async Task<SubmissionViewModel> GetSubmission(int id, string userName)
		{
			using (_db.Database.BeginTransactionAsync())
			{
				var submissionModel = await _db.Submissions
					.Where(s => s.Id == id)
					.Select(s => new SubmissionViewModel // It is important to use a projection here to avoid querying the file data which is not needed and can be slow
					{
						Id = s.Id,
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
						Judge = s.Judge != null ? s.Judge.UserName : "",
						Title = s.Title,
						TierName = s.IntendedTier != null ? s.IntendedTier.Name : ""
					})
					.SingleOrDefaultAsync();

				if (submissionModel != null)
				{
					submissionModel.Authors = await _db.SubmissionAuthors
						.Where(sa => sa.SubmissionId == submissionModel.Id)
						.Select(sa => sa.Author.UserName)
						.ToListAsync();

					submissionModel.CanEdit = !string.IsNullOrWhiteSpace(userName)
						&& (userName == submissionModel.Submitter
							|| submissionModel.Authors.Contains(userName));
				}

				return submissionModel;
			}
		}

		public async Task<IEnumerable<SubmissionListViewModel>> GetSubmissionList(SubmissionSearchCriteriaModel criteria)
		{
			IQueryable<Submission> query = _db.Submissions;

			if (!string.IsNullOrWhiteSpace(criteria.User))
			{
				query = query.Where(s => s.Submitter.UserName == criteria.User);
			}

			var iquery = query
				.Include(s => s.Submitter)
				.Include(s => s.System)
				.Include(s => s.SystemFrameRate)
				.Include(s => s.SubmissionAuthors)
				.ThenInclude(sa => sa.Author);

			var results = await iquery.ToListAsync();
			return results.Select(s => new SubmissionListViewModel
			{
				Id = s.Id,
				System = s.System.Code,
				GameName = s.GameName,
				Time = s.Time,
				Branch = s.Branch,
				Author = string.Join(" & ", s.SubmissionAuthors.Select(sa => sa.Author.UserName)),
				Submitted = s.CreateTimeStamp,
				Status = s.Status
			});
		}

		/// <summary>
		/// Gets the title of a submission with the given id
		/// If the submission is not found, null is returned
		/// </summary>
		public async Task<string> GetTitle(int id)
		{
			return (await _db.Submissions.SingleOrDefaultAsync(s => s.Id == id))?.Title;
		}

		/// <summary>
		/// Returns data for a <see cref="Submission"/> with the given <see cref="id" />
		/// for the purpose of edit
		/// If a submission can not be found, null is returned
		/// </summary>
		public async Task<SubmissionEditModel> GetSubmissionForEdit(int id)
		{
			using (_db.Database.BeginTransactionAsync())
			{
				var submissionModel = await _db.Submissions
					.Where(s => s.Id == id)
					.Select(s => new SubmissionEditModel // It is important to use a projection here to avoid querying the file data which not needed and can be slow
					{
						Id = s.Id,
						SystemDisplayName = s.System.DisplayName,
						SystemCode = s.System.Code,
						SystemId = s.System.Id,
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
						GameId = s.Game != null ? s.Game.Id : (int?)null,
						TierId = s.IntendedTier != null ? s.IntendedTier.Id : (int?)null
					})
					.SingleOrDefaultAsync();

				if (submissionModel != null)
				{
					submissionModel.Authors = await _db.SubmissionAuthors
						.Where(sa => sa.SubmissionId == submissionModel.Id)
						.Select(sa => sa.Author.UserName)
						.ToListAsync();

					submissionModel.AvailableGames = await _db.Games
						.Where(g => g.SystemId == submissionModel.SystemId)
						.Select(g => new SelectListItem
						{
							Value = g.Id.ToString(),
							Text = g.DisplayName
						})
						.ToListAsync();

					submissionModel.AvailableTiers = await _db.Tiers
						.Select(t => new SelectListItem
						{
							Value = t.Id.ToString(),
							Text = t.Name
						})
						.ToListAsync();
				}

				return submissionModel;
			}
		}

		public async Task<SubmitResult> UpdateSubmission(SubmissionEditModel model, string userName)
		{
			var submission = await _db.Submissions
				.Include(s => s.Judge)
				.Include(s => s.System)
				.Include(s => s.SystemFrameRate)
				.Include(s => s.SubmissionAuthors)
				.ThenInclude(sa => sa.Author)
				.SingleAsync(s => s.Id == model.Id);

			// Parse movie file if it exists
			if (model.MovieFile != null)
			{
				// TODO: check warnings
				var parseResult = _parser.Parse(model.MovieFile.OpenReadStream());
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
					await model.MovieFile.CopyToAsync(memoryStream);
					submission.MovieFile = memoryStream.ToArray();
				}
			}

			// If a judge is claiming the submission
			if (model.Status == SubmissionStatus.JudgingUnderWay && submission.Status != SubmissionStatus.JudgingUnderWay)
			{
				submission.Judge = await _db.Users.SingleAsync(s => s.UserName == userName);
			}
			else if (submission.Status == SubmissionStatus.JudgingUnderWay // If judge is unclaiming, remove them
				&& model.Status == SubmissionStatus.New
				&& submission.Judge != null)
			{
				submission.Judge = null;
			}

			if (submission.Status != model.Status)
			{
				var history = new SubmissionStatusHistory
				{
					SubmissionId = submission.Id,
					Status = model.Status
				};
				submission.History.Add(history);
				_db.SubmissionStatusHistory.Add(history);
			}

			if (model.GameId.HasValue)
			{
				submission.Game = await _db.Games.SingleAsync(g => g.Id == model.GameId.Value);
			}

			if (model.TierId.HasValue)
			{
				submission.IntendedTier = await _db.Tiers.SingleAsync(t => t.Id == model.TierId.Value);
			}
			else
			{
				submission.IntendedTier = null;
			}

			submission.GameVersion = model.GameVersion;
			submission.GameName = model.GameName;
			submission.EmulatorVersion = model.Emulator;
			submission.Branch = model.Branch;
			submission.RomName = model.RomName;
			submission.EncodeEmbedLink = model.EncodeEmbedLink;
			submission.Status = model.Status;

			var id = await _wikiTasks.SavePage(new WikiEditModel
			{
				PageName = $"System/SubmissionContent/S{model.Id}",
				Markup = model.Markup,
				MinorEdit = model.MinorEdit,
				RevisionMessage = model.RevisionMessage,
				Referrals = Util.GetAllWikiLinks(model.Markup)
					.Select(wl => new WikiReferralModel
					{
						Link = wl.Link,
						Excerpt = wl.Excerpt
					})
			});

			submission.WikiContent = await _db.WikiPages.SingleAsync(wp => wp.Id == id);

			submission.GenerateTitle();
			await _db.SaveChangesAsync();

			return new SubmitResult(submission.Id);
		}

		/// <summary>
		/// Returns the submission file as bytes with the given id
		/// If no submission is found, an empty byte array is returned
		/// </summary>
		public async Task<byte[]> GetSubmissionFile(int id)
		{
			var data = await _db.Submissions
				.Where(s => s.Id == id)
				.Select(s => s.MovieFile)
				.SingleOrDefaultAsync();

			return data ?? new byte[0];
		}

		/// <summary>
		/// Takes the given data and generates a movie submission
		/// If successful, the Success flag will be true, and the Id field will be
		/// greater than 0 and represent the id to the newly created submission
		/// If unsucessful, the success flag will be false, Id will be 0 and the 
		/// Errors property will be filled with any relevant error messages
		/// </summary>
		public async Task<SubmitResult> SubmitMovie(SubmissionCreateViewModel model, string userName)
		{
			var submission = _mapper.Map<Submission>(model);
			submission.Submitter = await _db.Users.SingleAsync(u => u.UserName == userName);

			// Parse movie file
			// TODO: check warnings
			var parseResult = _parser.Parse(model.MovieFile.OpenReadStream());
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
				await model.MovieFile.CopyToAsync(memoryStream);
				submission.MovieFile = memoryStream.ToArray();
			}

			_db.Submissions.Add(submission);
			await _db.SaveChangesAsync();

			// Create a wiki page corresponding to this submission
			var wikiPage = new WikiPage
			{
				RevisionMessage = $"Auto-generated from Submission #{submission.Id}",
				PageName = LinkConstants.SubmissionWikiPage + submission.Id,
				MinorEdit = false,
				Markup = model.Markup
			};

			_db.WikiPages.Add(wikiPage);

			submission.WikiContent = wikiPage;

			// Add authors
			var users = await _db.Users
				.Where(u => model.Authors.Contains(u.UserName))
				.ToListAsync();

			var submissionAuthors = users.Select(u => new SubmissionAuthor
			{
				SubmissionId = submission.Id,
				UserId = u.Id
			});

			_db.SubmissionAuthors.AddRange(submissionAuthors);


			submission.GenerateTitle();
			await _db.SaveChangesAsync();

			return new SubmitResult(submission.Id);
		}

		public async Task<SubmissionPublishModel> GetSubmissionForPublish(int id)
		{
			// TODO: pre-validate, don't start the publication process without setting the obsoleting movie(s), intended tier, game, etc
			using (_db.Database.BeginTransactionAsync())
			{
				var model = await _db.Submissions
					.Where(s => s.Id == id && s.Status == SubmissionStatus.Accepted)
					.Select(s => new SubmissionPublishModel
					{
						Id = s.Id,
						Title = s.Title,
						SubmissionMarkup = s.WikiContent.Markup,
						SystemId = s.System.Id,
						TierId = s.IntendedTier != null ? s.IntendedTier.Id : (int?)null,
						Branch = s.Branch,
						EmulatorVersion = s.EmulatorVersion,
						GameVersion = s.GameVersion
					})
					.SingleOrDefaultAsync();

				if (model != null)
				{
					model.AvailableTiers = await _db.Tiers
						.Select(t => new SelectListItem
						{
							Value = t.Id.ToString(),
							Text = t.Name
						})
						.ToListAsync();

					model.AvailableGames = await _db.Games
						.Where(g => g.SystemId == model.SystemId)
						.Select(g => new SelectListItem
						{
							Value = g.Id.ToString(),
							Text = g.DisplayName
						})
						.ToListAsync();
				}

				return model;
			}
		}
	}
}
