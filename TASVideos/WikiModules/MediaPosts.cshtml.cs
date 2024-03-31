using TASVideos.Core.Services.ExternalMediaPublisher;
using TASVideos.WikiEngine;

namespace TASVideos.WikiModules;

[WikiModule(ModuleNames.MediaPosts)]
public class MediaPosts(ApplicationDbContext db) : WikiViewComponent
{
	public List<MediaPost> Posts { get; set; } = [];

	public async Task<IViewComponentResult> InvokeAsync(int? days, int? limit)
	{
		var startDate = DateTime.UtcNow.AddDays(-(days ?? 7));
		Posts = await GetPosts(startDate, limit ?? 50);

		return View();
	}

	public async Task<List<MediaPost>> GetPosts(DateTime startDate, int limit)
	{
		var canSeeRestricted = UserClaimsPrincipal.Has(PermissionTo.SeeRestrictedForums);

		return await db.MediaPosts
			.Since(startDate)
			.Where(m => canSeeRestricted || m.Type != PostType.Critical.ToString())
			.Where(m => canSeeRestricted || m.Type != PostType.Administrative.ToString())
			.Where(m => canSeeRestricted || m.Type != PostType.Log.ToString())
			.ByMostRecent()
			.Take(limit)
			.ToListAsync();
	}
}
