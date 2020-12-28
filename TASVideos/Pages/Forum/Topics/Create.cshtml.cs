using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Data.Entity.Forum;
using TASVideos.Extensions;
using TASVideos.Pages.Forum.Posts.Models;
using TASVideos.Pages.Forum.Topics.Models;
using TASVideos.Services;
using TASVideos.Services.ExternalMediaPublisher;

namespace TASVideos.Pages.Forum.Topics
{
	[RequirePermission(PermissionTo.CreateForumTopics)]
	public class CreateModel : BaseForumModel
	{
		private readonly UserManager _userManager;
		private readonly ApplicationDbContext _db;
		private readonly ExternalMediaPublisher _publisher;

		public CreateModel(
			UserManager userManager,
			ApplicationDbContext db,
			ExternalMediaPublisher publisher,
			ITopicWatcher watcher)
			: base(db, watcher)
		{
			_userManager = userManager;
			_db = db;
			_publisher = publisher;
		}

		[FromRoute]
		public int ForumId { get; set; }

		[BindProperty]
		public TopicCreateModel Topic { get; set; } = new ();

		public async Task<IActionResult> OnGet()
		{
			var seeRestricted = User.Has(PermissionTo.SeeRestrictedForums);
			Topic = await _db.Forums
				.ExcludeRestricted(seeRestricted)
				.Where(f => f.Id == ForumId)
				.Select(f => new TopicCreateModel
				{
					ForumName = f.Name
				})
				.SingleOrDefaultAsync();

			return Topic == null
				? NotFound()
				: Page();
		}

		public async Task<IActionResult> OnPost(PollCreateModel poll)
		{
			if (!ModelState.IsValid)
			{
				return Page();
			}

			var forum = await _db.Forums.SingleOrDefaultAsync(f => f.Id == ForumId);
			if (forum == null)
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

			_db.ForumTopics.Add(topic);

			// TODO: catch DbConcurrencyException
			await _db.SaveChangesAsync();

			var forumPostModel = new ForumPostModel
			{
				Subject = null,
				Text = Topic.Post
			};

			await CreatePost(topic.Id, forumPostModel, userId, IpAddress);

			if (User.Has(PermissionTo.CreateForumPolls) && poll.IsValid)
			{
				await CreatePoll(topic, poll);
			}

			_publisher.SendForum(
				forum.Restricted,
				$"New Topic by {User.Name()} ({forum.ShortName}: {Topic.Title})",
				Topic.Post.CapAndEllipse(50),
				$"Forum/Topics/{topic.Id}",
				User.Name());

			var user = await _userManager.GetUserAsync(User);
			await _userManager.AssignAutoAssignableRolesByPost(user);

			return RedirectToPage("Index", new { topic.Id });
		}
	}
}
