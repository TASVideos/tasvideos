using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Data.Entity.Forum;
using TASVideos.Models;
using TASVideos.Services.ExternalMediaPublisher;
using TASVideos.Tasks;

namespace TASVideos.Pages.Forum.Topics
{
	[AllowAnonymous]
	public class IndexModel : BasePageModel
	{
		private readonly ApplicationDbContext _db;
		private readonly UserManager<User> _userManager;
		private readonly ExternalMediaPublisher _publisher;
		private readonly ForumTasks _forumTasks;

		public IndexModel(
			ApplicationDbContext db,
			UserManager<User> userManager,
			ExternalMediaPublisher publisher,
			ForumTasks forumTasks,
			UserTasks userTasks)
			: base(userTasks)
		{
			_db = db;
			_userManager = userManager;
			_publisher = publisher;
			_forumTasks = forumTasks;
		}

		[FromRoute]
		public int Id { get; set; }

		[FromQuery]
		public TopicRequest Search { get; set; }

		public ForumTopicModel Topic { get; set; }

		public async Task<IActionResult> OnGet()
		{
			int? userId = User.Identity.IsAuthenticated
				? int.Parse(_userManager.GetUserId(User))
				: (int?)null;

			Topic = await _forumTasks
				.GetTopicForDisplay(Id, Search, UserHas(PermissionTo.SeeRestrictedForums), userId);

			if (Topic == null)
			{
				return NotFound();
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
				post.IsEditable = UserHas(PermissionTo.EditForumPosts)
					|| (userId.HasValue && post.PosterId == userId.Value && post.IsLastPost);
				post.IsDeletable = UserHas(PermissionTo.DeleteForumPosts)
					|| (userId.HasValue && post.PosterId == userId && post.IsLastPost);
			}

			if (Topic.Poll != null)
			{
				Topic.Poll.Question = RenderPost(Topic.Poll.Question, false, true); // TODO: do we have bbcode in poll questions??
			}

			if (userId.HasValue)
			{
				await _forumTasks.MarkTopicAsUnNotifiedForUser(userId.Value,  Id);
			}

			return Page();
		}

		public async Task<IActionResult> OnPostVote(int pollId, int ordinal)
		{
			if (!UserHas(PermissionTo.VoteInPolls))
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

			var user = await _userManager.GetUserAsync(User);
			if (pollOption.Votes.All(v => v.UserId != user.Id))
			{
				pollOption.Votes.Add(new ForumPollOptionVote
				{
					User = user,
					IpAddress = IpAddress.ToString()
				});
				await _db.SaveChangesAsync();
			}

			return RedirectToPage("Index", new { Id = pollOption.Poll.TopicId });
		}

		public async Task<IActionResult> OnPostLock(string topicTitle, bool locked, string returnUrl)
		{
			var seeRestricted = UserHas(PermissionTo.SeeRestrictedForums);
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

			var user = await _userManager.GetUserAsync(User);
			await _forumTasks.WatchTopic(Id, user.Id, UserHas(PermissionTo.SeeRestrictedForums));
			return RedirectToPage("Index", new { Id });
		}

		public async Task<IActionResult> OnGetUnwatch()
		{
			if (!User.Identity.IsAuthenticated)
			{
				return AccessDenied();
			}

			var user = await _userManager.GetUserAsync(User);
			await _forumTasks.UnwatchTopic(Id, user.Id, UserHas(PermissionTo.SeeRestrictedForums));
			return RedirectToPage("Index", new { Id });
		}
	}
}
