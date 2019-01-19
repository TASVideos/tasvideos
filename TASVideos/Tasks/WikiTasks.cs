using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Models;
using TASVideos.Services;

namespace TASVideos.Tasks
{
	public class WikiTasks
	{
		private readonly ApplicationDbContext _db;
		private readonly IWikiPages _wikiPages;

		public WikiTasks(
			ApplicationDbContext db,
			IWikiPages wikiPages)
		{
			_db = db;
			_wikiPages = wikiPages;
		}

		/// <summary>
		/// Returns the data necessary to generate a diff of the
		/// latest revision of the wiki page with the given <see cref="pageName"/>
		/// compared against the previous revision
		/// </summary>
		/// <exception cref="InvalidOperationException">thrown if the given <see cref="pageName" /> does not exist</exception>
		public async Task<WikiDiffModel> GetLatestPageDiff(string pageName)
		{
			var revisions = await _db.WikiPages
				.ForPage(pageName)
				.ThatAreNotDeleted()
				.OrderByDescending(wp => wp.Revision)
				.Take(2)
				.ToListAsync();

			if (!revisions.Any())
			{
				throw new InvalidOperationException($"Page \"{pageName}\" could not be found");
			}

			// If count is 1, it must be a new page with no history, so compare against nothing
			if (revisions.Count == 1)
			{
				return new WikiDiffModel
				{
					PageName = pageName,
					LeftRevision = revisions.First().Revision - 1,
					LeftMarkup = "",
					RightRevision = revisions.First().Revision,
					RightMarkup = revisions.First().Markup
				};
			}
			
			return new WikiDiffModel
			{
				PageName = pageName,
				LeftRevision = revisions[1].Revision,
				LeftMarkup = revisions[1].Markup,
				RightRevision = revisions[0].Revision,
				RightMarkup = revisions[0].Markup
			};
		}

		/// <summary>
		/// Returns the data necessary to generate a diff between
		/// two revisions of the given page
		/// </summary>
		/// <exception cref="InvalidOperationException">If the given <see cref="pageName"/> does not exists
		/// or if the given <see cref="fromRevision"/> or <see cref="toRevision"/> do not exist
		/// </exception>
		public async Task<WikiDiffModel> GetPageDiff(string pageName, int fromRevision, int toRevision)
		{
			var revisions = await _db.WikiPages
				.ForPage(pageName)
				.Where(wp => wp.Revision == fromRevision
					|| wp.Revision == toRevision)
				.ToListAsync();

			if (fromRevision > 0 && revisions.Count != 2)
			{
				throw new InvalidOperationException($"Page \"{pageName}\" or revisions {fromRevision}-{toRevision} could not be found");
			}

			return new WikiDiffModel
			{
				PageName = pageName,
				LeftRevision = fromRevision,
				RightRevision = toRevision,
				LeftMarkup = fromRevision > 0 ? revisions.Single(wp => wp.Revision == fromRevision).Markup : "",
				RightMarkup = revisions.Single(wp => wp.Revision == toRevision).Markup
			};
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
