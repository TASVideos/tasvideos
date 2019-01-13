using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Data.Entity.Forum;
using TASVideos.Models;
using TASVideos.Services.ExternalMediaPublisher;
using TASVideos.Tasks;

namespace TASVideos.Pages.Forum.Posts
{
	[RequirePermission(PermissionTo.CreateForumPosts)]
	public class CreateModel : BasePageModel
	{
		private readonly UserManager<User> _userManager;
		private readonly ForumTasks _forumTasks;
		private readonly ExternalMediaPublisher _publisher;
		private readonly ApplicationDbContext _db;

		public CreateModel(
			UserManager<User> userManager,
			ForumTasks forumTasks,
			ExternalMediaPublisher publisher,
			ApplicationDbContext db,
			UserTasks userTasks) 
			: base(userTasks)
		{
			_userManager = userManager;
			_forumTasks = forumTasks;
			_publisher = publisher;
			_db = db;
		}

		[FromRoute]
		public int TopicId { get; set; }

		[FromQuery]
		public int? QuoteId { get; set; }

		[BindProperty]
		public ForumPostCreateModel Post { get; set; }

		public async Task<IActionResult> OnGet()
		{
			var seeRestricted = UserHas(PermissionTo.SeeRestrictedForums);
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

			if (Post.IsLocked && !UserHas(PermissionTo.PostInLockedTopics))
			{
				return AccessDenied();
			}

			if (QuoteId.HasValue)
			{
				var post = await _db.ForumPosts
					.Include(p => p.Poster)
					.Where(p => p.Id == QuoteId)
					.SingleOrDefaultAsync();

				Post.Text = $"[quote=\"{post.Poster.UserName}\"]{post.Text}[/quote]";
			}

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
				if (isLocked && !UserHas(PermissionTo.PostInLockedTopics))
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
					UserSignature = user.Signature
				};

				return Page();
			}

			var topic = await _forumTasks.GetTopic(TopicId);
			if (topic == null)
			{
				return NotFound();
			}

			if (topic.Forum.Restricted && !UserHas(PermissionTo.SeeRestrictedForums))
			{
				return NotFound();
			}

			if (topic.IsLocked && !UserHas(PermissionTo.PostInLockedTopics))
			{
				return AccessDenied();
			}

			var id = await _forumTasks.CreatePost(TopicId, Post, user, IpAddress.ToString());

			_publisher.SendForum(
				topic.Forum.Restricted,
				$"New reply by {user.UserName} ({topic.Forum.ShortName}: {topic.Title}) ({Post.Subject})",
				$"{Post.TopicTitle} ({Post.Subject})",
				$"{BaseUrl}/p/{id}#{id}");

			await _forumTasks.NotifyWatchedTopics(TopicId, user.Id);

			return RedirectToPage("/Forum/Topic", new { Id = TopicId });
		}
	}
}
