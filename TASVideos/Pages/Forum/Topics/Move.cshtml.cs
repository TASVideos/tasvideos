using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TASVideos.Core.Services.ExternalMediaPublisher;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Data.Entity.Forum;
using TASVideos.Pages.Forum.Topics.Models;

namespace TASVideos.Pages.Forum.Topics
{
	[RequirePermission(PermissionTo.MoveTopics)]
	public class MoveModel : BasePageModel
	{
		private readonly ApplicationDbContext _db;
		private readonly ExternalMediaPublisher _publisher;

		public MoveModel(
			ApplicationDbContext db,
			ExternalMediaPublisher publisher)
		{
			_db = db;
			_publisher = publisher;
		}

		[FromRoute]
		public int Id { get; set; }

		[BindProperty]
		public MoveTopicModel Topic { get; set; } = new ();

		public IEnumerable<SelectListItem> AvailableForums { get; set; } = new List<SelectListItem>();

		public bool CanSeeRestricted => User.Has(PermissionTo.SeeRestrictedForums);

		public async Task<IActionResult> OnGet()
		{
			var seeRestricted = CanSeeRestricted;

			Topic = await _db.ForumTopics
				.Where(t => t.Id == Id)
				.Include(t => t.Forum)
				.ExcludeRestricted(seeRestricted)
				.Select(t => new MoveTopicModel
				{
					TopicTitle = t.Title,
					ForumId = t.Forum!.Id,
					ForumName = t.Forum.Name
				})
				.SingleOrDefaultAsync();

			if (Topic == null)
			{
				return NotFound();
			}

			await PopulateAvailableForums();

			return Page();
		}

		public async Task<IActionResult> OnPost()
		{
			if (!ModelState.IsValid)
			{
				await PopulateAvailableForums();
				return Page();
			}

			var seeRestricted = CanSeeRestricted;
			var topic = await _db.ForumTopics
				.Include(t => t.Forum)
				.ExcludeRestricted(seeRestricted)
				.SingleOrDefaultAsync(t => t.Id == Id);

			if (topic == null)
			{
				return NotFound();
			}

			var forum = await _db.Forums.SingleOrDefaultAsync(f => f.Id == Topic.ForumId);

			if (forum == null)
			{
				return NotFound();
			}

			var topicWasRestricted = topic.Forum?.Restricted ?? false;
			topic.ForumId = Topic.ForumId;

			var postsToMove = await _db.ForumPosts
				.ForTopic(topic.Id)
				.ToListAsync();

			foreach (var post in postsToMove)
			{
				post.ForumId = forum.Id;
			}

			await _db.SaveChangesAsync();

			await _publisher.SendForum(
				topicWasRestricted || forum.Restricted,
				$"Topic MOVED by {User.Name()}",
				$@"""{Topic.TopicTitle}"" from {Topic.ForumName} to {forum.Name}",
				$"Forum/Topics/{Id}");

			return RedirectToPage("Index", new { Id });
		}

		private async Task PopulateAvailableForums()
		{
			AvailableForums = await _db.Forums
				.ExcludeRestricted(CanSeeRestricted)
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
