using TASVideos.ViewComponents.Models;
using TASVideos.WikiEngine;

namespace TASVideos.ViewComponents.TODO;

[WikiModule(WikiModules.UncatalogedTopics)]
public class UncatalogedTopics(ApplicationDbContext db) : ViewComponent
{
	public async Task<IViewComponentResult> InvokeAsync()
	{
		int? forumId = null;
		string forumIdStr = HttpContext.Request.QueryStringValue("forumId");
		if (int.TryParse(forumIdStr, out var f))
		{
			forumId = f;
		}

		var query = db.ForumTopics
			.Where(t => t.Forum!.Category!.Id == SiteGlobalConstants.GamesForumCategory)
			.Where(t => t.ForumId != SiteGlobalConstants.OtherGamesForum)
			.Where(t => !t.Title.ToLower().Contains("wishlist"))
			.Where(t => t.GameId == null);

		if (forumId.HasValue)
		{
			query = query.Where(t => t.ForumId == forumId.Value);
		}

		var topics = await query
			.Select(t => new UncatalogedTopic
			{
				Id = t.Id,
				Title = t.Title,
				ForumId = t.Forum!.Id,
				ForumName = t.Forum.Name
			})
			.ToListAsync();
		return View(topics);
	}
}
