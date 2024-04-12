using TASVideos.WikiEngine;

namespace TASVideos.WikiModules;

[WikiModule(ModuleNames.UncatalogedTopics)]
public class UncatalogedTopics(ApplicationDbContext db) : WikiViewComponent
{
	public List<UncatalogedTopic> Topics { get; set; } = [];

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

		Topics = await query
			.Select(t => new UncatalogedTopic(t.Id, t.Title, t.Forum!.Name))
			.ToListAsync();
		return View();
	}

	public record UncatalogedTopic(int Id, string Title, string ForumName);
}
