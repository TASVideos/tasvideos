using TASVideos.Core;
using TASVideos.Data.Entity.Forum;

namespace TASVideos.Pages.Forum.Posts;

[AllowAnonymous]
public class LatestModel(ApplicationDbContext db) : BasePageModel
{
	[FromQuery]
	public PagingModel Search { get; set; } = new();

	public PageOf<LatestPost> Posts { get; set; } = PageOf<LatestPost>.Empty();

	public async Task OnGet()
	{
		var allowRestricted = User.Has(PermissionTo.SeeRestrictedForums);
		Posts = await db.ForumPosts
			.ExcludeRestricted(allowRestricted)
			.Since(DateTime.UtcNow.AddDays(-3))
			.Select(p => new LatestPost
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

	public class LatestPost
	{
		[Sortable]
		public DateTime CreateTimestamp { get; init; }
		public int Id { get; init; }
		public int TopicId { get; init; }
		public string TopicTitle { get; init; } = "";
		public int ForumId { get; init; }
		public string ForumName { get; init; } = "";
		public string Text { get; init; } = "";
		public string PosterName { get; init; } = "";
	}
}
