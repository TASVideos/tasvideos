using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Data.Entity.Forum;
using TASVideos.Models;
using TASVideos.Services.ExternalMediaPublisher;

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
		public MoveTopicModel Topic { get; set; }

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
					ForumId = t.Forum.Id,
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
				.ExcludeRestricted(seeRestricted)
				.SingleOrDefaultAsync(t => t.Id == Id);

			if (topic == null)
			{
				return NotFound();
			}

			topic.ForumId = Topic.ForumId;
			await _db.SaveChangesAsync();

			var forum = await _db.Forums.SingleOrDefaultAsync(f => f.Id == Topic.ForumId);
			_publisher.SendForum(
				forum.Restricted,
				$"Topic {Topic.TopicTitle} moved from {Topic.ForumName} to {forum.Name}",
				"",
				$"{BaseUrl}/Forum/Topics/{Id}");

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
