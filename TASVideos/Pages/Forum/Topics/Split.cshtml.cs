using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Data.Entity.Forum;
using TASVideos.Pages.Forum.Topics.Models;
using TASVideos.Services.ExternalMediaPublisher;

namespace TASVideos.Pages.Forum.Topics
{
	[RequirePermission(PermissionTo.SplitTopics)]
	public class SplitModel : BasePageModel
	{
		private readonly ApplicationDbContext _db;
		private readonly ExternalMediaPublisher _publisher;

		public SplitModel(
			ApplicationDbContext db,
			ExternalMediaPublisher publisher)
		{
			_db = db;
			_publisher = publisher;
		}

		[FromRoute]
		public int Id { get; set; }

		[BindProperty]
		public SplitTopicModel Topic { get; set; }

		public IEnumerable<SelectListItem> AvailableForums { get; set; } = new List<SelectListItem>();

		private bool CanSeeRestricted => User.Has(PermissionTo.SeeRestrictedForums);

		public async Task<IActionResult> OnGet()
		{
			bool seeRestricted = CanSeeRestricted;
			Topic = await _db.ForumTopics
				.ExcludeRestricted(seeRestricted)
				.Where(t => t.Id == Id)
				.Select(t => new SplitTopicModel
				{
					Title = t.Title,
					SplitTopicName = "(Split from " + t.Title + ")",
					SplitToForumId = t.Forum.Id,
					ForumId = t.Forum.Id,
					ForumName = t.Forum.Name,
					Posts = t.ForumPosts
						.Select(p => new SplitTopicModel.Post
						{
							Id = p.Id,
							PostCreateTimeStamp = p.CreateTimeStamp,
							EnableBbCode = p.EnableBbCode,
							EnableHtml = p.EnableHtml,
							Subject = p.Subject,
							Text = p.Text,
							PosterId = p.PosterId,
							PosterName = p.Poster.UserName,
							PosterAvatar = p.Poster.Avatar
						})
						.ToList()
				})
				.SingleOrDefaultAsync();

			if (Topic == null)
			{
				return NotFound();
			}

			await PopulateAvailableForums();

			foreach (var post in Topic.Posts)
			{
				post.Text = RenderPost(post.Text, post.EnableBbCode, post.EnableHtml);
			}

			return Page();
		}

		public async Task<IActionResult> OnPost()
		{
			if (!ModelState.IsValid)
			{
				await PopulateAvailableForums();
				return Page();
			}

			bool seeRestricted = CanSeeRestricted;
			var topic = await _db.ForumTopics
				.Include(t => t.Forum)
				.Include(t => t.ForumPosts)
				.ExcludeRestricted(seeRestricted)
				.SingleOrDefaultAsync(t => t.Id == Id);

			if (topic == null)
			{
				return NotFound();
			}

			var destinationForum = _db.Forums
				.ExcludeRestricted(seeRestricted)
				.SingleOrDefaultAsync(f => f.Id == Topic.SplitToForumId);

			if (destinationForum == null)
			{
				return NotFound();
			}

			var splitOnPost = topic.ForumPosts
				.SingleOrDefault(p => p.Id == Topic.PostToSplitId);

			if (splitOnPost == null)
			{
				return NotFound();
			}

			var newTopic = new ForumTopic
			{
				Type = ForumTopicType.Regular,
				Title = Topic.SplitTopicName,
				PosterId = User.GetUserId(),
				ForumId = Topic.SplitToForumId
			};

			_db.ForumTopics.Add(newTopic);
			await _db.SaveChangesAsync();

			var splitPosts = topic.ForumPosts
				.Where(p => p.Id == splitOnPost.Id
					|| p.CreateTimeStamp > splitOnPost.CreateTimeStamp);

			foreach (var post in splitPosts)
			{
				post.TopicId = newTopic.Id;
			}

			await _db.SaveChangesAsync();

			var newForum = await _db.Forums.SingleOrDefaultAsync(f => f.Id == topic.ForumId);

			_publisher.SendForum(
				topic.Forum.Restricted,
				$"Topic {newForum.Name}: {newTopic.Title} SPLIT by {User.Identity.Name} from {Topic.ForumName}: {Topic.Title}",
				"",
				$"{BaseUrl}/Forum/Topics/{newTopic.Id}");

			return RedirectToPage("Index", new { id = newTopic.Id });
		}

		private async Task PopulateAvailableForums()
		{
			var seeRestricted = CanSeeRestricted;
			AvailableForums = await _db.Forums
					.ExcludeRestricted(seeRestricted)
					.Select(f => new SelectListItem
					{
						Text = f.Name,
						Value = f.Id.ToString(),
						Selected = f.Id == Topic.ForumId
					})
					.ToListAsync();
		}
	}
}
