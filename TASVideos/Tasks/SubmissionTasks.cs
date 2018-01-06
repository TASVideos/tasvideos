using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
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
					Judge = s.Judge != null ? s.Judge.UserName : ""
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

		/// <summary>
		/// Returns data for a <see cref="Submission"/> with the given <see cref="id" />
		/// for the purpose of edit
		/// If a submission can not be found, null is returned
		/// </summary>
		public async Task<SubmissionEditModel> GetSubmissionForEdit(int id)
		{
			var submissionModel = await _db.Submissions
				.Where(s => s.Id == id)
				.Select(s => new SubmissionEditModel // It is important to use a projection here to avoid querying the file data which not needed and can be slow
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
					Markup = s.WikiContent.Markup,
					Judge = s.Judge != null ? s.Judge.UserName : ""
				})
				.SingleOrDefaultAsync();

			if (submissionModel != null)
			{
				submissionModel.Authors = await _db.SubmissionAuthors
					.Where(sa => sa.SubmissionId == submissionModel.Id)
					.Select(sa => sa.Author.UserName)
					.ToListAsync();
			}

			return submissionModel;
		}

		public async Task UpdateSubmission(SubmissionEditModel model, string userName)
		{
			var submission = await _db.Submissions
				.Include(s => s.Judge)
				.SingleAsync(s => s.Id == model.Id);

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
			await _db.SaveChangesAsync();
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
			var submitter = await _db.Users.SingleAsync(u => u.UserName == userName);
			var submission = _mapper.Map<Submission>(model);
			submission.Submitter = submitter;

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

			await _db.SaveChangesAsync();

			return new SubmitResult(submission.Id);
		}
	}
}
