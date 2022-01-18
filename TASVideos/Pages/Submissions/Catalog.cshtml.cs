using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TASVideos.Core.Services.ExternalMediaPublisher;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Data.Entity.Game;
using TASVideos.Extensions;
using TASVideos.Pages.Submissions.Models;
using static TASVideos.Core.Services.AwardAssignment;

namespace TASVideos.Pages.Submissions
{
	[RequirePermission(PermissionTo.CatalogMovies)]
	public class CatalogModel : BasePageModel
	{
		private readonly ApplicationDbContext _db;
		private readonly ExternalMediaPublisher _publisher;

		public CatalogModel(
			ApplicationDbContext db,
			ExternalMediaPublisher publisher)
		{
			_db = db;
			_publisher = publisher;
		}

		[FromRoute]
		public int Id { get; set; }

		[FromQuery]
		public int? GameId { get; set; }

		[FromQuery]
		public int? RomId { get; set; }

		[BindProperty]
		public SubmissionCatalogModel Catalog { get; set; } = new ();

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
					SystemFrameRateId = s.SystemFrameRateId
				})
				.SingleOrDefaultAsync();

			if (Catalog == null)
			{
				return NotFound();
			}

			if (GameId.HasValue)
			{
				var game = await _db.Games.SingleOrDefaultAsync(g => g.Id == GameId && g.SystemId == Catalog.SystemId);
				if (game is not null)
				{
					Catalog.GameId = game.Id;

					// We only want to pre-populate the Rom if a valid Game was provided
					if (RomId.HasValue)
					{
						var rom = await _db.GameRoms.SingleOrDefaultAsync(r => r.GameId == game.Id && r.Id == RomId);
						if (rom is not null)
						{
							Catalog.RomId = rom.Id;
						}
					}
				}
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

			var submission = await _db.Submissions
				.Include(s => s.System)
				.Include(s => s.SystemFrameRate)
				.Include(s => s.Game)
				.Include(s => s.Rom)
				.SingleOrDefaultAsync(s => s.Id == Id);
			if (submission == null)
			{
				return NotFound();
			}

			var externalMessages = new List<string>();

			if (submission.SystemId != Catalog.SystemId)
			{
				var system = await _db.GameSystems.SingleOrDefaultAsync(s => s.Id == Catalog.SystemId!.Value);
				if (system == null)
				{
					ModelState.AddModelError($"{nameof(Catalog)}.{nameof(Catalog.SystemId)}", $"Unknown System Id: {Catalog.SystemId!.Value}");
				}
				else
				{
					externalMessages.Add($"System changed from {submission.System?.Code ?? ""} to {system.Code}");
					submission.SystemId = Catalog.SystemId!.Value;
				}
			}

			if (submission.SystemFrameRateId != Catalog.SystemFrameRateId)
			{
				if (Catalog.SystemFrameRateId.HasValue)
				{
					var systemFramerate = await _db.GameSystemFrameRates.SingleOrDefaultAsync(s => s.Id == Catalog.SystemFrameRateId.Value);
					if (systemFramerate == null)
					{
						ModelState.AddModelError($"{nameof(Catalog)}.{nameof(Catalog.SystemFrameRateId)}", $"Unknown System Framerate Id: {Catalog.SystemFrameRateId.Value}");
					}
					else
					{
						externalMessages.Add($"Framerate changed from {submission.SystemFrameRate?.FrameRate ?? 0.0} to {systemFramerate.FrameRate}");
						submission.SystemFrameRateId = Catalog.SystemFrameRateId.Value;
					}
				}
				else
				{
					externalMessages.Add("Framerate removed");
					submission.SystemFrameRateId = null;
				}
			}

			if (submission.GameId != Catalog.GameId)
			{
				if (Catalog.GameId.HasValue)
				{
					var game = await _db.Games.SingleOrDefaultAsync(s => s.Id == Catalog.GameId.Value);
					if (game == null)
					{
						ModelState.AddModelError($"{nameof(Catalog)}.{nameof(Catalog.GameId)}", $"Unknown Game Id: {Catalog.GameId.Value}");
					}
					else
					{
						externalMessages.Add($"Game changed from {submission.Game?.DisplayName ?? "\"\""} to {game.DisplayName}");
						submission.GameId = Catalog.GameId.Value;
					}
				}
				else if (submission.GameId.HasValue)
				{
					externalMessages.Add("Game removed");
					submission.GameId = null;
				}
			}

			if (submission.RomId != Catalog.RomId)
			{
				if (Catalog.RomId.HasValue)
				{
					var rom = await _db.GameRoms.SingleOrDefaultAsync(s => s.Id == Catalog.RomId.Value);
					if (rom == null)
					{
						ModelState.AddModelError($"{nameof(Catalog)}.{nameof(Catalog.RomId)}", $"Unknown Rom Id: {Catalog.RomId.Value}");
					}
					else
					{
						externalMessages.Add($"Rom Hash changed from {submission.Rom?.Name ?? "\"\""} to {rom.Name}");
						submission.RomId = Catalog.RomId.Value;
					}
				}
				else
				{
					externalMessages.Add("Rom removed");
					submission.RomId = null;
				}
			}

			if (!ModelState.IsValid)
			{
				await PopulateCatalogDropDowns();
				return Page();
			}

			var result = await ConcurrentSave(_db, $"{Id}S catalog updated", $"Unable to save {Id}S catalog");
			if (result)
			{
				await _publisher.SendSubmissionEdit(
					$"{Id}S Catalog edited by {User.Name()}",
					string.Join(", ", externalMessages),
					$"{Id}S");
			}

			return RedirectToPage("View", new { Id });
		}

		private async Task PopulateCatalogDropDowns()
		{
			AvailableRoms = await _db.GameRoms
				.Where(r => !Catalog.SystemId.HasValue || r.Game!.SystemId == Catalog.SystemId)
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
