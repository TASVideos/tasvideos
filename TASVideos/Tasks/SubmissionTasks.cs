using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TASVideos.Constants;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Models;
using TASVideos.MovieParsers;

namespace TASVideos.Tasks
{
    public class SubmissionTasks
    {
		private readonly ApplicationDbContext _db;
		private readonly MovieParser _parser;

		public SubmissionTasks(ApplicationDbContext db, MovieParser parser)
		{
			_db = db;
			_parser = parser;
		}

		/// <summary>
		/// Returns data for a <see cref="Submission"/> with the given <see cref="id" />
		/// for the purpose of display
		/// If a submission can not be found, null is returned
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public async Task<SubmissionViewModel> GetSubmission(int id)
		{
			var submissionModel = await _db.Submissions
				.Where(s => s.Id == id)
				.Select(s => new SubmissionViewModel // It is important to use a projection here to avoid querying the file data which can be slow
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
					FrameRate = s.FrameRate,
					RerecordCount = s.RerecordCount,
					CreateTimestamp = s.CreateTimeStamp,
					Submitter = s.Submitter.UserName,
					LastUpdateTimeStamp = s.WikiContent.LastUpdateTimeStamp,
					LastUpdateUser = s.WikiContent.LastUpdateUserName,
					Status = s.Status,
					EncodeEmbedLink = s.EncodeEmbedLink
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

			var submission = new Submission
			{
				Submitter = submitter,
				GameVersion = model.GameVersion,
				GameName = model.GameName,
				Branch = model.BranchName,
				RomName = model.RomName,
				EmulatorVersion = model.Emulator,
				EncodeEmbedLink = model.EncodeEmbedLink
			};

			// Parse movie file
			// TODO: check success, errors, warnings
			var parseResult = _parser.Parse(model.MovieFile.OpenReadStream());
			if (parseResult.Success)
			{
				submission.FrameRate = 60M; // TODO: look up from lookup table based on system and region
				submission.Frames = parseResult.Frames;
				submission.RerecordCount = parseResult.RerecordCount;
				submission.System = await _db.GameSystems.SingleOrDefaultAsync(g => g.Code == parseResult.SystemCode);
				if (submission.System == null)
				{
					return new SubmitResult($"Unknown system type of {parseResult.SystemCode}");
				}
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
