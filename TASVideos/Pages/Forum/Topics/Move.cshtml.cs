using TASVideos.Data.Entity.Forum;

namespace TASVideos.Pages.Forum.Topics;

[RequirePermission(PermissionTo.MoveTopics)]
public class MoveModel(
	ApplicationDbContext db,
	IExternalMediaPublisher publisher,
	IForumService forumService)
	: BasePageModel
{
	[FromRoute]
	public int Id { get; set; }

	[BindProperty]
	public TopicMove Topic { get; set; } = new();

	public List<SelectListItem> AvailableForums { get; set; } = [];

	public async Task<IActionResult> OnGet()
	{
		var topic = await db.ForumTopics
			.Where(t => t.Id == Id)
			.Include(t => t.Forum)
			.ExcludeRestricted(UserCanSeeRestricted)
			.Select(t => new TopicMove
			{
				Topic = t.Title,
				NewForum = t.Forum!.Id,
				CurrentForum = t.Forum.Name
			})
			.SingleOrDefaultAsync();

		if (topic is null)
		{
			return NotFound();
		}

		Topic = topic;
		await PopulateAvailableForums();

		return Page();
	}

	public async Task<IActionResult> OnPost()
	{
		if (!ModelState.IsValid)
		{
			await PopulateAvailableForums();
			return Page();
		}

		var topic = await db.ForumTopics
			.Include(t => t.Forum)
			.ExcludeRestricted(UserCanSeeRestricted)
			.SingleOrDefaultAsync(t => t.Id == Id);

		if (topic is null)
		{
			return NotFound();
		}

		var forum = await db.Forums.SingleOrDefaultAsync(f => f.Id == Topic.NewForum);

		if (forum is null)
		{
			return NotFound();
		}

		var topicWasRestricted = topic.Forum?.Restricted ?? false;
		topic.ForumId = Topic.NewForum;

		var postsToMove = await db.ForumPosts
			.ForTopic(topic.Id)
			.ToListAsync();

		foreach (var post in postsToMove)
		{
			post.ForumId = forum.Id;
		}

		await db.SaveChangesAsync();

		forumService.ClearLatestPostCache();
		forumService.ClearTopicActivityCache();

		await publisher.SendForum(
			topicWasRestricted || forum.Restricted,
			$"[Topic]({{0}}) MOVED by {User.Name()}",
			$"\"{Topic.Topic}\" from {Topic.CurrentForum} to {forum.Name}",
			$"Forum/Topics/{Id}");

		return RedirectToPage("Index", new { Id });
	}

	private async Task PopulateAvailableForums()
	{
		AvailableForums = await db.Forums.ToDropdownList(UserCanSeeRestricted, Topic.NewForum);
	}

	public class TopicMove
	{
		public int NewForum { get; init; }
		public string Topic { get; init; } = "";
		public string CurrentForum { get; init; } = "";
	}
}
