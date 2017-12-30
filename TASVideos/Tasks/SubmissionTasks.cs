using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Models;

namespace TASVideos.Tasks
{
    public class SubmissionTasks
    {
		private readonly ApplicationDbContext _db;

		public SubmissionTasks(ApplicationDbContext db)
		{
			_db = db;
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
					GameName = s.GameName
				})
				.SingleOrDefaultAsync();

			return submissionModel;
		}

		/// <summary>
		/// Takes the given data and generates a movie submission
		/// </summary>
		public async Task<int> SubmitMovie(SubmissionCreateViewModel model)
		{
			var submission = new Submission
			{
				GameVersion = model.GameVersion,
				GameName = model.GameName,
				MovieFile = new byte[0] // TODO
			};

			using (var memoryStream = new MemoryStream())
			{
				await model.MovieFile.CopyToAsync(memoryStream);
				submission.MovieFile = memoryStream.ToArray();
			}

			_db.Submissions.Add(submission);

			await _db.SaveChangesAsync();
			return submission.Id;
		}
	}
}
