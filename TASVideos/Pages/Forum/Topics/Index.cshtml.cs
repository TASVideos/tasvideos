﻿using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TASVideos.Core;
using TASVideos.Core.Services;
using TASVideos.Core.Services.ExternalMediaPublisher;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Data.Entity.Forum;
using TASVideos.Pages.Forum.Posts.Models;
using TASVideos.Pages.Forum.Topics.Models;

namespace TASVideos.Pages.Forum.Topics
{
	[AllowAnonymous]
	[RequireCurrentPermissions]
	public class IndexModel : BaseForumModel
	{
		private readonly ApplicationDbContext _db;
		private readonly ExternalMediaPublisher _publisher;
		private readonly IAwards _awards;
		private readonly IPointsService _pointsService;
		private readonly ITopicWatcher _topicWatcher;
		private readonly IWikiPages _wikiPages;

		public IndexModel(
			ApplicationDbContext db,
			ExternalMediaPublisher publisher,
			IAwards awards,
			IPointsService pointsService,
			ITopicWatcher topicWatcher,
			IWikiPages wikiPages)
			: base(db, topicWatcher)
		{
			_db = db;
			_publisher = publisher;
			_awards = awards;
			_pointsService = pointsService;
			_topicWatcher = topicWatcher;
			_wikiPages = wikiPages;
		}

		[FromRoute]
		public int Id { get; set; }

		[FromQuery]
		public TopicRequest Search { get; set; } = new ();

		public ForumTopicModel Topic { get; set; } = new ();

		public WikiPage? WikiPage { get; set; }

		public string? EncodeEmbedLink { get; set; }

		public async Task<IActionResult> OnGet()
		{
			int? userId = User.IsLoggedIn()
				? User.GetUserId()
				: null;

			bool seeRestricted = User.Has(PermissionTo.SeeRestrictedForums);
			Topic = await _db.ForumTopics
				.ExcludeRestricted(seeRestricted)
				.Select(t => new ForumTopicModel
				{
					Id = t.Id,
					IsWatching = userId.HasValue && t.ForumTopicWatches.Any(ft => ft.UserId == userId.Value),
					Title = t.Title,
					Type = t.Type,
					ForumId = t.ForumId,
					ForumName = t.Forum!.Name,
					IsLocked = t.IsLocked,
					LastPostId = t.ForumPosts.Any() ? t.ForumPosts.OrderByDescending(p => p.CreateTimestamp).First().Id : -1,
					SubmissionId = t.SubmissionId,
					Poll = t.PollId.HasValue
						? new ForumTopicModel.PollModel { PollId = t.PollId.Value, Question = t.Poll!.Question }
						: null
				})
				.SingleOrDefaultAsync(t => t.Id == Id);

			if (Topic == null)
			{
				return NotFound();
			}

			if (Topic.SubmissionId.HasValue)
			{
				WikiPage = await _wikiPages.Page(LinkConstants.SubmissionWikiPage + Topic.SubmissionId.Value);
				EncodeEmbedLink = await _db.Submissions
					.Where(s => s.Id == Topic.SubmissionId.Value)
					.Select(s => s.EncodeEmbedLink)
					.SingleOrDefaultAsync();
			}

			Topic.Posts = await _db.ForumPosts
				.ForTopic(Id)
				.Select(p => new ForumPostEntry
				{
					Id = p.Id,
					TopicId = Id,
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
						.Select(ur => ur.Role!.Name)
						.ToList(),
					PosterJoined = p.Poster.CreateTimestamp,
					PosterPostCount = p.Poster.Posts.Count,
					PosterMood = p.PosterMood,
					Text = p.Text,
					Subject = p.Subject,
					Signature = p.Poster.Signature,
					IsLastPost = p.Id == Topic.LastPostId
				})
				.OrderBy(p => p.CreateTimestamp)
				.PageOf(Search);

			foreach (var post in Topic.Posts)
			{
				post.Awards = await _awards.ForUser(post.PosterId);
				post.PosterPlayerPoints = await _pointsService.PlayerPoints(post.PosterId);
			}

			if (Topic.Poll is not null)
			{
				Topic.Poll.Options = await _db.ForumPollOptions
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
				var post = Topic.Posts.SingleOrDefault(p => p.Id == Search.Highlight);
				if (post is not null)
				{
					post.Highlight = true;
				}
			}

			foreach (var post in Topic.Posts)
			{
				post.IsEditable = User.Has(PermissionTo.EditForumPosts)
					|| (userId.HasValue && post.PosterId == userId.Value);
				post.IsDeletable = User.Has(PermissionTo.DeleteForumPosts)
					|| (userId.HasValue && post.PosterId == userId && post.IsLastPost);
			}

			if (userId.HasValue)
			{
				await _topicWatcher.MarkSeen(Id, userId.Value);
			}

			return Page();
		}

