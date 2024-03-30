using Microsoft.AspNetCore.Mvc.Rendering;
using TASVideos.Core.Services.ExternalMediaPublisher;
using TASVideos.Data.Entity.Forum;

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

	public List<SelectListItem> AvailableForums { get; set; } = [];

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
			$"[Topic]({{0}}) SPLIT by {User.Name()}",
			$"\"{newTopic.Title}\" from \"{Topic.Title}\"",
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
			.ToDropdown(Topic.ForumId)
			.ToListAsync();
	}

	public class SplitTopicModel
	{
		[Display(Name = "Split Posts Starting At")]
		public int? PostToSplitId { get; init; }

		[Display(Name = "Create New Topic In")]
		public int SplitToForumId { get; init; }

		[Display(Name = "New Topic Name")]
		public string SplitTopicName { get; init; } = "";

		public string Title { get; init; } = "";

		public int ForumId { get; init; }
		public string ForumName { get; init; } = "";

		public List<Post> Posts { get; init; } = [];

		public class Post
		{
			public int Id { get; init; }
			public DateTime PostCreateTimestamp { get; init; }
			public bool EnableHtml { get; init; }
			public bool EnableBbCode { get; init; }
			public string? Subject { get; init; }
			public string Text { get; init; } = "";
			public int PosterId { get; init; }
			public string PosterName { get; init; } = "";
			public string? PosterAvatar { get; init; }
			public bool Selected { get; init; }
		}
	}
}
