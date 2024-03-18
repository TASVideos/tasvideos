using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using TASVideos.Core.Services;
using TASVideos.Core.Services.ExternalMediaPublisher;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Data.Entity.Forum;
using TASVideos.Pages.Forum.Topics.Models;

namespace TASVideos.Pages.Forum.Topics;

[RequirePermission(PermissionTo.MergeTopics)]
public class MergeModel(
	ApplicationDbContext db,
	ExternalMediaPublisher publisher,
	IForumService forumService)
	: BasePageModel
{
	[FromRoute]
	public int Id { get; set; }

	[BindProperty]
	public MergeTopicModel Topic { get; set; } = new();

	public IEnumerable<SelectListItem> AvailableForums { get; set; } = [];

	public IEnumerable<SelectListItem> AvailableTopics { get; set; } = [];

	private bool CanSeeRestricted => User.Has(PermissionTo.SeeRestrictedForums);

	public async Task<IActionResult> OnGet()
	{
		bool seeRestricted = CanSeeRestricted;
		var topic = await db.ForumTopics
			.ExcludeRestricted(seeRestricted)
			.Where(t => t.Id == Id)
			.Select(t => new MergeTopicModel
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
				$"Topics MERGED by {User.Name()}",
				$"[Topics MERGED]({{0}}) by {User.Name()}",
				$@"""{originalTopic.Title}"" into ""{destinationTopic.Title}""",
				$"Forum/Topics/{destinationTopic.Id}");
		}

		return RedirectToPage("Index", new { id = Topic.DestinationTopicId });
	}

	public async Task<IActionResult> OnGetTopicsForForum(int forumId)
	{
		var items = UiDefaults.DefaultEntry.Concat(await GetTopicsForForum(forumId));
		return new PartialViewResult
		{
			ViewName = "_DropdownItems",
			ViewData = new ViewDataDictionary<IEnumerable<SelectListItem>>(ViewData, items)
		};
	}

	private async Task PopulateAvailableForums()
	{
		var seeRestricted = CanSeeRestricted;
		AvailableForums = await db.Forums
			.ExcludeRestricted(seeRestricted)
			.Select(f => new SelectListItem
			{
				Text = f.Name,
				Value = f.Id.ToString(),
				Selected = f.Id == Topic.ForumId
			})
			.ToListAsync();

		AvailableTopics = UiDefaults.DefaultEntry.Concat(await GetTopicsForForum(Topic.ForumId));
	}

	private async Task<IEnumerable<SelectListItem>> GetTopicsForForum(int forumId)
	{
		var seeRestricted = CanSeeRestricted;
		return await db.ForumTopics
			.ExcludeRestricted(seeRestricted)
			.ForForum(forumId)
			.Where(t => t.Id != Id)
			.Select(t => new SelectListItem
			{
				Text = t.Title,
				Value = t.Id.ToString()
			})
			.ToListAsync();
	}
}
