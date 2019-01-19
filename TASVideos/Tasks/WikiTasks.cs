using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Models;

namespace TASVideos.Tasks
{
	public class WikiTasks
	{
		private readonly ApplicationDbContext _db;

		public WikiTasks(ApplicationDbContext db)
		{
			_db = db;
		}

		/// <summary>
		/// Returns a list of information about <see cref="WikiPage"/> entries
		/// ordered by timestamp with the given criteria
		/// </summary>
		public async Task<IEnumerable<WikiTextChangelogModel>> GetWikiChangeLog(int limit, bool includeMinorEdits)
		{
			var query = _db.WikiPages
				.ThatAreNotDeleted()
				.ByMostRecent()
				.Take(limit);

			if (!includeMinorEdits)
			{
				query = query.ExcludingMinorEdits();
			}

			return await query
				.Select(wp => new WikiTextChangelogModel
				{
					PageName = wp.PageName,
					Revision = wp.Revision,
					Author = wp.CreateUserName,
					CreateTimestamp = wp.CreateTimeStamp,
					MinorEdit = wp.MinorEdit,
					RevisionMessage = wp.RevisionMessage
				})
				.ToListAsync();
		}
	}
}
