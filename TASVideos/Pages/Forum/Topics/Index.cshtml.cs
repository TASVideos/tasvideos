using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TASVideos.Core;
using TASVideos.Core.Services;
using TASVideos.Core.Services.ExternalMediaPublisher;
using TASVideos.Core.Services.Wiki;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Data.Entity.Forum;
using TASVideos.Pages.Forum.Posts.Models;
using TASVideos.Pages.Forum.Topics.Models;

namespace TASVideos.Pages.Forum.Topics;

[AllowAnonymous]
[RequireCurrentPermissions]
public class IndexModel(
	ApplicationDbContext db,
	ExternalMediaPublisher publisher,
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

	public ForumTopicModel Topic { get; set; } = new();

	public IWikiPage? WikiPage { get; set; }

	public string? EncodeEmbedLink { get; set; }
	public int? PublicationId { get; set; }

	public ForumPostEntry? HighlightedPost { get; set; }

	public bool SaveActivity { get; set; }

	public async Task<IActionResult> OnGet()
	{
		int? userId = User.IsLoggedIn()
			? User.GetUserId()
			: null;

		bool seeRestricted = User.Has(PermissionTo.SeeRestrictedForums);
		var topic = await db.ForumTopics
			.ExcludeRestricted(seeRestricted)
			.Select(t => new ForumTopicModel
			{
				Id = t.Id,
				IsWatching = userId.HasValue && t.ForumTopicWatches.Any(ft => ft.UserId == userId.Value),
				Title = t.Title,
				Restricted = t.Forum!.Restricted,
				Type = t.Type,
				ForumId = t.ForumId,
				ForumName = t.Forum!.Name,
				IsLocked = t.IsLocked,
				LastPostId = t.ForumPosts.Any() ? t.ForumPosts.Max(p => p.Id) : -1,
				SubmissionId = t.SubmissionId,
				GameId = t.GameId,
				GameName = t.Game != null ? t.Game.DisplayName : null,
				CategoryId = t.Forum!.CategoryId,
				Poll = t.PollId.HasValue
					? new ForumTopicModel.PollModel
					{
						PollId = t.PollId.Value,
						Question = t.Poll!.Question,
						CloseDate = t.Poll!.CloseDate,
						MultiSelect = t.Poll!.MultiSelect,
						ViewPollResults = ViewPollResults,
					}
					: null
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
			.Select(p => new ForumPostEntry
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
				.Select(o => new ForumTopicModel.PollModel.PollOptionModel
				{
					Text = o.Text,
					Ordinal = o.Ordinal,
					Voters = o.Votes
						.Select(v => v.UserId)
						.ToList()
				})
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
			post.IsEditable = User.Has(PermissionTo.EditForumPosts)
				|| (isOwnPost && isOpenTopic);
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
		var seeRestricted = User.Has(PermissionTo.SeeRestrictedForums);
		var topic = await db.ForumTopics
			.Include(t => t.Forum)
			.ExcludeRestricted(seeRestricted)
			.SingleOrDefaultAsync(t => t.Id == Id);
		if (topic is null)
		{
			return NotFound();
		}

		if (topic.IsLocked != locked)
		{
			topic.IsLocked = locked;

			var lockedState = locked ? "LOCKED" : "UNLOCKED";
			var result = await ConcurrentSave(db, $"Topic {topicTitle} set to locked {lockedState}", $"Unable to set {topicTitle} to status of {lockedState}");
			if (result)
			{
				await publisher.SendForum(
					topic.Forum!.Restricted,
					$"Topic {topicTitle} {lockedState} by {User.Name()}",
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

	private RedirectToPageResult RedirectToTopic()
	{
		return RedirectToPage("Index", new { Id });
	}
}
