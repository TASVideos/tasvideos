using TASVideos.Data.Entity.Forum;

namespace TASVideos.Pages.Forum.Posts;

[AllowAnonymous]
public class LatestModel(ApplicationDbContext db) : BasePageModel
{
	[FromQuery]
	public PagingModel Search { get; set; } = new();

	public PageOf<LatestPost> Posts { get; set; } = new([], new());

	public async Task OnGet()
	{
		Posts = await db.ForumPosts
			.ExcludeRestricted(UserCanSeeRestricted)
			.Since(DateTime.UtcNow.AddDays(-3))
			.Select(p => p.TopicId)
			.Distinct()
			.Select(id => db.ForumPosts
				.ExcludeRestricted(UserCanSeeRestricted)
				.Where(p => p.TopicId == id)
				.OrderByDescending(p => p.CreateTimestamp)
				.First())
			.OrderByDescending(p => p.CreateTimestamp)
			.ToLatestPost()
			.PageOf(Search);
	}

	public record LatestPost(
		DateTime CreateTimestamp,
		int Id,
		int TopicId,
		string TopicTitle,
		int ForumId,
		string ForumName,
		string PosterName);
}
