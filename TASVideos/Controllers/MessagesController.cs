using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

using TASVideos.Data.Entity;
using TASVideos.Models;
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
			var model = await _pmTasks.GetPrivateMessageToUser(user, id);

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

		[Authorize]
		public async Task<IActionResult> Savebox()
		{
			var user = await _userManager.GetUserAsync(User);
			var model = await _pmTasks.GetUserSaveBox(user);
			return View(model);
		}

		[Authorize]
		public async Task<IActionResult> Sentbox()
		{
			var user = await _userManager.GetUserAsync(User);
			var model = await _pmTasks.GetUserSentBox(user);
			return View(model);
		}

		[Authorize]
		public async Task<IActionResult> SaveToUser(int id)
		{
			var user = await _userManager.GetUserAsync(User);
			await _pmTasks.SaveMessageToUser(user, id);
			return RedirectToAction(nameof(Inbox));
		}

		[Authorize]
		public async Task<IActionResult> DeleteToUser(int id)
		{
			var user = await _userManager.GetUserAsync(User);
			await _pmTasks.DeleteMessageToUser(user, id);
			return RedirectToAction(nameof(Inbox));
		}

		[Authorize]
		public IActionResult Create()
		{
			return View(new PrivateMessageCreateModel());
		}

		[Authorize]
		[HttpPost, ValidateAntiForgeryToken]
		public IActionResult Create(PrivateMessageCreateModel model)
		{
			if (!ModelState.IsValid)
			{
				return View(model);
			}

			return RedirectToAction(nameof(Inbox));
		}
	}
}
