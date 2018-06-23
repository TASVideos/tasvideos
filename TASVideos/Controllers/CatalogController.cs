using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TASVideos.Data.Entity;
using TASVideos.Data.Entity.Game;
using TASVideos.Filter;
using TASVideos.Models;
using TASVideos.Tasks;

namespace TASVideos.Controllers
{
	[RequirePermission(PermissionTo.CatalogMovies)]
	public class CatalogController : BaseController
	{
		private readonly CatalogTasks _catalogTasks;
		private readonly PlatformTasks _platformTasks;

		public CatalogController(
			CatalogTasks catalogTasks,
			PlatformTasks platformTasks,
			UserTasks userTasks)
			: base(userTasks)
		{
			_catalogTasks = catalogTasks;
			_platformTasks = platformTasks;
		}

		public async Task<IActionResult> GameList(GameListRequest getModel)
		{
			var model = _catalogTasks.GetPageOfGames(getModel);
			var systems = (await _platformTasks.GetGameSystemDropdownList()).ToList();
			systems.Insert(0, new SelectListItem { Text = "All", Value = "" });
			ViewData["GameSystemList"] = systems;
			return View(model);
		}

		public async Task<IActionResult> GameEdit(int? id)
		{
			var model = id.HasValue
				? (await _catalogTasks.GetGameForEdit(id.Value))
				: new GameEditModel();

			model.AvailableSystems = await _platformTasks.GetGameSystemDropdownList();

			return View(model);
		}

		[HttpPost, AutoValidateAntiforgeryToken]
		public async Task<IActionResult> GameEdit(GameEditModel model)
		{
			if (!ModelState.IsValid)
			{
				model.AvailableSystems = await _platformTasks.GetGameSystemDropdownList();
				return View(model);
			}

			await _catalogTasks.AddUpdateGame(model);
			return RedirectToAction(nameof(GameList));
		}

		public async Task<IActionResult> DeleteGame(int id)
		{
			var result = await _catalogTasks.DeleteGame(id);
			if (result)
			{
				return RedirectToAction(nameof(GameList));
			}

			return new ContentResult
			{
				Content = $"Unable to delete Game {id}, game is used by a publication or submission",
				StatusCode = (int)HttpStatusCode.BadRequest
			};
		}

		public async Task<IActionResult> RomList(int gameId)
		{
			var model = await _catalogTasks.GetRomsForGame(gameId);

			if (model == null)
			{
				return NotFound();
			}

			return View(model);
		}

		public async Task<IActionResult> RomEdit(int gameId, int? romId)
		{
			var model = await _catalogTasks.GetRomForEdit(gameId, romId);
			model.AvailableRomTypes = Enum.GetValues(typeof(RomTypes))
				.Cast<RomTypes>()
				.Select(r => new SelectListItem
				{
					Text = r.ToString(),
					Value = ((int)r).ToString()
				});

			return View(model);
		}

		[HttpPost, AutoValidateAntiforgeryToken]
		public async Task<IActionResult> RomEdit(RomEditModel model)
		{
			if (!ModelState.IsValid)
			{
				model.AvailableRomTypes = Enum.GetValues(typeof(RomTypes))
					.Cast<RomTypes>()
					.Select(r => new SelectListItem
					{
						Text = r.ToString(),
						Value = ((int)r).ToString()
					});
				return View(model);
			}

			await _catalogTasks.AddUpdateRom(model);
			return RedirectToAction(nameof(RomList), new { gameId = model.GameId });
		}

		public async Task<IActionResult> DeleteRom(int gameId, int romId)
		{
			var result = await _catalogTasks.DeleteRom(romId);
			if (result)
			{
				return RedirectToAction(nameof(RomList), new { gameId });
			}

			return new ContentResult
			{
				Content = $"Unable to delete Rom {romId}, rom is used by a publication or submission",
				StatusCode = (int)HttpStatusCode.BadRequest
			};
		}

		public async Task<IActionResult> FrameRateDropDownForSystem(int systemId, bool includeEmpty)
		{
			var model = await _catalogTasks.GetFrameRateDropDownForSystem(systemId, includeEmpty);
			return PartialView("~/Views/Shared/_DropdownItems.cshtml", model);
		}

		public async Task<IActionResult> GameDropDownForSystem(int systemId, bool includeEmpty)
		{
			var model = await _catalogTasks.GetGameDropDownForSystem(systemId, includeEmpty);
			return PartialView("~/Views/Shared/_DropdownItems.cshtml", model);
		}

		public async Task<IActionResult> RomDropDownForGame(int gameId, bool includeEmpty)
		{
			var model = await _catalogTasks.GetRomDropDownForGame(gameId, includeEmpty);
			return PartialView("~/Views/Shared/_DropdownItems.cshtml", model);
		}
	}
}
