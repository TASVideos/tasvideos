using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TASVideos.Data;
using TASVideos.Data.Entity;
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
			:base(userTasks)
		{
			_catalogTasks = catalogTasks;
			_platformTasks = platformTasks;
		}

		public async Task<IActionResult> GameList(PagedModel getModel)
		{
			var model = _catalogTasks.GetPageOfGames(getModel);
			//model.AvailableSystems = await _platformTasks.GetGameSystemDropdownList();
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

		public IActionResult RomList(int gameId)
		{
			return View();
		}

		public IActionResult RomView(int romId)
		{
			return View();
		}

		public IActionResult RomEdit(int? romId)
		{
			return View();
		}

		[HttpPost, AutoValidateAntiforgeryToken]
		public IActionResult RomEdit(object model)
		{
			return View();
		}
	}
}
