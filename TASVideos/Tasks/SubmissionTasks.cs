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
		/// Takes the given data and generates a movie submission
		/// </summary>
		public async Task SubmitMovie(SubmissionCreateViewModel model)
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
		}
	}
}
