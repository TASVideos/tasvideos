using Microsoft.Net.Http.Headers;

namespace TASVideos.Pages.UserFiles;

[AllowAnonymous]
public class InfoModel(ApplicationDbContext db, IFileService fileService) : BasePageModel
{
	private static readonly string[] PreviewableExtensions = ["avs", "bat", "cfg", "lua", "sh", "uae", "wch"];

	[FromRoute]
	public long Id { get; set; }

	public UserFileModel UserFile { get; set; } = new();

	public async Task<IActionResult> OnGet()
	{
		var file = await db.UserFiles
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
			var entity = await db.UserFiles.FindAsync(UserFile.Id);
			UserFile.Content = entity!.Content;
			UserFile.CompressionType = entity.CompressionType;

			if (UserFile.CompressionType == Compression.Gzip)
			{
				UserFile.ContentPreview = await fileService.DecompressGzipToString(UserFile.Content);
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
		var file = await db.UserFiles
			.Where(userFile => userFile.Id == Id)
			.SingleOrDefaultAsync();

		if (file is null)
		{
			return NotFound();
		}

		file.Downloads++;

		await db.TrySaveChanges();
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

	public class UserFileModel
	{
		public long Id { get; init; }
		public UserFileClass Class { get; init; }
		public string Title { get; init; } = "";
		public string? Description { get; init; }
		public DateTime UploadTimestamp { get; init; }
		public string Author { get; init; } = "";
		public int AuthorUserFilesCount { get; init; }
		public int Downloads { get; init; }
		public bool Hidden { get; init; }
		public string? FileName { get; init; }
		public int FileSizeUncompressed { get; init; }
		public int FileSizeCompressed { get; init; }
		public int? GameId { get; init; }
		public string? GameName { get; init; }
		public string? GameSystem { get; init; }
		public string? System { get; init; }

		// Only relevant to Movies
		public TimeSpan Time => TimeSpan.FromSeconds(Math.Round((double)Length, 2, MidpointRounding.AwayFromZero));
		public bool IsMovie => Class == UserFileClass.Movie;

		public decimal Length { get; init; }
		public int Frames { get; init; }
		public int Rerecords { get; init; }

		public List<Comment> Comments { get; init; } = [];
		public bool HideComments { get; init; }
		public Compression CompressionType { get; set; }
		public byte[] Content { get; set; } = [];
		public string ContentPreview { get; set; } = "";

		public string Extension => (FileName ?? "").ToLower().Split('.').Last();
		public string? Annotations { get; init; }

		public record Comment(int Id, string Text, DateTime CreationTimeStamp, int UserId, string UserName);
	}
}
