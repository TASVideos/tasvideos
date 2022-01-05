using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TASVideos.Core.Services;
using TASVideos.Core.Services.ExternalMediaPublisher;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Data.Entity.Forum;
using TASVideos.Pages.Forum.Posts.Models;

namespace TASVideos.Pages.Forum.Posts
{
	[RequirePermission(PermissionTo.CreateForumPosts)]
	public class CreateModel : BaseForumModel
	{
		private readonly UserManager _userManager;
		private readonly ExternalMediaPublisher _publisher;
		private readonly ApplicationDbContext _db;
		private readonly ITopicWatcher _topicWatcher;

		public CreateModel(
			UserManager userManager,
			ExternalMediaPublisher publisher,
			ApplicationDbContext db,
			ITopicWatcher topicWatcher)
			: base(db, topicWatcher)
		{
			_userManager = userManager;
			_publisher = publisher;
			_topicWatcher = topicWatcher;
			_db = db;
		}

		[FromRoute]
		public int TopicId { get; set; }

		[FromQuery]
		public int? QuoteId { get; set; }

		[BindProperty]
		public ForumPostCreateModel Post { get; set; } = new ();

		[BindProperty]
		[DisplayName("Watch Topic for Replies")]
		public bool WatchTopic { get; set; }

		public async Task<IActionResult> OnGet()
		{
			var seeRestricted = User.Has(PermissionTo.SeeRestrictedForums);
			Post = await _db.ForumTopics
				.ExcludeRestricted(seeRestricted)
				.Where(t => t.Id == TopicId)
				.Select(t => new ForumPostCreateModel
				{
					TopicTitle = t.Title,
					IsLocked = t.IsLocked
				})
				.SingleOrDefaultAsync();

			if (Post == null)
			{
				return NotFound();
			}

			if (Post.IsLocked && !User.Has(PermissionTo.PostInLockedTopics))
			{
				return AccessDenied();
			}

			if (QuoteId.HasValue)
			{
				var post = await _db.ForumPosts
					.Include(p => p.Poster)
					.Where(p => p.Id == QuoteId)
					.SingleOrDefaultAsync();

				Post.Text = $"[quote=\"{post.Poster!.UserName}\"]{post.Text}[/quote]";
			}

			WatchTopic = await _topicWatcher.IsWatchingTopic(TopicId, User.GetUserId());

			return Page();
		}

		public async Task<IActionResult> OnPost()
		{
			var user = await _userManager.GetUserAsync(User);
			if (!ModelState.IsValid)
			{
				// We have to consider direct posting to this call, including "over-posting",
				// so all of this logic is necessary
				var isLocked = await _db.ForumTopics
					.AnyAsync(t => t.Id == TopicId && t.IsLocked);
				if (isLocked && !User.Has(PermissionTo.PostInLockedTopics))
				{
					return AccessDenied();
				}

				Post = new ForumPostCreateModel
				{
					TopicTitle = Post.TopicTitle,
					Subject = Post.Subject,
					Text = Post.Text,
					IsLocked = isLocked,
					UserAvatar = user.Avatar,
					UserSignature = user.Signature,
					Mood = User.Has(PermissionTo.UseMoodAvatars) ? Post.Mood : ForumPostMood.Normal
				};

				return Page();
			}

			var topic = await _db.ForumTopics
				.Include(t => t.Forum)
				.SingleOrDefaultAsync(t => t.Id == TopicId);

			if (topic == null)
			{
				return NotFound();
			}

			if (topic.Forum!.Restricted && !User.Has(PermissionTo.SeeRestrictedForums))
			{
				return NotFound();
			}

			if (topic.IsLocked && !User.Has(PermissionTo.PostInLockedTopics))
			{
				return AccessDenied();
			}

			var id = await CreatePost(TopicId, topic.ForumId, Post, user.Id, IpAddress, WatchTopic);

			var mood = Post.Mood != ForumPostMood.Normal ? $" Mood: ({Post.Mood})" : "";
			await _publisher.SendForum(
				topic.Forum.Restricted,
				$"New reply by {user.UserName}{mood}",
				$"({topic.Forum.ShortName}: {topic.Title}) ({Post.Subject})",
				$"Forum/p/{id}#{id}",
				$"{user.UserName}{mood}");

			await _topicWatcher.NotifyNewPost(new TopicNotification(
				id, topic.Id, topic.Title, user.Id));

			await _userManager.AssignAutoAssignableRolesByPost(user);

			return BaseRedirect($"/forum/p/{id}#{id}");
		}
	}
}
