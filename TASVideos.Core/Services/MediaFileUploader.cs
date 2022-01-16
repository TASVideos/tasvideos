using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using TASVideos.Data;
using TASVideos.Data.Entity;

namespace TASVideos.Core.Services
{
	public interface IMediaFileUploader
	{
		Task<string> UploadScreenshot(int publicationId, IFormFile screenshot, string? description);
		Task<DeletedFile?> DeleteFile(int publicationFileId);
	}

	internal class MediaFileUploader : IMediaFileUploader
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

			var screenShotExists = File.Exists(screenshotPath);
			await File.WriteAllBytesAsync(screenshotPath, screenshotBytes);

			List<PublicationFile> publicationFiles = new ();
			if (screenShotExists)
			{
				// Should never be more than 1, but just in case
				publicationFiles = await _db.PublicationFiles
					.Where(pf => pf.PublicationId == publicationId && pf.Path == screenshotFileName)
					.ToListAsync();
			}

			if (screenShotExists && publicationFiles.Any())
			{
				foreach (var file in publicationFiles)
				{
					if (file.Description != description)
					{
						file.Description = description;
					}
				}
			}
			else
			{
				_db.PublicationFiles.Add(new PublicationFile
				{
					PublicationId = publicationId,
					Path = screenshotFileName,
					Type = FileType.Screenshot,
					Description = description
				});
			}

			await _db.SaveChangesAsync();
			return screenshotFileName;
		}

		public async Task<DeletedFile?> DeleteFile(int publicationFileId)
		{
			var file = await _db.PublicationFiles
				.SingleOrDefaultAsync(pf => pf.Id == publicationFileId);

			if (file is not null)
			{
				string path = Path.Combine(_env.WebRootPath, file.Path);
				File.Delete(path);

				_db.PublicationFiles.Remove(file);
				await _db.SaveChangesAsync();
				return new DeletedFile(file.Id, file.Type, file.Path);
			}

			return null;
		}
	}

	public record DeletedFile(int Id, FileType Type, string Path);
}
