using System.IO.Compression;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Models;

namespace TASVideos.Pages.UserFiles;

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

	public UserFileModel UserFile { get; set; } = new();

	public async Task<IActionResult> OnGet()
	{
		var file = await _db.UserFiles
			.Include(uf => uf.Comments)
			.ThenInclude(c => c.User)
			.Include(uf => uf.Author)
			.ThenInclude(a => a!.UserFiles)
			.Include(uf => uf.Game)
			.ThenInclude(g => g!.System)
			.Include(uf => uf.System)
			.Where(userFile => userFile.Id == Id)
			.SingleOrDefaultAsync();

		if (file == null)
		{
			return NotFound();
		}

		UserFile = _mapper.Map<UserFileModel>(file);

		// TODO: why is this necessary? The mapper configuration works with ProjectTo, why not here?
		UserFile.Comments = file.Comments
			.Select(_mapper.Map<UserFileModel.UserFileCommentModel>)
			.ToList();

		file.Views++;

		await _db.TrySaveChangesAsync();

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

		file.Downloads++;

		await _db.TrySaveChangesAsync();

		Stream stream;
		if (file.CompressionType == Compression.Gzip)
		{
			stream = new GZipStream(
				new MemoryStream(file.Content),
				CompressionMode.Decompress);
		}
		else
		{
			stream = new MemoryStream(file.Content);
		}

		return new FileStreamResult(stream, "application/x-" + file.Type)
		{
			FileDownloadName = file.FileName
		};
	}
}
