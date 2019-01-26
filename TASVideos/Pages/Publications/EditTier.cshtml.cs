using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Models;
using TASVideos.Tasks;

namespace TASVideos.Pages.Publications
{
	[RequirePermission(PermissionTo.SetTier)]
	public class EditTierModel : BasePageModel
	{
		private readonly ApplicationDbContext _db;

		public EditTierModel(
			ApplicationDbContext db,
			UserTasks userTasks)
			: base(userTasks)
		{
			_db = db;
		}

		[FromRoute]
		public int Id { get; set; }

		[BindProperty]
		public PublicationTierEditModel Publication { get; set; }

		public IEnumerable<SelectListItem> AvailableTiers { get; set; } = new List<SelectListItem>();

		public async Task<IActionResult> OnGet()
		{
			Publication = await _db.Publications
				.Where(p => p.Id == Id)
				.Select(p => new PublicationTierEditModel
				{
					Id = p.Id,
					Title = p.Title,
					TierId = p.TierId 
				})
				.SingleOrDefaultAsync();

			if (Publication == null)
			{
				return NotFound();
			}

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

			publication.TierId = Publication.TierId;

			// TODO: catch DbConcurrencyException
			await _db.SaveChangesAsync();

			return RedirectToPage("View", new { Id });
		}

		private async Task PopulateAvailableTiers()
		{
			AvailableTiers = await _db.Tiers
			.Select(t => new SelectListItem
			{
				Text = t.Name,
				Value = t.Id.ToString()
			})
			.ToListAsync();
		}
	}
}
