using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;

using AutoMapper;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using TASVideos.Data;
using TASVideos.Models;

namespace TASVideos.Pages.UserFiles
{
	[AllowAnonymous]
	public class InfoModel : BasePageModel
	{
		private readonly ApplicationDbContext _db;
		private readonly IMapper _mapper;
		
		public InfoModel(
			ApplicationDbContext db,
			IMapper mapper)
		{
			_db = db;
			_mapper = mapper;
		}

		[FromRoute]
		public long Id { get; set; }

		public UserFileModel UserFile { get; set; } = new UserFileModel();

		public async Task<IActionResult> OnGet()
		{
			var file = await _db.UserFiles
				.Include(uf => uf.Author)
				.Include(uf => uf.Game)
				.Include(uf => uf.System)
				.Where(userFile => userFile.Id == Id)
				.SingleOrDefaultAsync();

			if (file == null)
			{
				return NotFound();
			}

			if (file.Hidden)
			{
				if (!User.Identity.IsAuthenticated || UserFile.Author != User.Identity.Name)
				{
					return NotFound();
				}
			}

			UserFile = _mapper.Map<UserFileModel>(file);

			file.Views++;

			// TODO: handle DbConcurrencyException
			await _db.SaveChangesAsync();

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
