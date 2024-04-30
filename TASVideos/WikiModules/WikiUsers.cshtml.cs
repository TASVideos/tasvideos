using TASVideos.WikiEngine;

namespace TASVideos.WikiModules;

[WikiModule(ModuleNames.WikiUsers)]
public class WikiUsers(ApplicationDbContext db) : WikiViewComponent
{
	public List<Entry> Users { get; set; } = [];

	public async Task<IViewComponentResult> InvokeAsync(string? role)
	{
		Users = await db.Users
			.ThatHaveRole(role ?? "")
			.Select(u => new Entry(
				u.UserName, u.Publications.Count, u.Submissions.Count))
			.ToListAsync();

		return View();
	}

	public record Entry(string UserName, int PublicationCount, int SubmissionCount);
}
