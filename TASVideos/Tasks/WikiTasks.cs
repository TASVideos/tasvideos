using System;
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
