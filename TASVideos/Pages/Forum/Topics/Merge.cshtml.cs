using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;

using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Data.Entity.Forum;
using TASVideos.Pages.Forum.Topics.Models;
using TASVideos.Services.ExternalMediaPublisher;

namespace TASVideos.Pages.Forum.Topics
{
	[RequirePermission(PermissionTo.MergeTopics)]
	public class MergeModel : BasePageModel
	{
		private readonly ApplicationDbContext _db;
		private readonly ExternalMediaPublisher _publisher;

		public MergeModel(
			ApplicationDbContext db,
			ExternalMediaPublisher publisher)
		{
			_db = db;
			_publisher = publisher;
		}

		[FromRoute]
		public int Id { get; set; }

		[BindProperty]
		public MergeTopicModel Topic { get; set; } = new();

		public IEnumerable<SelectListItem> AvailableForums { get; set; } = new List<SelectListItem>();

		public IEnumerable<SelectListItem> AvailableTopics { get; set; } = new List<SelectListItem>();

		private bool CanSeeRestricted => User.Has(PermissionTo.SeeRestrictedForums);

		public async Task<IActionResult> OnGet()
		{
			bool seeRestricted = CanSeeRestricted;
			Topic = await _db.ForumTopics
				.ExcludeRestricted(seeRestricted)
				.Where(t => t.Id == Id)
				.Select(t => new MergeTopicModel
				{
					Title = t.Title,
					ForumId = t.Forum!.Id,
					ForumName = t.Forum.Name
				})
				.SingleOrDefaultAsync();

			if (Topic == null)
			{
				return NotFound();
			}

			Topic.DestinationForumId = Topic.ForumId;
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
			var originalTopic = await _db.ForumTopics
				.ExcludeRestricted(seeRestricted)
				.SingleOrDefaultAsync(t => t.Id == Id);

			if (originalTopic == null)
			{
				return NotFound();
			}

			var destinationTopic = await _db.ForumTopics
				.Include(t => t.Forum)
				.ExcludeRestricted(seeRestricted)
				.SingleOrDefaultAsync(t => t.Id == Topic.DestinationForumId);

			if (destinationTopic == null)
			{
				return NotFound();
			}

			var oldPosts = await _db.ForumPosts
				.Where(p => p.TopicId == Id)
				.ToListAsync();

			foreach (var post in oldPosts)
			{
				post.TopicId = Topic.DestinationTopicId;
			}

			_db.ForumTopics.Remove(originalTopic);

			try
			{
				await _db.SaveChangesAsync();
			}
			catch (DbUpdateConcurrencyException)
			{
				ModelState.AddModelError("", "An error occurred. The topic may have changed since loading this page. Go back and try again.");
				return Page();
			}

			_publisher.SendForum(
				destinationTopic.Forum!.Restricted,
				$"Topic {originalTopic.Title} merged into {destinationTopic.Title} by {User.Name()}",
				"",
				$"Forum/Topics/{destinationTopic.Id}",
				User.Name());

			return RedirectToPage("Index", new { id = Topic.DestinationTopicId });
		}

		public async Task<IActionResult> OnGetTopicsForForum(int forumId)
		{
			var items = UiDefaults.DefaultEntry.Concat(await GetTopicsForForum(forumId));
			return new PartialViewResult
			{
				ViewName = "_DropdownItems",
				ViewData = new ViewDataDictionary<IEnumerable<SelectListItem>>(ViewData, items)
			};
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

			AvailableTopics = UiDefaults.DefaultEntry.Concat(await GetTopicsForForum(Topic.ForumId));
		}

		private async Task<IEnumerable<SelectListItem>> GetTopicsForForum(int forumId)
		{
			var seeRestricted = CanSeeRestricted;
			return await _db.ForumTopics
				.ExcludeRestricted(seeRestricted)
				.Where(t => t.ForumId == forumId)
				.Select(t => new SelectListItem
				{
					Text = t.Title,
					Value = t.Id.ToString()
				})
				.ToListAsync();
		}
	}
}
