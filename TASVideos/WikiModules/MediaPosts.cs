using TASVideos.Core.Services.ExternalMediaPublisher;
using TASVideos.WikiEngine;

namespace TASVideos.WikiModules;

[WikiModule(ModuleNames.MediaPosts)]
public class MediaPosts(ApplicationDbContext db) : ViewComponent
{
	public async Task<IViewComponentResult> InvokeAsync(int? days, int? limit)
	{
		var startDate = DateTime.UtcNow.AddDays(-(days ?? 7));
		var model = await GetPosts(startDate, limit ?? 50);

		return View(model);
	}

	public async Task<IEnumerable<MediaPost>> GetPosts(DateTime startDate, int limit)
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
