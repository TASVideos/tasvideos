using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using TASVideos.Core;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.WikiEngine;

namespace TASVideos.ViewComponents;

[WikiModule(WikiModules.UserMaintenanceLogs)]
public class UserMaintenanceLogs : ViewComponent
{
	private readonly ApplicationDbContext _db;

	public UserMaintenanceLogs(ApplicationDbContext db)
	{
		_db = db;
	}

	public async Task<IViewComponentResult> InvokeAsync()
	{
		if (!HttpContext.User.Has(PermissionTo.ViewPrivateUserData))
		{
			return new ContentViewComponentResult("No access to this resource");
		}

		var paging = new PagingModel
		{
			Sort = HttpContext.Request.QueryStringValue("Sort"),
			PageSize = HttpContext.Request.QueryStringIntValue("PageSize") ?? 25,
			CurrentPage = HttpContext.Request.QueryStringIntValue("CurrentPage") ?? 1
		};

		if (string.IsNullOrWhiteSpace(paging.Sort))
		{
			paging.Sort = "-TimeStamp";
		}

		var logs = await _db.UserMaintenanceLogs
			.Select(m => new UserMaintenanceLogEntry
			{
				User = m.User!.UserName,
				Editor = m.Editor!.UserName,
				TimeStamp = m.TimeStamp,
				Log = m.Log
			})
			.SortedPageOf(paging);

		ViewData["PagingModel"] = paging;
		ViewData["CurrentPage"] = HttpContext.Request.Path.Value;
		return View(logs);
	}
}
