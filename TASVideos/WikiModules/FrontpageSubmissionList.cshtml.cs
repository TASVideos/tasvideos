using TASVideos.Pages.Submissions.Models;
using TASVideos.WikiEngine;

namespace TASVideos.WikiModules;

[WikiModule(ModuleNames.FrontpageSubmissionList)]
public class FrontpageSubmissionList(ApplicationDbContext db) : WikiViewComponent
{
	public List<SubmissionListEntry> Subs { get; set; } = [];

	public async Task<IViewComponentResult> InvokeAsync(int? limit)
	{
		// Legacy system supported a max days value, which isn't easily translated to the current filtering
		// However, we currently have it set to 365 which greatly exceeds any max number
		// And submissions are frequent enough to not worry about too stale submissions showing up on the front page
		var request = new SubmissionSearchRequest();

		Subs = await db.Submissions
			.ThatAreActive()
			.FilterBy(request)
			.ByMostRecent()
			.Take(limit ?? 5)
			.ToSubListEntry()
			.ToListAsync();

		return View();
	}
}
