using TASVideos.WikiEngine;

namespace TASVideos.WikiModules;

[WikiModule(ModuleNames.UserMaintenanceLogs)]
public class UserMaintenanceLogs(ApplicationDbContext db) : WikiViewComponent
{
	public PageOf<LogEntry> Logs { get; set; } = new([], new());

	public async Task<IViewComponentResult> InvokeAsync()
	{
		if (!HttpContext.User.Has(PermissionTo.ViewPrivateUserData))
		{
			return Error("No access to this resource");
		}

		DefaultPageSize = 100;
		DefaultSort = "-TimeStamp";

		var logsQuery = db.UserMaintenanceLogs
			.Select(m => new LogEntry
			{
				User = m.User!.UserName,
				Editor = m.Editor!.UserName,
				TimeStamp = m.TimeStamp,
				Log = m.Log
			});

		var user = Request.QueryStringValue("User");
		if (!string.IsNullOrWhiteSpace(user))
		{
			logsQuery = logsQuery.Where(l => l.User == user);
		}

		Logs = await logsQuery.SortedPageOf(GetPaging());

		return View();
	}

	public class LogEntry
	{
		[Sortable]
		public string User { get; init; } = "";

		[Sortable]
		public string Editor { get; init; } = "";

		[Sortable]
		public DateTime TimeStamp { get; init; }

		[Sortable]
		public string Log { get; init; } = "";
	}
}
