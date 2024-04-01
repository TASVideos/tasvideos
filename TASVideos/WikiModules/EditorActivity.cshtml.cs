using TASVideos.WikiEngine;

namespace TASVideos.WikiModules;

[WikiModule(ModuleNames.EditorActivity)]
public class EditorActivity(ApplicationDbContext db) : WikiViewComponent
{
	public List<Entry> Edits { get; set; } = [];

	public async Task<IViewComponentResult> InvokeAsync()
	{
		Edits = await db.WikiPages
			.ThatAreNotDeleted()
			.GroupBy(g => g.Author!.UserName)
			.Select(w => new Entry
			{
				UserName = w.Key,
				WikiEdits = w.Count()
			})
			.OrderByDescending(m => m.WikiEdits)
			.Take(30)
			.ToListAsync();

		return View();
	}

	public class Entry
	{
		public string UserName { get; init; } = "";
		public int WikiEdits { get; init; }
	}
}
