using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TASVideos.Core.Services;
using TASVideos.Core.Services.ExternalMediaPublisher;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Data.Entity.Forum;
using TASVideos.Pages.Forum.Topics.Models;

namespace TASVideos.Pages.Forum.Topics;

[RequirePermission(PermissionTo.MoveTopics)]
public class MoveModel(
	ApplicationDbContext db,
	ExternalMediaPublisher publisher,
	IForumService forumService)
	: BasePageModel
{
	[FromRoute]
	public int Id { get; set; }

	[BindProperty]
	public MoveTopicModel Topic { get; set; } = new();

	public IEnumerable<SelectListItem> AvailableForums { get; set; } = [];

	public bool CanSeeRestricted => User.Has(PermissionTo.SeeRestrictedForums);

	public async Task<IActionResult> OnGet()
	{
		var seeRestricted = CanSeeRestricted;

		var topic = await db.ForumTopics
			.Where(t => t.Id == Id)
			.Include(t => t.Forum)
			.ExcludeRestricted(seeRestricted)
			.Select(t => new MoveTopicModel
			{
				TopicTitle = t.Title,
				ForumId = t.Forum!.Id,
				ForumName = t.Forum.Name
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

		var seeRestricted = CanSeeRestricted;
		var topic = await db.ForumTopics
			.Include(t => t.Forum)
			.ExcludeRestricted(seeRestricted)
			.SingleOrDefaultAsync(t => t.Id == Id);

		if (topic is null)
		{
			return NotFound();
		}

		var forum = await db.Forums.SingleOrDefaultAsync(f => f.Id == Topic.ForumId);

		if (forum is null)
		{
			return NotFound();
		}

		var topicWasRestricted = topic.Forum?.Restricted ?? false;
		topic.ForumId = Topic.ForumId;

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
			$"Topic MOVED by {User.Name()}",
			$"[Topic]({{0}}) MOVED by {User.Name()}",
			$@"""{Topic.TopicTitle}"" from {Topic.ForumName} to {forum.Name}",
			$"Forum/Topics/{Id}");

		return RedirectToPage("Index", new { Id });
	}

	private async Task PopulateAvailableForums()
	{
		AvailableForums = await db.Forums
			.ExcludeRestricted(CanSeeRestricted)
			.Select(f => new SelectListItem
			{
				Text = f.Name,
				Value = f.Id.ToString(),
				Selected = f.Id == Topic.ForumId
			})
			.ToListAsync();
	}
}
