using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Data.Entity.Forum;
using TASVideos.Models;
using TASVideos.Services;
using TASVideos.Services.ExternalMediaPublisher;
using TASVideos.Tasks;

namespace TASVideos.Pages.Forum.Posts
{
	[RequirePermission(PermissionTo.CreateForumPosts)]
	public class CreateModel : BasePageModel
	{
		private readonly UserManager _userManager;
		private readonly ForumTasks _forumTasks;
		private readonly ExternalMediaPublisher _publisher;
		private readonly ApplicationDbContext _db;
		private readonly IEmailService _emailService;

		public CreateModel(
			UserManager userManager,
			ForumTasks forumTasks,
			ExternalMediaPublisher publisher,
			ApplicationDbContext db,
			IEmailService emailService)
		{
			_userManager = userManager;
			_forumTasks = forumTasks;
			_publisher = publisher;
			_emailService = emailService;
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
					UserSignature = user.Signature
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

			if (topic.Forum.Restricted && !User.Has(PermissionTo.SeeRestrictedForums))
			{
				return NotFound();
			}

			if (topic.IsLocked && !User.Has(PermissionTo.PostInLockedTopics))
			{
				return AccessDenied();
			}

			var id = await _forumTasks.CreatePost(TopicId, Post, user.Id, IpAddress.ToString());

			_publisher.SendForum(
				topic.Forum.Restricted,
				$"New reply by {user.UserName} ({topic.Forum.ShortName}: {topic.Title}) ({Post.Subject})",
				$"{Post.TopicTitle} ({Post.Subject})",
				$"{BaseUrl}/p/{id}#{id}");

			// Notify watched topic
			var watches = await _db.ForumTopicWatches
				.Include(w => w.User)
				.Where(w => w.ForumTopicId == TopicId)
				.Where(w => w.UserId != user.Id)
				.Where(w => !w.IsNotified)
				.ToListAsync();

			if (watches.Any())
			{
				await _emailService.SendTopicNotification(watches.Select(w => w.User.Email));

				foreach (var watch in watches)
				{
					watch.IsNotified = true;
				}

				await _db.SaveChangesAsync();
			}

			return RedirectToPage("/Forum/Topic", new { Id = TopicId });
		}
	}
}
