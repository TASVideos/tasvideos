using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TASVideos.Core.Services;
using TASVideos.Core.Services.ExternalMediaPublisher;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Data.Entity.Forum;
using TASVideos.Pages.Forum.Topics.Models;

namespace TASVideos.Pages.Forum.Topics;

[RequirePermission(PermissionTo.MoveTopics)]
public class MoveModel : BasePageModel
{
	private readonly ApplicationDbContext _db;
	private readonly ExternalMediaPublisher _publisher;
	private readonly IForumService _forumService;

	public MoveModel(
		ApplicationDbContext db,
		ExternalMediaPublisher publisher,
		IForumService forumService)
	{
		_db = db;
		_publisher = publisher;
		_forumService = forumService;
	}

	[FromRoute]
	public int Id { get; set; }

	[BindProperty]
	public MoveTopicModel Topic { get; set; } = new();

	public IEnumerable<SelectListItem> AvailableForums { get; set; } = new List<SelectListItem>();

	public bool CanSeeRestricted => User.Has(PermissionTo.SeeRestrictedForums);

	public async Task<IActionResult> OnGet()
	{
		var seeRestricted = CanSeeRestricted;

		var topic = await _db.ForumTopics
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
		var topic = await _db.ForumTopics
			.Include(t => t.Forum)
			.ExcludeRestricted(seeRestricted)
			.SingleOrDefaultAsync(t => t.Id == Id);

		if (topic is null)
		{
			return NotFound();
		}

		var forum = await _db.Forums.SingleOrDefaultAsync(f => f.Id == Topic.ForumId);

		if (forum is null)
		{
			return NotFound();
		}

		var topicWasRestricted = topic.Forum?.Restricted ?? false;
		topic.ForumId = Topic.ForumId;

		var postsToMove = await _db.ForumPosts
			.ForTopic(topic.Id)
			.ToListAsync();

		foreach (var post in postsToMove)
		{
			post.ForumId = forum.Id;
		}

		await _db.SaveChangesAsync();

		_forumService.ClearLatestPostCache();
		_forumService.ClearTopicActivityCache();

		await _publisher.SendForum(
			topicWasRestricted || forum.Restricted,
			$"Topic MOVED by {User.Name()}",
			$@"""{Topic.TopicTitle}"" from {Topic.ForumName} to {forum.Name}",
			$"Forum/Topics/{Id}");

		return RedirectToPage("Index", new { Id });
	}

	private async Task PopulateAvailableForums()
	{
		AvailableForums = await _db.Forums
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
