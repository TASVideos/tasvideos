using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Extensions;
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
		/// Gets a list of <see cref="Submission"/>s for the submission queue filtered on the given <see cref="criteria" />
		/// </summary>
		public async Task<IEnumerable<SubmissionListEntry>> GetSubmissionList(SubmissionSearchRequest criteria)
		{
			var iquery = _db.Submissions
				.Include(s => s.Submitter)
				.Include(s => s.System)
				.Include(s => s.SystemFrameRate)
				.Include(s => s.SubmissionAuthors)
				.ThenInclude(sa => sa.Author);

			IQueryable<Submission> query = iquery.SearchBy(criteria);

			// It is important to actually query for an Entity object here instead of a ViewModel
			// Because we need the title property which is a derived property that can't be done in Linq to Sql
			// And needs a variety of information from sub-tables, hence all the includes
			var results = await query.ToListAsync();

			return results.Select(s => new SubmissionListEntry
			{
				Id = s.Id,
				System = s.System.Code,
				GameName = s.GameName,
				Time = s.Time,
				Branch = s.Branch,
				Author = string.Join(" & ", s.SubmissionAuthors.Select(sa => sa.Author.UserName).ToList()),
				Submitted = s.CreateTimeStamp,
				Status = s.Status
			});
		}
	}
}
