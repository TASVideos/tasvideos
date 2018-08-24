using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

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

		[AllowAnonymous]
		[Route("[controller]/user/{id}")]
		public async Task<IActionResult> FilesUser(string id)
		{
			var userId = await UserTasks.GetUserIdByName(id);
			var model = await _userFileTasks.GetUserIndex(userId, includeHidden: false);

			return View($"~/Views/Profile/{nameof(ProfileController.UserFiles)}.cshtml", model);
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

			var stream = new GZipStream(
				new MemoryStream(model.Content),
				CompressionMode.Decompress);

			return new FileStreamResult(stream, "application/x-" + model.FileType)
			{
				FileDownloadName = model.FileName
			};
		}
	}
}
