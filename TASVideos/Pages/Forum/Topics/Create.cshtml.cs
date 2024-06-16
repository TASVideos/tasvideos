using TASVideos.Data.Entity.Forum;

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
	public string ForumName { get; set; } = "";

	[BindProperty]
	[StringLength(100, MinimumLength = 5)]
	public string Title { get; init; } = "";

	[BindProperty]
	public string Post { get; init; } = "";

	[BindProperty]
	public ForumTopicType Type { get; init; } = ForumTopicType.Regular;

	[BindProperty]
	public ForumPostMood Mood { get; init; } = ForumPostMood.Normal;

	[BindProperty]
	public AddEditPollModel.PollCreate Poll { get; set; } = new();

	[BindProperty]
	public bool WatchTopic { get; set; } = true;

	public AvatarUrls UserAvatars { get; set; } = new(null, null);

	public string BackupSubmissionDeterminator { get; set; } = "";

	public async Task<IActionResult> OnGet()
	{
		var seeRestricted = User.Has(PermissionTo.SeeRestrictedForums);
		var forum = await db.Forums
			.ExcludeRestricted(seeRestricted)
			.Where(f => f.Id == ForumId)
			.Select(f => new { f.Name, f.CanCreateTopics })
			.SingleOrDefaultAsync();

		if (forum is null)
		{
			return NotFound();
		}

		if (!forum.CanCreateTopics)
		{
			return AccessDenied();
		}

		ForumName = forum.Name;
		UserAvatars = await forumService.UserAvatars(User.GetUserId());

		var user = await userManager.GetRequiredUser(User);
		if (user.AutoWatchTopic == UserPreference.Always)
		{
			WatchTopic = true;
		}

		BackupSubmissionDeterminator = (await db.ForumTopics
			.ForForum(ForumId)
			.CountAsync(t => t.PosterId == User.GetUserId()))
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
			Type = Type,
			Title = Title,
			PosterId = userId,
			ForumId = ForumId
		};

		await using var dbTransaction = await db.Database.BeginTransactionAsync();
		db.ForumTopics.Add(topic);
		await db.SaveChangesAsync();

		await forumService.CreatePost(new PostCreateDto(
			ForumId, topic.Id, null, Post, userId, User.Name(), Mood, IpAddress, WatchTopic));

		if (User.Has(PermissionTo.CreateForumPolls) && Poll.IsValid)
		{
			await forumService.CreatePoll(
				topic,
				new PollCreateDto(Poll.Question, Poll.DaysOpen, Poll.MultiSelect, Poll.PollOptions));
		}

		await userManager.AssignAutoAssignableRolesByPost(User.GetUserId());
		await dbTransaction.CommitAsync();

		await publisher.SendForum(
			forum.Restricted,
			$"[New Topic]({{0}}) by {User.Name()}",
			$"{forum.ShortName}: {Title}",
			$"Forum/Topics/{topic.Id}");

		return RedirectToPage("Index", new { topic.Id });
	}
}
