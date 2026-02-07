using TASVideos.Core.Services.ExternalMediaPublisher;
using TASVideos.WikiEngine;

namespace TASVideos.WikiModules;

[WikiModule(ModuleNames.MediaPosts)]
public class MediaPosts(ApplicationDbContext db) : WikiViewComponent
{
	public PageOf<MediaPost> Posts { get; set; } = new([], new());

	public async Task<IViewComponentResult> InvokeAsync()
	{
		var canSeeRestricted = UserClaimsPrincipal.Has(PermissionTo.SeeRestrictedForums);

		Posts = await db.MediaPosts
			.Where(m => canSeeRestricted || m.Type != nameof(PostType.Critical))
			.Where(m => canSeeRestricted || m.Type != nameof(PostType.Administrative))
			.Where(m => canSeeRestricted || m.Type != nameof(PostType.Log))
			.ByMostRecent()
			.PageOf(GetPaging());

		return View();
	}
}
