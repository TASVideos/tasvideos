using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TASVideos.Core;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Data.Entity.Forum;

namespace TASVideos.Pages.Forum.Posts;

[AllowAnonymous]
public class UnansweredModel(ApplicationDbContext db) : BasePageModel
{
	[FromQuery]
	public PagingModel Search { get; set; } = new();

	public PageOf<UnansweredPostsModel> Posts { get; set; } = PageOf<UnansweredPostsModel>.Empty();

	public async Task OnGet()
	{
		Posts = await db.ForumTopics
			.ExcludeRestricted(User.Has(PermissionTo.SeeRestrictedForums))
			.Where(t => t.ForumPosts.Count == 1)
			.OrderByDescending(t => t.CreateTimestamp)
			.Select(t => new UnansweredPostsModel(
				t.ForumId,
				t.Forum!.Name,
				t.Id,
				t.Title,
				t.PosterId,
				t.Poster!.UserName,
				t.CreateTimestamp))
			.PageOf(Search);
	}

	public record UnansweredPostsModel(int ForumId, string ForumName, int TopicId, string TopicName, int AuthorId, string AuthorName, DateTime PostDate);
}
