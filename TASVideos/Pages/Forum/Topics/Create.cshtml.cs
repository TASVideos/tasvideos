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
using TASVideos.Services.ExternalMediaPublisher;

namespace TASVideos.Pages.Forum.Topics
{
	[RequirePermission(PermissionTo.CreateForumTopics)]
	public class CreateModel : BaseForumModel
	{
		private readonly ApplicationDbContext _db;
		private readonly ExternalMediaPublisher _publisher;

		public CreateModel(
			ApplicationDbContext db,
			ExternalMediaPublisher publisher)
			: base(db)
		{
			_db = db;
			_publisher = publisher;
		}

		[FromRoute]
		public int ForumId { get; set; }

		[BindProperty]
		public TopicCreateModel Topic { get; set; }

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

			if (Topic == null)
			{
				return NotFound();
			}

			return Page();
		}

		public async Task<IActionResult> OnPost()
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

			await CreatePost(topic.Id, forumPostModel, userId, IpAddress.ToString());

			//// TODO: auto-add topic permission based on post count, also ability to vote
			
			_publisher.SendForum(
				forum.Restricted,
				$"New Topic by {User.Identity.Name} ({forum.ShortName}: {Topic.Title})",
				Topic.Post.CapAndEllipse(50),
				$"{BaseUrl}/Forum/Topics/{topic.Id}");

			return RedirectToPage("Index", new { topic.Id });
		}
	}
}
