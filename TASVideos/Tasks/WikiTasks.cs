using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Data.SeedData;
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

		// TODO: document
		// returns null if a revision of this page is not found
		public async Task<WikiViewModel> GetPage(string pageName) // TODO: ability to pass in a particular revision of a page
		{
			pageName = pageName?.Trim('/');
			var existingPage = await _db.WikiPages
				.Where(wp => wp.PageName == pageName)
				.Where(wp => wp.Child == null)
				.SingleOrDefaultAsync();

			if (existingPage != null)
			{
				return new WikiViewModel
				{
					PageName = existingPage.PageName,
					Markup = existingPage.Markup,
					DbId = existingPage.Id
				};
			}

			return null;
		}
		public async Task<WikiViewModel> GetPage(int dbid)
		{
			var existingPage = await _db.WikiPages
				.Where(wp => wp.Id == dbid)
				.SingleOrDefaultAsync();
			
			if (existingPage != null)
			{
				return new WikiViewModel
				{
					PageName = existingPage.PageName,
					Markup = existingPage.Markup,
					DbId = existingPage.Id
				};
			}

			return null;
		}

		public async Task<WikiViewModel> GetPageNotFoundPage()
		{
			string pageName = WikiPageSeedData.PageNotFound; // TODO: make this a const somewhere
			var page = await GetPage(pageName);
			if (page == null)
			{
				throw new InvalidOperationException("DRAGONS!");
			}

			return page;
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
			}

			await _db.SaveChangesAsync();
		}
	}
}
