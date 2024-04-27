using Microsoft.AspNetCore.Mvc.Rendering;
using TASVideos.Core.Services.ExternalMediaPublisher;
using TASVideos.Data.Entity.Forum;

namespace TASVideos.Pages.Forum.Topics;

[RequirePermission(PermissionTo.MergeTopics)]
public class MergeModel(ApplicationDbContext db, ExternalMediaPublisher publisher, IForumService forumService) : BasePageModel
{
	[FromRoute]
	public int Id { get; set; }

	[BindProperty]
	public TopicMerge Topic { get; set; } = new();

	public List<SelectListItem> AvailableForums { get; set; } = [];

	public List<SelectListItem> AvailableTopics { get; set; } = [];

	private bool CanSeeRestricted => User.Has(PermissionTo.SeeRestrictedForums);

	public async Task<IActionResult> OnGet()
	{
		bool seeRestricted = CanSeeRestricted;
		var topic = await db.ForumTopics
			.ExcludeRestricted(seeRestricted)
			.Where(t => t.Id == Id)
			.Select(t => new TopicMerge
			{
				Title = t.Title,
				ForumId = t.Forum!.Id,
				ForumName = t.Forum.Name
			})
			.SingleOrDefaultAsync();

		if (topic is null)
		{
			return NotFound();
		}

		Topic = topic;
		Topic.DestinationForumId = Topic.ForumId;
		await PopulateAvailableForums();

		return Page();
	}

	public async Task<IActionResult> OnPost()
	{
		if (Topic.DestinationTopicId == Id)
		{
			ModelState.AddModelError($"{nameof(Topic)}.{nameof(Topic.DestinationTopicId)}", "Cannot merge topic into itself!");
		}

		if (!ModelState.IsValid)
		{
			await PopulateAvailableForums();
			return Page();
		}

		var seeRestricted = CanSeeRestricted;
		var originalTopic = await db.ForumTopics
			.Include(f => f.Forum)
			.ExcludeRestricted(seeRestricted)
			.SingleOrDefaultAsync(t => t.Id == Id);

		if (originalTopic is null)
		{
			return NotFound();
		}

		var destinationTopic = await db.ForumTopics
			.Include(t => t.Forum)
			.ExcludeRestricted(seeRestricted)
			.SingleOrDefaultAsync(t => t.Id == Topic.DestinationTopicId);

		if (destinationTopic is null)
		{
			return NotFound();
		}

		var oldPosts = await db.ForumPosts
			.ForTopic(Id)
			.ToListAsync();

		foreach (var post in oldPosts)
		{
			post.TopicId = Topic.DestinationTopicId;
			post.ForumId = destinationTopic.ForumId;
		}

		db.ForumTopics.Remove(originalTopic);

		var result = await ConcurrentSave(db, $"Topic merged into {destinationTopic.Title}", "Unable to merge topic");
		if (result)
		{
			forumService.ClearLatestPostCache();
			forumService.ClearTopicActivityCache();
			await publisher.SendForum(
				originalTopic.Forum!.Restricted || destinationTopic.Forum!.Restricted,
				$"[Topics MERGED]({{0}}) by {User.Name()}",
				$"\"{originalTopic.Title}\" into \"{destinationTopic.Title}\"",
				$"Forum/Topics/{destinationTopic.Id}");
		}

		return RedirectToPage("Index", new { id = Topic.DestinationTopicId });
	}

	public async Task<IActionResult> OnGetTopicsForForum(int forumId)
	{
		var items = UiDefaults.DefaultEntry.Concat(await GetTopicsForForum(forumId));
		return ToDropdownResult(items);
	}

	private async Task PopulateAvailableForums()
	{
		var seeRestricted = CanSeeRestricted;
		AvailableForums = await db.Forums
			.ExcludeRestricted(seeRestricted)
			.ToDropdown(Topic.ForumId)
			.ToListAsync();

		AvailableTopics = (await GetTopicsForForum(Topic.ForumId)).WithDefaultEntry();
	}

	private async Task<List<SelectListItem>> GetTopicsForForum(int forumId)
	{
		var seeRestricted = CanSeeRestricted;
		return await db.ForumTopics
			.ExcludeRestricted(seeRestricted)
			.ForForum(forumId)
			.Where(t => t.Id != Id)
			.ToDropdown()
			.ToListAsync();
	}

	public class TopicMerge
	{
		public int ForumId { get; init; }
		public string ForumName { get; init; } = "";
		public string Title { get; init; } = "";

		[Display(Name = "Forum To Merge In to")]
		public int DestinationForumId { get; set; }

		[Display(Name = "Topic To Merge In to")]
		public int DestinationTopicId { get; init; }
	}
}
