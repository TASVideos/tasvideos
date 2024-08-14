using TASVideos.Data.Entity.Forum;

namespace TASVideos.Pages.Forum.Posts;

[AllowAnonymous]
public class UnansweredModel(ApplicationDbContext db) : BasePageModel
{
	[FromQuery]
	public PagingModel Search { get; set; } = new();

	public PageOf<UnansweredPosts> Posts { get; set; } = new([], new());

	public async Task OnGet()
	{
		Posts = await db.ForumTopics
			.ExcludeRestricted(UserCanSeeRestricted)
			.Where(t => t.ForumPosts.Count == 1)
			.OrderByDescending(t => t.CreateTimestamp)
			.Select(t => new UnansweredPosts(
				t.ForumId,
				t.Forum!.Name,
				t.Id,
				t.Title,
				t.PosterId,
				t.Poster!.UserName,
				t.CreateTimestamp))
			.PageOf(Search);
	}

	public record UnansweredPosts(int ForumId, string ForumName, int TopicId, string TopicName, int AuthorId, string AuthorName, DateTime PostDate);
}