		public async Task<IActionResult> OnPostVote(int pollId, int ordinal)
		{
			if (!User.Has(PermissionTo.VoteInPolls))
			{
				return AccessDenied();
			}

			var pollOption = await _db.ForumPollOptions
				.Include(o => o.Poll)
				.Include(o => o.Votes)
				.SingleOrDefaultAsync(o => o.PollId == pollId && o.Ordinal == ordinal);

			if (pollOption == null)
			{
				return NotFound();
			}

			var userId = User.GetUserId();
			if (pollOption.Votes.All(v => v.UserId != userId))
			{
				pollOption.Votes.Add(new ForumPollOptionVote
				{
					UserId = User.GetUserId(),
					IpAddress = IpAddress
				});
				await _db.SaveChangesAsync();
			}

			return RedirectToTopic();
		}

		public async Task<IActionResult> OnPostLock(string topicTitle, bool locked)
		{
			var seeRestricted = User.Has(PermissionTo.SeeRestrictedForums);
			var topic = await _db.ForumTopics
				.Include(t => t.Forum)
				.ExcludeRestricted(seeRestricted)
				.SingleOrDefaultAsync(t => t.Id == Id);
			if (topic == null)
			{
				return NotFound();
			}

			if (topic.IsLocked != locked)
			{
				topic.IsLocked = locked;

				var lockedState = locked ? "LOCKED" : "UNLOCKED";
				var result = await ConcurrentSave(_db, $"Topic set to locked {lockedState}", $"Unable to set status of {lockedState}");
				if (result)
				{
					await _publisher.SendForum(
						topic.Forum!.Restricted,
						$"Topic {topicTitle} {lockedState} by {User.Name()}",
						"",
						$"Forum/Topics/{Id}",
						User.Name());
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

			await _topicWatcher.WatchTopic(Id, User.GetUserId(), User.Has(PermissionTo.SeeRestrictedForums));
			return RedirectToTopic();
		}

		public async Task<IActionResult> OnGetUnwatch()
		{
			if (!User.IsLoggedIn())
			{
				return AccessDenied();
			}

			await _topicWatcher.UnwatchTopic(Id, User.GetUserId());
			return RedirectToTopic();
		}

		public async Task<IActionResult> OnPostReset()
		{
			var topic = await _db.ForumTopics
				.Include(t => t.Poll)
				.ThenInclude(p => p!.PollOptions)
				.ThenInclude(o => o.Votes)
				.Where(t => t.Id == Id)
				.SingleOrDefaultAsync();

			if (topic?.Poll == null)
			{
				return NotFound();
			}

			foreach (var option in topic.Poll.PollOptions)
			{
				option.Votes.Clear();
			}

			await ConcurrentSave(_db, "Poll reset", "Unable to reset poll results");
			return RedirectToTopic();
		}

		private IActionResult RedirectToTopic()
		{
			return RedirectToPage("Index", new { Id });
		}
	}
}
