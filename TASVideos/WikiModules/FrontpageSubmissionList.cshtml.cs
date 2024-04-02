using TASVideos.Pages.Submissions;
using TASVideos.WikiEngine;

namespace TASVideos.WikiModules;

[WikiModule(ModuleNames.FrontpageSubmissionList)]
public class FrontpageSubmissionList(ApplicationDbContext db) : WikiViewComponent
{
	public List<IndexModel.SubmissionEntry> Subs { get; set; } = [];

	public async Task<IViewComponentResult> InvokeAsync(int? limit)
	{
		Subs = await db.Submissions
			.ThatAreActive()
			.FilterBy(new IndexModel.SubmissionSearchRequest())
			.ByMostRecent()
			.Take(limit ?? 5)
			.ToSubListEntry()
			.ToListAsync();

		return View();
	}
}
