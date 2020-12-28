using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Extensions;
using TASVideos.Pages.Publications.Models;
using TASVideos.Services.ExternalMediaPublisher;

namespace TASVideos.Pages.Publications
{
	[RequirePermission(PermissionTo.SetTier)]
	public class EditTierModel : BasePageModel
	{
		private readonly ApplicationDbContext _db;
		private readonly ExternalMediaPublisher _publisher;

		public EditTierModel(ApplicationDbContext db, ExternalMediaPublisher publisher)
		{
			_db = db;
			_publisher = publisher;
		}

		[FromRoute]
		public int Id { get; set; }

		[BindProperty]
		public PublicationTierEditModel Publication { get; set; } = new ();

		[BindProperty]
		public string Title { get; set; } = "";

		public IEnumerable<SelectListItem> AvailableTiers { get; set; } = new List<SelectListItem>();

		public async Task<IActionResult> OnGet()
		{
			Publication = await _db.Publications
				.Where(p => p.Id == Id)
				.Select(p => new PublicationTierEditModel
				{
					Title = p.Title,
					TierId = p.TierId
				})
				.SingleOrDefaultAsync();

			if (Publication == null)
			{
				return NotFound();
			}

			Title = Publication.Title;
			await PopulateAvailableTiers();
			return Page();
		}

		public async Task<IActionResult> OnPost()
		{
			if (!ModelState.IsValid)
			{
				await PopulateAvailableTiers();
				return Page();
			}

			var publication = await _db.Publications
				.SingleOrDefaultAsync(p => p.Id == Id);

			if (publication == null)
			{
				return NotFound();
			}

			var tier = await _db.Tiers
				.SingleOrDefaultAsync(t => t.Id == Publication.TierId);

			if (tier == null)
			{
				return NotFound();
			}

			if (publication.TierId != Publication.TierId)
			{
				var originalTier = publication.TierId;
				publication.TierId = Publication.TierId;

				_publisher.SendPublicationEdit(
					$"Publication {Id} {Title} Tier changed to {tier.Name}",
					$"{Id}M",
					User.Name());

				// TODO: catch DbConcurrencyException
				await _db.SaveChangesAsync();
			}

			return RedirectToPage("Edit", new { Id });
		}

		private async Task PopulateAvailableTiers()
		{
			AvailableTiers = await _db.Tiers
				.ToDropdown()
				.ToListAsync();
		}
	}
}
