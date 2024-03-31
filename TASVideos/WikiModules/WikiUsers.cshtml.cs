using TASVideos.WikiEngine;

namespace TASVideos.WikiModules;

[WikiModule(ModuleNames.WikiUsers)]
public class WikiUsers(ApplicationDbContext db) : WikiViewComponent
{
	public List<WikiUserEntry> Users { get; set; } = [];

	public async Task<IViewComponentResult> InvokeAsync(string? role)
	{
		Users = await db.Users
			.ThatHaveRole(role ?? "")
			.Select(u => new WikiUserEntry
			{
				UserName = u.UserName,
				PublicationCount = u.Publications.Count,
				SubmissionCount = u.Submissions.Count
			})
			.ToListAsync();

		return View();
	}

	public class WikiUserEntry
	{
		public string UserName { get; init; } = "";
		public int SubmissionCount { get; init; }
		public int PublicationCount { get; init; }
	}
}
