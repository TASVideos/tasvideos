using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

using TASVideos.Data;
using TASVideos.Data.Entity;

namespace TASVideos.Services
{
	public interface IMediaFileUploader
	{
		Task UploadScreenshot(int publicationId, IFormFile screenshot, string description);
	}

	public class MediaFileUploader : IMediaFileUploader
	{
		private readonly ApplicationDbContext _db;
		private readonly IHostingEnvironment _env;

		public MediaFileUploader(ApplicationDbContext db, IHostingEnvironment env)
		{
			_db = db;
			_env = env;
		}

		public async Task UploadScreenshot(int publicationId, IFormFile screenshot, string description)
		{
			using (var memoryStream = new MemoryStream())
			{
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
			}
		}
	}
}
