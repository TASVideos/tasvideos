using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TASVideos.Tasks;

namespace TASVideos.Controllers
{
	public class GameController : BaseController
	{
		private readonly GameTasks _gameTasks;

		public GameController(UserTasks userTasks, GameTasks gameTasks)
			: base(userTasks)
		{
			_gameTasks = gameTasks;
		}

		[AllowAnonymous]
		public async Task<IActionResult> View(int id)
		{
			var model = await _gameTasks.GetGameForDisplay(id);
			if (model == null)
			{
				return NotFound();
			}

			return View(model);
		}
	}
}
