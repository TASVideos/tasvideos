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
		/// Returns details about a Wiki page with the given <see cref="pageName" />
		/// If a <see cref="revisionId" /> is provided then that revision of the page will be returned
		/// Else the latest revision is returned
		/// </summary>
		/// <returns>A model representing the Wiki page if it exists else null</returns>
		public async Task<WikiPage> GetPage(string pageName, int? revisionId = null) // TODO: ability to pass in a particular revision of a page
		{
			pageName = pageName?.Trim('/');
			return await _db.WikiPages
				.Where(wp => wp.PageName == pageName)
				.Where(wp => revisionId != null
					? wp.Revision == revisionId
					: wp.Child == null)
				.SingleOrDefaultAsync();
		}

		/// <summary>
		/// Returns details about a Wiki page with the given id
		/// </summary>
		/// <returns>A model representing the Wiki page if it exists else null</returns>
		public async Task<WikiPage> GetPage(int dbid)
		{
			return await _db.WikiPages
				.Where(wp => wp.Id == dbid)
				.SingleOrDefaultAsync();
		}

		/// <summary>
		/// Returns whether or not any revision of the given page exists
		/// </summary>
		public async Task<bool> PageExists(string pageName)
		{
			return await _db.WikiPages
				.AnyAsync(wp => wp.PageName == pageName);
		}

		// TODO: document
		public async Task SavePage(WikiEditModel model)
		{
			model.PageName = model.PageName.Trim('/');

			// TODO: check if the user is allowed to make a page like this,
			// Mainly check that it doesn't hit existing controller names
			var newRevision = new WikiPage
			{
				PageName = model.PageName,
				Markup = model.Markup,
				MinorEdit = model.MinorEdit,
				RevisionMessage = model.RevisionMessage
			};

			_db.WikiPages.Add(newRevision);

			var currentRevision = await _db.WikiPages
				.Where(wp => wp.PageName == model.PageName)
				.Where(wp => wp.Child == null)
				.SingleOrDefaultAsync();

			if (currentRevision != null)
			{
				currentRevision.Child = newRevision;
				newRevision.Revision = currentRevision.Revision + 1;
			}

			await _db.SaveChangesAsync();
		}

		// TODO: document
		public async Task<WikiHistoryModel> GetPageHistory(string pageName)
		{
			pageName = pageName.Trim('/');

			return new WikiHistoryModel
			{
				PageName = pageName,
				Revisions = await _db.WikiPages
					.Where(wp => wp.PageName == pageName)
					.OrderBy(wp => wp.Revision)
					.Select(wp => new WikiHistoryModel.WikiRevisionModel
					{
						Revision = wp.Revision,
						CreateTimeStamp = wp.CreateTimeStamp,
						CreateUserName = wp.CreateUserName,
						MinorEdit = wp.MinorEdit,
						RevisionMessage = wp.RevisionMessage
					})
					.ToListAsync()
			};
		}

		// TODO: document
		public async Task MovePage(WikiMoveModel model)
		{
			var existingRevisions = await _db.WikiPages
				.Where(wp => wp.PageName == model.OriginalPageName)
				.ToListAsync();

			foreach (var revision in existingRevisions)
			{
				revision.PageName = model.DestinationPageName;
			}

			await _db.SaveChangesAsync();
		}

		// TODO: document
		public async Task<IEnumerable<string>> GetSubPages(string pageName)
		{
			pageName = pageName.Trim('/');
			return await _db.WikiPages
				.ThatAreCurrentRevisions()
				.Where(wp => wp.PageName != pageName)
				.Where(wp => wp.PageName.StartsWith(pageName))
				.Select(wp => wp.PageName)
				.ToListAsync();
		}

		// TODO: document
		// Gets all pages that refer to the given page
		public async Task<IEnumerable<WikiPageReferral>> GetReferrers(string pageName)
		{
			pageName = pageName.Trim('/');
			return await _db.WikiReferrals
				.Where(wr => wr.Referral == pageName)
				.ToListAsync();
		}
	}
}
