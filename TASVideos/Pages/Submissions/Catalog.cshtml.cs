using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using AutoMapper;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Data.Entity.Game;
using TASVideos.Extensions;
using TASVideos.Pages.Submissions.Models;

namespace TASVideos.Pages.Submissions
{
	[RequirePermission(PermissionTo.CatalogMovies)]
	public class CatalogModel : BasePageModel
	{
		private readonly ApplicationDbContext _db;
		private readonly IMapper _mapper;

		public CatalogModel(
			ApplicationDbContext db,
			IMapper mapper)
		{
			_db = db;
			_mapper = mapper;
		}

		[FromRoute]
		public int Id { get; set; }

		[BindProperty]
		public SubmissionCatalogModel Catalog { get; set; }

		public IEnumerable<SelectListItem> AvailableRoms { get; set; } = new List<SelectListItem>();
		public IEnumerable<SelectListItem> AvailableGames { get; set; } = new List<SelectListItem>();
		public IEnumerable<SelectListItem> AvailableSystems { get; set; } = new List<SelectListItem>();
		public IEnumerable<SelectListItem> AvailableSystemFrameRates { get; set; } = new List<SelectListItem>();

		public async Task<IActionResult> OnGet()
		{
			Catalog = await _db.Submissions
				.Where(s => s.Id == Id)
				.Select(s => new SubmissionCatalogModel
				{
					Title = s.Title,
					RomId = s.RomId,
					GameId = s.GameId,
					SystemId = s.SystemId,
					SystemFrameRateId = s.SystemFrameRateId,
				})
				.SingleOrDefaultAsync();

			if (Catalog == null)
			{
				return NotFound();
			}

			await PopulateCatalogDropDowns();
			return Page();
		}

		public async Task<IActionResult> OnPost()
		{
			if (!ModelState.IsValid)
			{
				await PopulateCatalogDropDowns();
				return Page();
			}

			var submission = await _db.Submissions.SingleAsync(s => s.Id == Id);
			_mapper.Map(Catalog, submission);

			// TODO: catch DbConcurrencyException
			await _db.SaveChangesAsync();

			return RedirectToPage("View", new { Id });
		}

		private async Task PopulateCatalogDropDowns()
		{
			using (_db.Database.BeginTransactionAsync())
			{
				AvailableRoms = await _db.Roms
					.Where(r => !Catalog.SystemId.HasValue || r.Game.SystemId == Catalog.SystemId)
					.Where(r => !Catalog.GameId.HasValue || r.GameId == Catalog.GameId)
					.OrderBy(r => r.Name)
					.Select(r => new SelectListItem
					{
						Value = r.Id.ToString(),
						Text = r.Name
					})
					.ToListAsync();

				AvailableGames = await _db.Games
					.Where(g => !Catalog.SystemId.HasValue || g.SystemId == Catalog.SystemId)
					.OrderBy(g => g.DisplayName)
					.ToDropDown()
					.ToListAsync();

				AvailableSystems = await _db.GameSystems
					.OrderBy(s => s.Code)
					.Select(s => new SelectListItem
					{
						Value = s.Id.ToString(),
						Text = s.Code
					})
					.ToListAsync();

				AvailableSystemFrameRates = Catalog.SystemId.HasValue
					? await _db.GameSystemFrameRates
						.ForSystem(Catalog.SystemId.Value)
						.ToDropDown()
						.ToListAsync()
					: new List<SelectListItem>();
			}
		}
	}
}
