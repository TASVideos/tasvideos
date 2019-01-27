using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using TASVideos.Extensions;
using TASVideos.Models;
using TASVideos.Tasks;

namespace TASVideos.Pages.UserFiles
{
	[AllowAnonymous]
	public class InfoModel : BasePageModel
	{
		private readonly UserFileTasks _userFileTasks;
		
		public InfoModel(
			UserFileTasks userFileTasks,
			UserTasks userTasks) 
			: base(userTasks)
		{
			_userFileTasks = userFileTasks;
		}

		[FromRoute]
		public long Id { get; set; }

		public UserFileModel UserFile { get; set; } = new UserFileModel();

		public async Task<IActionResult> OnGet()
		{
			UserFile = await _userFileTasks.GetInfo(Id);
			if (UserFile == null)
			{
				return NotFound();
			}

			if (UserFile.Hidden)
			{
				if (!User.Identity.IsAuthenticated || UserFile.Author != User.Identity.Name)
				{
					return NotFound();
				}
			}

			await _userFileTasks.IncrementViewCount(Id);

			return Page();
		}

		public async Task<IActionResult> OnGetDownload()
		{
			var model = await _userFileTasks.GetContents(Id);

			if (model == null)
			{
				return NotFound();
			}

			if (model.Hidden)
			{
				if (!User.Identity.IsAuthenticated || model.AuthorId != User.GetUserId())
				{
					return NotFound();
				}
			}

			await _userFileTasks.IncrementDownloadCount(Id);

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
