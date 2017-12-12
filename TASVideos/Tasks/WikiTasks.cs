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
		/// </summary>
		/// <returns>A model representing the Wiki page if it exists else null</returns>
		public async Task<WikiPage> GetPage(string pageName) // TODO: ability to pass in a particular revision of a page
		{
			pageName = pageName?.Trim('/');
			return await _db.WikiPages
				.Where(wp => wp.PageName == pageName)
				.Where(wp => wp.Child == null)
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
	}
}
