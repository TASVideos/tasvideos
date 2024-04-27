﻿using Microsoft.AspNetCore.Mvc.Rendering;
using TASVideos.Core.Services.ExternalMediaPublisher;
using TASVideos.Data.Entity.Forum;

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
	public TopicMove Topic { get; set; } = new();

	public List<SelectListItem> AvailableForums { get; set; } = [];

	public bool CanSeeRestricted => User.Has(PermissionTo.SeeRestrictedForums);

	public async Task<IActionResult> OnGet()
	{
		var seeRestricted = CanSeeRestricted;

		var topic = await db.ForumTopics
			.Where(t => t.Id == Id)
			.Include(t => t.Forum)
			.ExcludeRestricted(seeRestricted)
			.Select(t => new TopicMove
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
			$"[Topic]({{0}}) MOVED by {User.Name()}",
			$"\"{Topic.TopicTitle}\" from {Topic.ForumName} to {forum.Name}",
			$"Forum/Topics/{Id}");

		return RedirectToPage("Index", new { Id });
	}

	private async Task PopulateAvailableForums()
	{
		AvailableForums = await db.Forums
			.ExcludeRestricted(CanSeeRestricted)
			.ToDropdown(Topic.ForumId)
			.ToListAsync();
	}

	public class TopicMove
	{
		[Display(Name = "New Forum")]
		public int ForumId { get; init; }

		[Display(Name = "Topic")]
		public string TopicTitle { get; init; } = "";

		[Display(Name = "Current Forum")]
		public string ForumName { get; init; } = "";
	}
}
