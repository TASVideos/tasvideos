using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Data.Entity.Forum;
using TASVideos.Models;
using TASVideos.Services;
using TASVideos.Services.ExternalMediaPublisher;

namespace TASVideos.Pages.Forum.Topics
{
	[AllowAnonymous]
	public class IndexModel : BaseForumModel
	{
		private readonly ApplicationDbContext _db;
		private readonly ExternalMediaPublisher _publisher;
		private readonly IAwardsCache _awards;

		public IndexModel(
			ApplicationDbContext db,
			ExternalMediaPublisher publisher,
			IAwardsCache awards)
			: base(db)
		{
			_db = db;
			_publisher = publisher;
			_awards = awards;
		}

		[FromRoute]
		public int Id { get; set; }

		[FromQuery]
		public TopicRequest Search { get; set; }

		public ForumTopicModel Topic { get; set; }

		public async Task<IActionResult> OnGet()
		{
			int? userId = User.Identity.IsAuthenticated
				? User.GetUserId()
				: (int?)null;

			bool seeRestricted = User.Has(PermissionTo.SeeRestrictedForums);
			Topic = await _db.ForumTopics
				.ExcludeRestricted(seeRestricted)
				.Select(t => new ForumTopicModel
				{
					Id = t.Id,
					IsWatching = userId.HasValue && t.ForumTopicWatches.Any(ft => ft.UserId == userId.Value),
					Title = t.Title,
					ForumId = t.ForumId,
					ForumName = t.Forum.Name,
					IsLocked = t.IsLocked,
					Poll = t.PollId.HasValue
						? new ForumTopicModel.PollModel { PollId = t.PollId.Value, Question = t.Poll.Question }
						: null
				})
				.SingleOrDefaultAsync(t => t.Id == Id);

			if (Topic == null)
			{
				return NotFound();
			}

			var lastPostId = (await _db.ForumPosts
				.Where(p => p.TopicId == Id)
				.ByMostRecent()
				.FirstAsync())
				.Id;

			Topic.Posts = _db.ForumPosts
				.ForTopic(Id)
				.Select(p => new ForumTopicModel.ForumPostEntry
				{
					Id = p.Id,
					TopicId = Id,
					EnableHtml = p.EnableHtml,
					EnableBbCode = p.EnableBbCode,
					PosterId = p.PosterId,
					CreateTimestamp = p.CreateTimeStamp,
					PosterName = p.Poster.UserName,
					PosterAvatar = p.Poster.Avatar,
					PosterLocation = p.Poster.From,
					PosterRoles = p.Poster.UserRoles
						.Where(ur => !ur.Role.IsDefault)
						.Select(ur => ur.Role.Name)
						.ToList(),
					PosterJoined = p.Poster.CreateTimeStamp,
					PosterPostCount = p.Poster.Posts.Count,
					Text = p.Text,
					Subject = p.Subject,
					Signature = p.Poster.Signature,
					IsLastPost = p.Id == lastPostId
				})
				.OrderBy(p => p.CreateTimestamp)
				.PageOf(_db, Search);

			foreach (var post in Topic.Posts)
			{
				post.Awards = await _awards.AwardsForUser(post.PosterId);
			}

			if (Topic.Poll != null)
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
				if (post != null)
				{
					post.Highlight = true;
				}
			}

			foreach (var post in Topic.Posts)
			{
				post.RenderedText = RenderPost(post.Text, post.EnableBbCode, post.EnableHtml);
				post.RenderedSignature = !string.IsNullOrWhiteSpace(post.Signature)
					? RenderSignature(post.Signature)
					: "";
				post.IsEditable = User.Has(PermissionTo.EditForumPosts)
					|| (userId.HasValue && post.PosterId == userId.Value && post.IsLastPost);
				post.IsDeletable = User.Has(PermissionTo.DeleteForumPosts)
					|| (userId.HasValue && post.PosterId == userId && post.IsLastPost);
			}

			if (Topic.Poll != null)
			{
				Topic.Poll.Question = RenderPost(Topic.Poll.Question, false, true); // TODO: do we have bbcode in poll questions??
			}

			if (userId.HasValue)
			{
				var watchedTopic = await _db.ForumTopicWatches
				.SingleOrDefaultAsync(w => w.UserId == userId && w.ForumTopicId == Id);

				if (watchedTopic != null && watchedTopic.IsNotified)
				{
					watchedTopic.IsNotified = false;
					await _db.SaveChangesAsync();
				}
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
					IpAddress = IpAddress.ToString()
				});
				await _db.SaveChangesAsync();
			}

			return RedirectToPage("Index", new { Id = pollOption.Poll.TopicId });
		}

		public async Task<IActionResult> OnPostLock(string topicTitle, bool locked, string returnUrl)
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
				await _db.SaveChangesAsync();
			}

			_publisher.SendForum(
				seeRestricted,
				$"Topic {topicTitle} {(locked ? "LOCKED" : "UNLOCKED")} by {User.Identity.Name}",
				"",
				$"/Forum/Topics/{Id}");

			return RedirectToLocal(returnUrl);
		}

		public async Task<IActionResult> OnGetWatch()
		{
			if (!User.Identity.IsAuthenticated)
			{
				return AccessDenied();
			}

			await WatchTopic(Id, User.GetUserId(), User.Has(PermissionTo.SeeRestrictedForums));
			return RedirectToPage("Index", new { Id });
		}

		public async Task<IActionResult> OnGetUnwatch()
		{
			if (!User.Identity.IsAuthenticated)
			{
				return AccessDenied();
			}

			await UnwatchTopic(Id, User.GetUserId(), User.Has(PermissionTo.SeeRestrictedForums));
			return RedirectToPage("Index", new { Id });
		}
	}
}
