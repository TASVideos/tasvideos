﻿using Microsoft.AspNetCore.Mvc;
using TASVideos.Core;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.WikiEngine;

namespace TASVideos.ViewComponents;

[WikiModule(WikiModules.WikiTextChangeLog)]
public class WikiTextChangeLog : ViewComponent
{
	private readonly ApplicationDbContext _db;

	public WikiTextChangeLog(ApplicationDbContext db)
	{
		_db = db;
	}

	public async Task<IViewComponentResult> InvokeAsync(bool includeMinors)
	{
		var paging = this.GetPagingModel(100);
		var results = await GetWikiChangeLog(paging, includeMinors);
		this.SetPagingToViewData(paging);
		return View(results);
	}

	private async Task<PageOf<WikiTextChangelogModel>> GetWikiChangeLog(PagingModel paging, bool includeMinorEdits)
	{
		var query = _db.WikiPages
			.ThatAreNotDeleted()
			.ByMostRecent();

		if (!includeMinorEdits)
		{
			query = query.ExcludingMinorEdits();
		}

		return await query
			.Select(wp => new WikiTextChangelogModel
			{
				PageName = wp.PageName,
				Revision = wp.Revision,
				Author = wp.Author!.UserName,
				CreateTimestamp = wp.CreateTimestamp,
				MinorEdit = wp.MinorEdit,
				RevisionMessage = wp.RevisionMessage
			})
			.PageOf(paging);
	}
}
