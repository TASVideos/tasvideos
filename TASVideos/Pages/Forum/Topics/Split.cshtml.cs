using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TASVideos.Core.Services;
using TASVideos.Core.Services.ExternalMediaPublisher;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Data.Entity.Forum;
using TASVideos.Pages.Forum.Topics.Models;

namespace TASVideos.Pages.Forum.Topics;

[RequirePermission(PermissionTo.SplitTopics)]
public class SplitModel(
	ApplicationDbContext db,
	ExternalMediaPublisher publisher,
	IForumService forumService)
	: BasePageModel
{
	[FromRoute]
	public int Id { get; set; }

	[BindProperty]
	public SplitTopicModel Topic { get; set; } = new();

	public IEnumerable<SelectListItem> AvailableForums { get; set; } = new List<SelectListItem>();

	private bool CanSeeRestricted => User.Has(PermissionTo.SeeRestrictedForums);

	public async Task<IActionResult> OnGet()
	{
		var splitTopic = await PopulatePosts();
		if (splitTopic is null)
		{
			return NotFound();
		}

		Topic = splitTopic;
		await PopulateAvailableForums();
		return Page();
	}

	public async Task<IActionResult> OnPost()
	{
		if (!ModelState.IsValid)
		{
			await PopulatePosts();
			await PopulateAvailableForums();
			return Page();
		}

		bool seeRestricted = CanSeeRestricted;
		var topic = await db.ForumTopics
			.Include(t => t.Forum)
			.Include(t => t.ForumPosts)
			.ExcludeRestricted(seeRestricted)
			.SingleOrDefaultAsync(t => t.Id == Id);

		if (topic is null)
		{
			return NotFound();
		}

		var destinationForum = await db.Forums
			.ExcludeRestricted(seeRestricted)
			.SingleOrDefaultAsync(f => f.Id == Topic.SplitToForumId);

		if (destinationForum is null)
		{
			return NotFound();
		}

		var selectedPosts = Topic.Posts
			.Where(tp => tp.Selected)
			.Select(tp => tp.Id)
			.ToList();
		var postsToSplit = topic.ForumPosts
			.Where(p => selectedPosts.Contains(p.Id))
			.ToList();

		if (!postsToSplit.Any())
		{
			var splitOnPost = topic.ForumPosts
				.SingleOrDefault(p => p.Id == Topic.PostToSplitId);

			if (splitOnPost is null)
			{
				await PopulatePosts();
				await PopulateAvailableForums();
				return Page();
			}

			postsToSplit = topic.ForumPosts
				.Where(p => p.Id == splitOnPost.Id
					|| p.CreateTimestamp > splitOnPost.CreateTimestamp)
				.ToList();
		}

		var newTopic = new ForumTopic
		{
			Type = ForumTopicType.Regular,
			Title = Topic.SplitTopicName,
			PosterId = User.GetUserId(),
			ForumId = Topic.SplitToForumId
		};

		db.ForumTopics.Add(newTopic);
		await db.SaveChangesAsync();

		foreach (var post in postsToSplit)
		{
			post.TopicId = newTopic.Id;
			post.ForumId = destinationForum.Id;
		}

		await db.SaveChangesAsync();

		forumService.ClearLatestPostCache();
		forumService.ClearTopicActivityCache();

		await publisher.SendForum(
			destinationForum.Restricted || topic.Forum!.Restricted,
			$"Topic SPLIT by {User.Name()}",
			$"[Topic]({{0}}) SPLIT by {User.Name()}",
			$@"""{newTopic.Title}"" from ""{Topic.Title}""",
			$"Forum/Topics/{newTopic.Id}");

		return RedirectToPage("Index", new { id = newTopic.Id });
	}

	private async Task<SplitTopicModel?> PopulatePosts()
	{
		bool seeRestricted = CanSeeRestricted;
		return await db.ForumTopics
			.ExcludeRestricted(seeRestricted)
			.Where(t => t.Id == Id)
			.Select(t => new SplitTopicModel
			{
				Title = t.Title,
				SplitTopicName = "(Split from " + t.Title + ")",
				SplitToForumId = t.Forum!.Id,
				ForumId = t.Forum.Id,
				ForumName = t.Forum.Name,
				Posts = t.ForumPosts
					.Select(p => new SplitTopicModel.Post
					{
						Id = p.Id,
						PostCreateTimestamp = p.CreateTimestamp,
						EnableBbCode = p.EnableBbCode,
						EnableHtml = p.EnableHtml,
						Subject = p.Subject,
						Text = p.Text,
						PosterId = p.PosterId,
						PosterName = p.Poster!.UserName,
						PosterAvatar = p.Poster.Avatar
					})
					.OrderBy(p => p.PostCreateTimestamp)
					.ToList()
			})
			.SingleOrDefaultAsync();
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
	}
}
