using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

using TASVideos.Data.Entity;
using TASVideos.Tasks;

namespace TASVideos.Controllers
{
	public class MessagesController : BaseController
	{
		private readonly UserManager<User> _userManager;
		private readonly PrivateMessageTasks _pmTasks;

		public MessagesController(
			UserTasks userTasks,
			UserManager<User> userManager,
			PrivateMessageTasks pmTasks)
			: base(userTasks)
		{
			_userManager = userManager;
			_pmTasks = pmTasks;
		}

		[Authorize]
		public async Task<IActionResult> Index(int id)
		{
			var user = await _userManager.GetUserAsync(User);
			var model = await _pmTasks.GetPrivateMessage(user, id);

			if (model == null)
			{
				return NotFound();
			}

			return View(model);
		}

		[Authorize]
		public async Task<IActionResult> Inbox()
		{
			var user = await _userManager.GetUserAsync(User);
			var model = await _pmTasks.GetUserInBox(user);
			return View(model);
		}
	}
}
