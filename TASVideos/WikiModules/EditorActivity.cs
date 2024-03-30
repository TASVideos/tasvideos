using TASVideos.WikiEngine;

namespace TASVideos.WikiModules;

[WikiModule(ModuleNames.EditorActivity)]
public class EditorActivity(ApplicationDbContext db) : ViewComponent
{
	public async Task<IViewComponentResult> InvokeAsync()
	{
		// Legacy system supported a max days value, which isn't easily translated to the current filtering
		// However, we currently have it set to 365 which greatly exceeds any max number
		// And submissions are frequent enough to not worry about too stale submissions showing up on the front page
		var subs = await db.WikiPages
			.ThatAreNotDeleted()
			.GroupBy(g => g.Author!.UserName)
			.Select(w => new EditorActivityModel
			{
				UserName = w.Key,
				WikiEdits = w.Count()
			})
			.OrderByDescending(m => m.WikiEdits)
			.Take(30)
			.ToListAsync();

		return View(subs);
	}
}
