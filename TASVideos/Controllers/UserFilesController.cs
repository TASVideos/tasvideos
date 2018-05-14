using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using TASVideos.Data.Entity;
using TASVideos.Tasks;

namespace TASVideos.Controllers
{
	public class UserFilesController : BaseController
	{
		private readonly UserFileTasks _userFileTasks;
		private readonly UserManager<User> _userManager;

		public UserFilesController(
			UserTasks userTasks,
			UserFileTasks userFileTasks,
			UserManager<User> userManager) : base(userTasks)
		{
			_userFileTasks = userFileTasks;
			_userManager = userManager;
		}

		public async Task<IActionResult> Index()
		{
			var model = await _userFileTasks.GetIndex();
			return View(model);
		}

		public async Task<IActionResult> My()
		{
			var user = await _userManager.GetUserAsync(User);
			var model = await _userFileTasks.GetUserIndex(user);
			return View(model);
		}

		public async Task<IActionResult> Info(long id)
		{
			var model = await _userFileTasks.GetInfo(id);
			return View(model);
		}
	}
}
