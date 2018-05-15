using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;

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

		[Authorize]
		public async Task<IActionResult> My()
		{
			var user = await _userManager.GetUserAsync(User);
			var model = await _userFileTasks.GetUserIndex(user);
			return View(model);
		}

		public async Task<IActionResult> Info(long id)
		{
			var model = await _userFileTasks.GetInfo(id);

			if (model == null)
			{
				return NotFound();
			}

			if (model.Hidden)
			{
				var user = await _userManager.GetUserAsync(User);

				if (user == null || model.Author != user.UserName)
				{
					return NotFound();
				}
			}

			await _userFileTasks.IncrementViewCount(id);
			return View(model);
		}

		public async Task<IActionResult> Download(long id)
		{
			var model = await _userFileTasks.GetContents(id);

			if (model == null)
			{
				return NotFound();
			}

			if (model.Hidden)
			{
				var user = await _userManager.GetUserAsync(User);

				if (user == null || model.AuthorId != user.Id)
				{
					return NotFound();
				}
			}

			await _userFileTasks.IncrementDownloadCount(id);

			var fileType = new FileExtensionContentTypeProvider().TryGetContentType(model.FileName, out var contentType)
				? contentType
				: "application/binary";

			return new FileContentResult(model.Content, fileType)
			{
				FileDownloadName = model.FileName
			};
		}
	}
}
