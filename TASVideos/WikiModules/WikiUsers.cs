using TASVideos.WikiEngine;

namespace TASVideos.WikiModules;

[WikiModule(ModuleNames.WikiUsers)]
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

	public class WikiUserEntry
	{
		public string UserName { get; init; } = "";
		public int SubmissionCount { get; init; }
		public int PublicationCount { get; init; }
	}
}
