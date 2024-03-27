using TASVideos.Core;
using TASVideos.Data.Entity.Forum;
using TASVideos.Pages.Forum.Posts.Models;

namespace TASVideos.Pages.Forum.Posts;

[AllowAnonymous]
public class LatestModel(ApplicationDbContext db) : BasePageModel
{
	[FromQuery]
	public PagingModel Search { get; set; } = new();

	public PageOf<LatestPostsModel> Posts { get; set; } = PageOf<LatestPostsModel>.Empty();

	public async Task OnGet()
	{
		var allowRestricted = User.Has(PermissionTo.SeeRestrictedForums);
		Posts = await db.ForumPosts
			.ExcludeRestricted(allowRestricted)
			.Since(DateTime.UtcNow.AddDays(-3))
			.Select(p => new LatestPostsModel
			{
				Id = p.Id,
				CreateTimestamp = p.CreateTimestamp,
				Text = p.Text,
				TopicId = p.TopicId ?? 0,
				TopicTitle = p.Topic!.Title,
				ForumId = p.Topic.ForumId,
				ForumName = p.Topic!.Forum!.Name,
				PosterName = p.Poster!.UserName
			})
			.OrderByDescending(p => p.CreateTimestamp)
			.PageOf(Search);
	}
}
