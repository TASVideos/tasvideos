using TASVideos.Core.Services.Wiki;
using TASVideos.Data.Entity.Forum;

namespace TASVideos.Pages.Forum.Topics;

[AllowAnonymous]
[RequireCurrentPermissions]
public class IndexModel(
	ApplicationDbContext db,
	IExternalMediaPublisher publisher,
	IAwards awards,
	IForumService forumService,
	IPointsService pointsService,
	ITopicWatcher topicWatcher,
	IWikiPages wikiPages)
	: BaseForumModel
{
	[FromRoute]
	public int Id { get; set; }

	[FromQuery]
	public TopicRequest Search { get; set; } = new();

	[FromQuery]
	public bool ViewPollResults { get; set; } = false;

	public TopicDisplay Topic { get; set; } = new();

	public IWikiPage? WikiPage { get; set; }

	public string? EncodeEmbedLink { get; set; }
	public int? PublicationId { get; set; }

	public PostEntry? HighlightedPost { get; set; }

	public bool SaveActivity { get; set; }

	public async Task<IActionResult> OnGet()
	{
		int? userId = User.IsLoggedIn()
			? User.GetUserId()
			: null;

		var topic = await db.ForumTopics
			.ExcludeRestricted(UserCanSeeRestricted)
			.Select(t => new TopicDisplay
			{
				Id = t.Id,
				IsWatching = userId.HasValue && t.ForumTopicWatches.Any(ft => ft.UserId == userId.Value),
				Title = t.Title,
				Restricted = t.Forum!.Restricted,
				ForumId = t.ForumId,
				ForumName = t.Forum!.Name,
				IsLocked = t.IsLocked,
				LastPostId = t.ForumPosts.Any() ? t.ForumPosts.Max(p => p.Id) : -1,
				SubmissionId = t.SubmissionId,
				GameId = t.GameId,
				GameName = t.Game != null ? t.Game.DisplayName : null,
				CategoryId = t.Forum!.CategoryId,
				Poll = t.PollId.HasValue
					? new TopicDisplay.PollModel
					{
						PollId = t.PollId.Value,
						Question = t.Poll!.Question,
						CloseDate = t.Poll!.CloseDate,
						MultiSelect = t.Poll!.MultiSelect,
						ViewPollResults = ViewPollResults
					}
					: null,
				TopicCreator = new PostEntry()
				{
					PosterName = t.Poster!.UserName,
					PosterAvatar = t.Poster!.Avatar,
					PosterMoodUrlBase = t.Poster!.MoodAvatarUrlBase,
					PosterMood = t.ForumPosts.OrderBy(p => p.CreateTimestamp).First().PosterMood,
					Text = t.ForumPosts.OrderBy(p => p.CreateTimestamp).First().Text,
					EnableBbCode = t.ForumPosts.OrderBy(p => p.CreateTimestamp).First().EnableBbCode,
					EnableHtml = t.ForumPosts.OrderBy(p => p.CreateTimestamp).First().EnableHtml,
				}
			})
			.SingleOrDefaultAsync(t => t.Id == Id);

		if (topic is null)
		{
			return NotFound();
		}

		Topic = topic;
		if (Topic.SubmissionId.HasValue)
		{
			WikiPage = await wikiPages.Page(LinkConstants.SubmissionWikiPage + Topic.SubmissionId.Value);
			var sub = await db.Submissions
				.Where(s => s.Id == Topic.SubmissionId.Value)
				.Select(s => new { s.EncodeEmbedLink, PublicationId = s.Publication != null ? s.Publication.Id : (int?)null })
				.SingleOrDefaultAsync();

			if (sub is not null)
			{
				EncodeEmbedLink = sub.EncodeEmbedLink;
				PublicationId = sub.PublicationId;
			}
		}

		Topic.Posts = await db.ForumPosts
			.ForTopic(Id)
			.Select(p => new PostEntry
			{
				Id = p.Id,
				TopicId = Id,
				Restricted = topic.Restricted,
				EnableHtml = p.EnableHtml,
				EnableBbCode = p.EnableBbCode,
				PosterId = p.PosterId,
				CreateTimestamp = p.CreateTimestamp,
				LastUpdateTimestamp = p.LastUpdateTimestamp,
				PosterName = p.Poster!.UserName,
				PosterAvatar = p.Poster.Avatar,
				PosterMoodUrlBase = p.Poster.MoodAvatarUrlBase,
				PosterPronouns = p.Poster.PreferredPronouns,
				PosterLocation = p.Poster.From,
				PosterRoles = p.Poster.UserRoles
					.Where(ur => !ur.Role!.IsDefault)

					// TODO: these violate separation of concerns, the code should not be aware of the specifics of what roles exist, as those can be any value a user chooses
					.Where(ur => ur.Role!.Name != "Published Author")
					.Where(ur => ur.Role!.Name != "Experienced Forum User")
					.Select(ur => ur.Role!.Name)
					.ToList(),
				PosterJoined = p.Poster.CreateTimestamp,
				PosterPostCount = p.Poster.Posts.Count,
				PosterMood = p.PosterMood,
				PosterIsBanned = p.Poster.BannedUntil.HasValue && p.Poster.BannedUntil > DateTime.UtcNow,
				Text = p.Text,
				PostEditedTimestamp = p.PostEditedTimestamp,
				Subject = p.Subject,
				Signature = p.Poster.Signature,
				IsLastPost = p.Id == Topic.LastPostId
			})
			.OrderBy(p => p.CreateTimestamp)
			.PageOf(Search);

		foreach (var post in Topic.Posts)
		{
			post.Awards = await awards.ForUser(post.PosterId);
			var (points, rank) = await pointsService.PlayerPoints(post.PosterId);
			post.PosterPlayerPoints = points;
			post.PosterPlayerRank = rank;
		}

		if (Topic.Poll is not null)
		{
			Topic.Poll.Options = await db.ForumPollOptions
				.ForPoll(Topic.Poll.PollId)
				.Select(o => new TopicDisplay.PollOption(
					o.Text,
					o.Ordinal,
					o.Votes
						.Select(v => v.UserId)
						.ToList()))
				.ToListAsync();
		}

		if (Search.Highlight.HasValue)
		{
			HighlightedPost = Topic.Posts.SingleOrDefault(p => p.Id == Search.Highlight);
			if (HighlightedPost is not null)
			{
				HighlightedPost.Highlight = true;
			}
		}

		foreach (var post in Topic.Posts)
		{
			var isOwnPost = post.PosterId == userId;
			var isOpenTopic = !topic.IsLocked;
			post.IsEditable = User.Has(PermissionTo.EditUsersForumPosts)
				|| (isOwnPost && User.Has(PermissionTo.EditForumPosts) && isOpenTopic);
			post.IsDeletable = User.Has(PermissionTo.DeleteForumPosts)
				|| (isOwnPost && isOpenTopic && post.IsLastPost);
		}

		if (userId.HasValue)
		{
			await topicWatcher.MarkSeen(Id, userId.Value);
		}

		SaveActivity = (await forumService.GetPostActivityOfSubforum(Topic.ForumId)).ContainsKey(Id);

		return Page();
	}

	public async Task<IActionResult> OnPostVote(int pollId, List<int> ordinal)
	{
		if (!User.Has(PermissionTo.VoteInPolls))
		{
			return AccessDenied();
		}

		var pollOptions = await db.ForumPollOptions
			.Include(o => o.Poll)
			.Include(o => o.Votes)
			.ForPoll(pollId)
			.ToListAsync();

		if (pollOptions.Count == 0 || pollOptions.First().Poll is null)
		{
			return NotFound();
		}

		var nowTimestamp = DateTime.UtcNow;

		if (pollOptions.First().Poll!.CloseDate <= nowTimestamp)
		{
			ErrorStatusMessage("Poll is already closed.");
			return RedirectToTopic();
		}

		var selectedOptions = pollOptions
			.Where(o => ordinal.Contains(o.Ordinal))
			.ToList();

		if (!selectedOptions.Any())
		{
			return RedirectToTopic();
		}

		if (!pollOptions.First().Poll!.MultiSelect && selectedOptions.Count != 1)
		{
			ErrorStatusMessage("Poll only allows 1 selection.");
			return RedirectToTopic();
		}

		var userId = User.GetUserId();
		if (pollOptions.All(o => o.Votes.All(v => v.UserId != userId)))
		{
			foreach (var selectedOption in selectedOptions)
			{
				selectedOption.Votes.Add(new ForumPollOptionVote
				{
					UserId = User.GetUserId(),
					CreateTimestamp = nowTimestamp,
					IpAddress = IpAddress
				});
				await db.SaveChangesAsync();
			}
		}

		return RedirectToTopic();
	}

	public async Task<IActionResult> OnPostLock(string topicTitle, bool locked)
	{
		var topic = await db.ForumTopics
			.Include(t => t.Forum)
			.ExcludeRestricted(UserCanSeeRestricted)
			.SingleOrDefaultAsync(t => t.Id == Id);
		if (topic is null)
		{
			return NotFound();
		}

		if (topic.IsLocked != locked)
		{
			topic.IsLocked = locked;

			var lockedState = locked ? "LOCKED" : "UNLOCKED";
			var result = await db.TrySaveChanges();
			SetMessage(result, $"Topic {topicTitle} set to locked {lockedState}", $"Unable to set {topicTitle} to status of {lockedState}");
			if (result.IsSuccess())
			{
				await publisher.SendForum(
					topic.Forum!.Restricted,
					$"[Topic]({{0}}) {lockedState} by {User.Name()}",
					$"{topic.Forum.ShortName}: {topic.Title}",
					$"Forum/Topics/{Id}");
			}
		}

		return RedirectToTopic();
	}

	public async Task<IActionResult> OnGetWatch()
	{
		if (!User.IsLoggedIn())
		{
			return AccessDenied();
		}

		await topicWatcher.WatchTopic(Id, User.GetUserId(), User.Has(PermissionTo.SeeRestrictedForums));
		return RedirectToTopic();
	}

	public async Task<IActionResult> OnGetUnwatch()
	{
		if (!User.IsLoggedIn())
		{
			return AccessDenied();
		}

		await topicWatcher.UnwatchTopic(Id, User.GetUserId());
		return RedirectToTopic();
	}

	private RedirectToPageResult RedirectToTopic() => RedirectToPage("Index", new { Id });

	[PagingDefaults(PageSize = ForumConstants.PostsPerPage, Sort = $"{nameof(PostEntry.CreateTimestamp)}")]
	public class TopicRequest : PagingModel
	{
		public int? Highlight { get; set; }
	}

	public class TopicDisplay
	{
		public int Id { get; init; }
		public int LastPostId { get; init; }
		public bool Restricted { get; init; }
		public bool IsWatching { get; init; }
		public bool IsLocked { get; init; }
		public string Title { get; init; } = "";
		public int ForumId { get; init; }
		public string ForumName { get; init; } = "";
		public int? SubmissionId { get; init; }
		public PageOf<PostEntry, TopicRequest> Posts { get; set; } = new([], new());
		public PollModel? Poll { get; init; }
		public int? GameId { get; init; }
		public string? GameName { get; init; }
		public int CategoryId { get; init; }
		public PostEntry? TopicCreator { get; init; }

		public class PollModel
		{
			public int PollId { get; init; }
			public string Question { get; init; } = "";
			public DateTime? CloseDate { get; init; }
			public bool MultiSelect { get; init; }
			public bool ViewPollResults { get; init; }
			public List<PollOption> Options { get; set; } = [];
		}

		public record PollOption(string Text, int Ordinal, List<int> Voters);
	}

	public class PostEntry
	{
		public int Id { get; init; }
		public int TopicId { get; init; }
		public bool Highlight { get; set; }
		public bool Restricted { get; init; }
		public int PosterId { get; init; }
		public string PosterName { get; set; } = "";
		public string? PosterAvatar { get; set; }
		public string? PosterLocation { get; set; }
		public int PosterPostCount { get; set; }
		public bool PosterIsBanned { get; set; }
		public double PosterPlayerPoints { get; set; }
		public DateTime PosterJoined { get; set; }
		public string? PosterMoodUrlBase { get; set; }
		public ForumPostMood PosterMood { get; init; }
		public PreferredPronounTypes PosterPronouns { get; set; }
		public IList<string> PosterRoles { get; set; } = [];
		public string? PosterPlayerRank { get; set; }
		public string Text { get; init; } = "";
		public DateTime? PostEditedTimestamp { get; init; }
		public string? Subject { get; init; }
		public string? Signature { get; set; }

		public ICollection<AwardAssignmentSummary> Awards { get; set; } = [];

		public bool EnableHtml { get; init; }
		public bool EnableBbCode { get; init; }

		[Sortable]
		public DateTime CreateTimestamp { get; init; }
		public DateTime LastUpdateTimestamp { get; init; }

		public bool IsLastPost { get; init; }
		public bool IsEditable { get; set; }
		public bool IsDeletable { get; set; }

		public string? GetCurrentAvatar()
		{
			if (PosterMood != ForumPostMood.None && !string.IsNullOrWhiteSpace(PosterMoodUrlBase))
			{
				return PosterMoodUrlBase.Replace("$", ((int)PosterMood).ToString());
			}

			return PosterAvatar;
		}

		public string CalculatedRoles
		{
			get
			{
				if (PosterIsBanned)
				{
					return "Banned User";
				}

				return string.Join(", ", PosterRoles
					.OrderBy(s => s)
					.Append(PosterPlayerRank)
					.Where(s => !string.IsNullOrEmpty(s))
					.Select(s => s!.Replace(' ', '\u00A0')));
			}
		}
	}
}
