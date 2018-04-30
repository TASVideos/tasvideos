using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.WindowsAzure.Storage.Queue.Protocol;

using TASVideos.Data.Entity;
using TASVideos.Models;
using TASVideos.Tasks;

namespace TASVideos.Controllers
{
	public class MessagesController : BaseController
	{
		private readonly UserTasks _userTasks;
		private readonly UserManager<User> _userManager;
		private readonly PrivateMessageTasks _pmTasks;

		public MessagesController(
			UserTasks userTasks,
			UserManager<User> userManager,
			PrivateMessageTasks pmTasks)
			: base(userTasks)
		{
			_userTasks = userTasks;
			_userManager = userManager;
			_pmTasks = pmTasks;
		}

		[Authorize]
		public async Task<IActionResult> Index(int id)
		{
			var user = await _userManager.GetUserAsync(User);
			var model = await _pmTasks.GetMessage(user, id);

			if (model == null)
			{
				return NotFound();
			}

			model.RenderedText = RenderPost(model.Text, model.EnableBbCode, model.EnableHtml);
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
		public async Task<IActionResult> Create(int? replyTo, string toUser = "")
		{
			string subject = "";
			PrivateMessageModel replyingTo = null;
			if (replyTo > 0)
			{
				var user = await _userManager.GetUserAsync(User);
				var message = await _pmTasks.GetMessage(user, replyTo.Value);
				if (message != null)
				{
					subject = "Re: " + message.Subject;
					toUser = message.FromUserName;
					replyingTo = new PrivateMessageModel
					{
						Subject = message.Subject,
						Text = message.Text,
						SentOn = message.SentOn,
						FromUserName = message.FromUserName,
						FromUserId = message.FromUserId,
						ToUserName = message.ToUserName,
						ToUserId = message.ToUserId
					};
				}
			}

			return View(new PrivateMessageCreateModel
			{
				Subject = subject,
				ToUser = toUser,
				ReplyingTo = replyingTo
			});
		}

		[Authorize]
		[HttpPost, ValidateAntiForgeryToken]
		public async Task<IActionResult> Create(PrivateMessageCreateModel model)
		{
			if (User.Identity.Name == model.ToUser)
			{
				ModelState.AddModelError(nameof(PrivateMessageCreateModel.ToUser), "Can not send a message to yourself!");
			}

			if (!ModelState.IsValid)
			{
				return View(model);
			}

			var exists = await _userTasks.CheckUserNameExists(model.ToUser);
			if (!exists)
			{
				ModelState.AddModelError(nameof(PrivateMessageCreateModel.ToUser), $"{model.ToUser} does not exist");
				return View(model);
			}

			var user = await _userManager.GetUserAsync(User);
			await _pmTasks.SendMessage(user, model, IpAddress.ToString());

			return RedirectToAction(nameof(Inbox));
		}
	}
}
