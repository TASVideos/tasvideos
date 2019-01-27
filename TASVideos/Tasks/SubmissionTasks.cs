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
		/// Gets a list of <see cref="Submission"/>s for the submission queue filtered on the given <see cref="criteria" />
		/// </summary>
		public async Task<SubmissionListModel> GetSubmissionList(SubmissionSearchRequest criteria)
		{
			var iquery = _db.Submissions
				.Include(s => s.Submitter)
				.Include(s => s.System)
				.Include(s => s.SystemFrameRate)
				.Include(s => s.SubmissionAuthors)
				.ThenInclude(sa => sa.Author);

			IQueryable<Submission> query = iquery.AsQueryable();

			if (!string.IsNullOrWhiteSpace(criteria.User))
			{
				query = iquery.Where(s => s.SubmissionAuthors.Any(sa => sa.Author.UserName == criteria.User)
					|| s.Submitter.UserName == criteria.User);
			}

			if (criteria.Cutoff.HasValue)
			{
				query = query.Where(s => s.CreateTimeStamp >= criteria.Cutoff.Value);
			}

			if (criteria.StatusFilter.Any())
			{
				query = query.Where(s => criteria.StatusFilter.Contains(s.Status));
			}

			if (criteria.Limit.HasValue)
			{
				query = query.Take(criteria.Limit.Value);
			}

			// It is important to actually query for an Entity object here instead of a ViewModel
			// Because we need the title property which is a derived property that can't be done in Linq to Sql
			// And needs a variety of information from sub-tables, hence all the includes
			var results = await query.ToListAsync();
			return new SubmissionListModel
			{
				User = criteria.User,
				StatusFilter = criteria.StatusFilter
					.Cast<int>()
					.ToList(),
				Entries = results.Select(s => new SubmissionListModel.Entry
				{
					Id = s.Id,
					System = s.System.Code,
					GameName = s.GameName,
					Time = s.Time,
					Branch = s.Branch,
					Author = string.Join(" & ", s.SubmissionAuthors.Select(sa => sa.Author.UserName).ToList()),
					Submitted = s.CreateTimeStamp,
					Status = s.Status
				}) 
			};
		}

		/// <summary>
		/// Gets the title of a submission with the given id
		/// If the submission is not found, null is returned
		/// </summary>
		public async Task<string> GetTitle(int id)
		{
			return (await _db.Submissions
				.Select(s => new { s.Id, s.Title })
				.SingleOrDefaultAsync(s => s.Id == id))?.Title;
		}
	}
}
