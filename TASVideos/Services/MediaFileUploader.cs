using System.IO;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using TASVideos.Data;
using TASVideos.Data.Entity;

namespace TASVideos.Services
{
	public interface IMediaFileUploader
	{
		Task<string> UploadScreenshot(int publicationId, IFormFile screenshot, string? description);
		Task<string> UploadTorrent(int publicationId, IFormFile torrent);
		Task<PublicationFile?> DeleteFile(int publicationFileId);
	}

	public class MediaFileUploader : IMediaFileUploader
	{
		private readonly ApplicationDbContext _db;
		private readonly IWebHostEnvironment _env;

		public MediaFileUploader(ApplicationDbContext db, IWebHostEnvironment env)
		{
			_db = db;
			_env = env;
		}

		public async Task<string> UploadScreenshot(int publicationId, IFormFile screenshot, string? description)
		{
			await using var memoryStream = new MemoryStream();
			await screenshot.CopyToAsync(memoryStream);
			var screenshotBytes = memoryStream.ToArray();

			string screenshotFileName = $"{publicationId}M{Path.GetExtension(screenshot.FileName)}";
			string screenshotPath = Path.Combine(_env.WebRootPath, "media", screenshotFileName);
			File.WriteAllBytes(screenshotPath, screenshotBytes);

			var pubFile = new PublicationFile
			{
				PublicationId = publicationId,
				Path = screenshotFileName,
				Type = FileType.Screenshot,
				Description = description
			};

			_db.PublicationFiles.Add(pubFile);
			await _db.SaveChangesAsync();
			return screenshotFileName;
		}

		public async Task<string> UploadTorrent(int publicationId, IFormFile torrent)
		{
			await using var memoryStream = new MemoryStream();
			await torrent.CopyToAsync(memoryStream);
			var torrentBytes = memoryStream.ToArray();

			string torrentFileName = torrent.FileName;
			string torrentPath = Path.Combine(_env.WebRootPath, "torrent", torrentFileName);
			File.WriteAllBytes(torrentPath, torrentBytes);

			var torrentFile = new PublicationFile
			{
				PublicationId = publicationId,
				Path = torrentFileName,
				Type = FileType.Torrent
			};
			_db.PublicationFiles.Add(torrentFile);
			await _db.SaveChangesAsync();
			return torrentFileName;
		}

		public async Task<PublicationFile?> DeleteFile(int publicationFileId)
		{
			var file = await _db.PublicationFiles
				.SingleOrDefaultAsync(pf => pf.Id == publicationFileId);

			if (file is not null)
			{
				string path = Path.Combine(_env.WebRootPath, "torrent", file.Path);
				File.Delete(path);

				_db.PublicationFiles.Remove(file);
				await _db.SaveChangesAsync();
			}

			return file;
		}
	}
}
