using TASVideos.Data.Entity.Forum;

namespace TASVideos.Pages.Forum.Posts;

[Authorize]
[RequireCurrentPermissions]
public class NewModel(ApplicationDbContext db, IUserManager userManager) : BasePageModel
{
	[FromQuery]
	public PagingModel Search { get; set; } = new();

	public PageOf<LatestModel.LatestPost> Posts { get; set; } = new([], new());

	public async Task OnGet()
	{
		var user = await userManager.GetRequiredUser(User);
		Posts = await db.ForumPosts
			.ExcludeRestricted(UserCanSeeRestricted)
			.Since(user.LastLoggedInTimeStamp ?? DateTime.UtcNow)
			.OrderByDescending(p => p.CreateTimestamp)
			.ToLatestPost()
			.PageOf(Search);
	}
}
