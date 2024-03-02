using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using TASVideos.Core.Services;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Models;

namespace TASVideos.Pages.UserFiles;

[AllowAnonymous]
public class InfoModel : BasePageModel
{
	private static readonly string[] PreviewableExtensions = ["avs", "bat", "lua", "sh", "wch"];

	private readonly ApplicationDbContext _db;
	private readonly IFileService _fileService;

	public InfoModel(
		ApplicationDbContext db, IFileService fileService)
	{
		_db = db;
		_fileService = fileService;
	}

	[FromRoute]
	public long Id { get; set; }

	public UserFileModel UserFile { get; set; } = new();

	public async Task<IActionResult> OnGet()
	{
		var file = await _db.UserFiles
			.Where(userFile => userFile.Id == Id)
			.ToUserFileModel(false)
			.SingleOrDefaultAsync();

		if (file is null)
		{
			return NotFound();
		}

		UserFile = file;

		if (UserFile.Class == UserFileClass.Support && PreviewableExtensions.Contains(UserFile.Extension))
		{
			// We are going back to the database on purpose here, because it is important to never query the entire file when getting lists of files, only when getting a single file
			var entity = await _db.UserFiles.FindAsync(UserFile.Id);
			UserFile.Content = entity!.Content;
			UserFile.CompressionType = entity.CompressionType;

			if (UserFile.CompressionType == Compression.Gzip)
			{
				UserFile.ContentPreview = await _fileService.DecompressGzipToString(UserFile.Content);
			}
			else
			{
				UserFile.ContentPreview = System.Text.Encoding.UTF8.GetString(UserFile.Content, 0, UserFile.Content.Length);
			}
		}

		return Page();
	}

	public async Task<IActionResult> OnGetDownload()
	{
		var file = await _db.UserFiles
			.Where(userFile => userFile.Id == Id)
			.SingleOrDefaultAsync();

		if (file is null)
		{
			return NotFound();
		}

		file.Downloads++;

		await _db.TrySaveChangesAsync();
		return new DownloadResult(file);
	}

	internal class DownloadResult(UserFile file) : IActionResult
	{
		public Task ExecuteResultAsync(ActionContext context)
		{
			var res = context.HttpContext.Response;

			res.Headers.Append("Content-Length", file.Content.Length.ToString());
			if (file.CompressionType == Compression.Gzip)
			{
				res.Headers.Append("Content-Encoding", "gzip");
			}

			res.Headers.Append("Content-Type", "application/octet-stream");
			var contentDisposition = new ContentDispositionHeaderValue("attachment");
			contentDisposition.SetHttpFileName(file.FileName);
			res.Headers.ContentDisposition = contentDisposition.ToString();
			res.StatusCode = 200;
			return res.Body.WriteAsync(file.Content, 0, file.Content.Length);
		}
	}
}
