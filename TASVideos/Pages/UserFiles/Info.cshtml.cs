using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;

using AutoMapper.QueryableExtensions;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using TASVideos.Data;
using TASVideos.Models;
using TASVideos.Tasks;

namespace TASVideos.Pages.UserFiles
{
	[AllowAnonymous]
	public class InfoModel : BasePageModel
	{
		private readonly ApplicationDbContext _db;
		private readonly UserFileTasks _userFileTasks;
		
		public InfoModel(
			ApplicationDbContext db,
			UserFileTasks userFileTasks)
		{
			_db = db;
			_userFileTasks = userFileTasks;
		}

		[FromRoute]
		public long Id { get; set; }

		public UserFileModel UserFile { get; set; } = new UserFileModel();

		public async Task<IActionResult> OnGet()
		{
			UserFile = await _db.UserFiles
				.Where(userFile => userFile.Id == Id)
				.ProjectTo<UserFileModel>()
				.SingleOrDefaultAsync();

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
			var file = await _db.UserFiles
				.Where(userFile => userFile.Id == Id)
				.SingleOrDefaultAsync();

			if (file == null)
			{
				return NotFound();
			}

			if (file.Hidden)
			{
				if (!User.Identity.IsAuthenticated || file.AuthorId != User.GetUserId())
				{
					return NotFound();
				}
			}

			file.Downloads++;

			// TODO: handle DbConcurrencyException
			await _db.SaveChangesAsync();

			var stream = new GZipStream(
				new MemoryStream(file.Content),
				CompressionMode.Decompress);

			return new FileStreamResult(stream, "application/x-" + file.Type)
			{
				FileDownloadName = file.FileName
			};
		}
	}
}
