using TASVideos.WikiEngine;

namespace TASVideos.ViewComponents;

[WikiModule(WikiModules.WikiUsers)]
public class WikiUsers(ApplicationDbContext db) : ViewComponent
{
	public async Task<IViewComponentResult> InvokeAsync(string? role)
	{
		var model = await db.Users
			.ThatHaveRole(role ?? "")
			.Select(u => new WikiUserEntry
			{
				UserName = u.UserName,
				PublicationCount = u.Publications.Count,
				SubmissionCount = u.Submissions.Count
			})
			.ToListAsync();

		return View(model);
	}
}
