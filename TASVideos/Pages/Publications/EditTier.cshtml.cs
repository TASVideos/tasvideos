using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TASVideos.Core.Services;
using TASVideos.Core.Services.ExternalMediaPublisher;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Extensions;
using TASVideos.Pages.Publications.Models;

namespace TASVideos.Pages.Publications
{
	[RequirePermission(PermissionTo.SetTier)]
	public class EditTierModel : BasePageModel
	{
		private readonly ApplicationDbContext _db;
		private readonly ExternalMediaPublisher _publisher;
		private readonly IPublicationMaintenanceLogger _publicationMaintenanceLogger;

		public EditTierModel(
			ApplicationDbContext db,
			ExternalMediaPublisher publisher,
			IPublicationMaintenanceLogger publicationMaintenanceLogger)
		{
			_db = db;
			_publisher = publisher;
			_publicationMaintenanceLogger = publicationMaintenanceLogger;
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
				.Include(p => p.Tier)
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
				var originalTier = publication.Tier!.Name;
				publication.TierId = Publication.TierId;

				var log = $"Tier changed from {originalTier} to {tier.Name}";
				await _publicationMaintenanceLogger.Log(Id, User.GetUserId(), log);

				var result = await ConcurrentSave(_db, log, "Unable to update tier");
				if (result)
				{
					await _publisher.SendPublicationEdit(
						$"Publication {Id} {Title} {log}",
						$"{Id}M",
						User.Name());
				}
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
