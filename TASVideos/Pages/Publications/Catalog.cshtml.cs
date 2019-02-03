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
using TASVideos.Pages.Publications.Models;

namespace TASVideos.Pages.Publications
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

		public PublicationCatalogModel Catalog { get; set; }

		public IEnumerable<SelectListItem> AvailableRoms { get; set; } = new List<SelectListItem>();
		public IEnumerable<SelectListItem> AvailableGames { get; set; } = new List<SelectListItem>();
		public IEnumerable<SelectListItem> AvailableSystems { get; set; } = new List<SelectListItem>();
		public IEnumerable<SelectListItem> AvailableSystemFrameRates { get; set; } = new List<SelectListItem>();

		public async Task<IActionResult> OnGet()
		{
			Catalog = await _db.Publications
					.Where(p => p.Id == Id)
					.Select(p => new PublicationCatalogModel
					{
						RomId = p.RomId,
						GameId = p.GameId,
						SystemId = p.SystemId,
						SystemFrameRateId = p.SystemFrameRateId,
					})
					.SingleAsync();

			if (Catalog == null)
			{
				return NotFound();
			}

			await PopulateCatalogDropDowns(Catalog.GameId, Catalog.SystemId);

			return Page();
		}

		public async Task<IActionResult> OnPost()
		{
			if (!ModelState.IsValid)
			{
				await PopulateCatalogDropDowns(Catalog.GameId, Catalog.SystemId);
				return Page();
			}

			var publication = await _db.Publications.SingleOrDefaultAsync(s => s.Id == Id);
			if (publication == null)
			{
				return NotFound();
			}

			_mapper.Map(Catalog, publication);

			// TODO: catch DbConcurrencyException
			await _db.SaveChangesAsync();

			return RedirectToPage("View", new { Id });
		}

		private async Task PopulateCatalogDropDowns(int gameId, int systemId)
		{
			using (_db.Database.BeginTransactionAsync())
			{
				AvailableRoms = await _db.Roms
					.ForGame(gameId)
					.ForSystem(systemId)
					.Select(r => new SelectListItem
					{
						Value = r.Id.ToString(),
						Text = r.Name
					})
					.ToListAsync();

				AvailableGames = await _db.Games
					.ForSystem(systemId)
					.Select(g => new SelectListItem
					{
						Value = g.Id.ToString(),
						Text = g.GoodName
					})
					.ToListAsync();

				AvailableSystems = await _db.GameSystems
					.Select(s => new SelectListItem
					{
						Value = s.Id.ToString(),
						Text = s.Code
					})
					.ToListAsync();

				AvailableSystemFrameRates = await _db.GameSystemFrameRates
					.ForSystem(systemId)
					.Select(sf => new SelectListItem
					{
						Value = sf.Id.ToString(),
						Text = sf.RegionCode + " (" + sf.FrameRate + ")"
					})
					.ToListAsync();
			}
		}
	}
}
