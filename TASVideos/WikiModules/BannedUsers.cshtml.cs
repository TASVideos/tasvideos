using TASVideos.WikiEngine;

namespace TASVideos.WikiModules;

[WikiModule(ModuleNames.BannedUsers)]
public class BannedUsers(ApplicationDbContext db) : WikiViewComponent
{
	public PageOf<Entry> Users { get; set; } = new([], new());

	public async Task<IViewComponentResult> InvokeAsync()
	{
		Users = await db.Users
			.ThatAreBanned()
			.OrderBy(u => u.UserName)
			.Select(u => new Entry
			{
				Name = u.UserName,
				BannedUntil = u.BannedUntil!.Value,
				LastLoggedIn = u.LastLoggedInTimeStamp,
				ModeratorComments = u.ModeratorComments,
			})
			.SortedPageOf(GetPaging());
		return View();
	}

	public class Entry
	{
		[Sortable]
		public string Name { get; init; } = "";

		[Sortable]
		public DateTime BannedUntil { get; init; }

		[Sortable]
		public DateTime? LastLoggedIn { get; init; }

		public string? ModeratorComments { get; init; }
	}
}
