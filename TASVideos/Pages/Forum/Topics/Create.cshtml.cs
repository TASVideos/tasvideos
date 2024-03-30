using System.ComponentModel;
using TASVideos.Core.Services.ExternalMediaPublisher;
using TASVideos.Data.Entity.Forum;
using TASVideos.Pages.Forum.Topics.Models;

namespace TASVideos.Pages.Forum.Topics;

[RequirePermission(PermissionTo.CreateForumTopics)]
public class CreateModel(
	UserManager userManager,
	ApplicationDbContext db,
	ExternalMediaPublisher publisher,
	IForumService forumService)
	: BaseForumModel
{
	[FromRoute]
	public int ForumId { get; set; }

	[BindProperty]
	public TopicCreateModel Topic { get; set; } = new();

	[BindProperty]
	public PollCreateModel Poll { get; set; } = new();

	[BindProperty]
	[DisplayName("Watch Topic for Replies")]
	public bool WatchTopic { get; set; } = true;

	public AvatarUrls UserAvatars { get; set; } = new(null, null);

	public string BackupSubmissionDeterminator { get; set; } = "";

	public async Task<IActionResult> OnGet()
	{
		var seeRestricted = User.Has(PermissionTo.SeeRestrictedForums);
		var topic = await db.Forums
			.ExcludeRestricted(seeRestricted)
			.Where(f => f.Id == ForumId)
			.Select(f => new TopicCreateModel
			{
				ForumName = f.Name
			})
			.SingleOrDefaultAsync();

		if (topic is null)
		{
			return NotFound();
		}

		Topic = topic;
		UserAvatars = await forumService.UserAvatars(User.GetUserId());

		var user = await userManager.GetRequiredUser(User);
		if (user.AutoWatchTopic is not null && user.AutoWatchTopic != UserPreference.Auto)
		{
			WatchTopic = user.AutoWatchTopic == UserPreference.Always;
		}

		BackupSubmissionDeterminator = (await db.ForumTopics
			.ForForum(ForumId)
			.Where(f => f.PosterId == User.GetUserId())
			.CountAsync())
			.ToString();

		return Page();
	}

	public async Task<IActionResult> OnPost()
	{
		if (Poll.HasAnyField)
		{
			if (string.IsNullOrEmpty(Poll.Question))
			{
				ModelState.AddModelError($"{nameof(Poll.Question)}", "The Question field is required.");
			}

			if (!Poll.OptionsAreValid)
			{
				ModelState.AddModelError($"{nameof(Poll.PollOptions)}", "Invalid poll options");
				return Page();
			}
		}

		if (!ModelState.IsValid)
		{
			UserAvatars = await forumService.UserAvatars(User.GetUserId());
			return Page();
		}

		var forum = await db.Forums.SingleOrDefaultAsync(f => f.Id == ForumId);
		if (forum is null)
		{
			return NotFound();
		}

		if (forum.Restricted && !User.Has(PermissionTo.SeeRestrictedForums))
		{
			return NotFound();
		}

		int userId = User.GetUserId();

		var topic = new ForumTopic
		{
			Type = Topic.Type,
			Title = Topic.Title,
			PosterId = userId,
			ForumId = ForumId
		};

		db.ForumTopics.Add(topic);

		// TODO: catch DbConcurrencyException
		await db.SaveChangesAsync();

		await forumService.CreatePost(new PostCreateDto(
			ForumId,
			topic.Id,
			null,
			Topic.Post,
			userId,
			User.Name(),
			Topic.Mood,
			IpAddress,
			WatchTopic));

		if (User.Has(PermissionTo.CreateForumPolls) && Poll.IsValid)
		{
			await forumService.CreatePoll(
				topic,
				new PollCreateDto(Poll.Question, Poll.DaysOpen, Poll.MultiSelect, Poll.PollOptions));
		}

		await publisher.SendForum(
			forum.Restricted,
			$"[New Topic]({{0}}) by {User.Name()}",
			$"{forum.ShortName}: {Topic.Title}",
			$"Forum/Topics/{topic.Id}");

		await userManager.AssignAutoAssignableRolesByPost(User.GetUserId());

		return RedirectToPage("Index", new { topic.Id });
	}

	public class TopicCreateModel
	{
		public string ForumName { get; init; } = "";

		[StringLength(100, MinimumLength = 5)]
		public string Title { get; init; } = "";
		public string Post { get; init; } = "";
		public ForumTopicType Type { get; init; } = ForumTopicType.Regular;
		public ForumPostMood Mood { get; init; } = ForumPostMood.Normal;
	}
}
