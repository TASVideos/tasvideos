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
		public MergeTopicModel Topic { get; set; }

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
					ForumId = t.Forum.Id,
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

			return RedirectToPage("Index", new { id = /*newTopic.*/Id });
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

			AvailableTopics = UiDefaults.DefaultEntry.Concat(await _db.ForumTopics
				.ExcludeRestricted(seeRestricted)
				.Where(t => t.ForumId == Topic.ForumId)
				.Select(t => new SelectListItem
				{
					Text = t.Title,
					Value = t.Id.ToString()
				})
				.ToListAsync());
		}
	}
}
