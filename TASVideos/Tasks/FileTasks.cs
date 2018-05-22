using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Net.Http.Headers;

using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Models;

namespace TASVideos.Tasks
{
	public class FileTasks
	{
		private readonly ApplicationDbContext _db;

		public object UTF8 { get; private set; }

		public FileTasks(ApplicationDbContext db)
		{
			_db = db;
		}

		/// <summary>
		/// Stores the file with the given name in the database, returning the assigned id.
		/// </summary>
		public async Task<FileViewModel> StoreFile(
			string filename,
			byte[] contents,
			CancellationToken cancellationToken = default(CancellationToken))
		{
			byte[] gzipContents;

			using (var compressedStream = new MemoryStream())
			{
				using (var gzipStream = new GZipStream(compressedStream, CompressionLevel.Optimal))
				using (var originalStream = new MemoryStream(contents))
				{
					// This is the default buffer size used by CopyTo
					const int bufferSize = 81920;
					await originalStream.CopyToAsync(gzipStream, bufferSize, cancellationToken);
				}

				gzipContents = compressedStream.ToArray();
			}

			var model = new DatabaseFile
			{
				Filename = filename,
				OriginalSize = contents.Length
			};

			if (gzipContents.Length < contents.Length)
			{
				model.CompressedSize = gzipContents.Length;
				model.Compression = Compression.Gzip;
				model.Data = gzipContents;
			}
			else
			{
				model.CompressedSize = contents.Length;
				model.Compression = Compression.None;
				model.Data = contents;
			}

			await _db.Files.AddAsync(model, cancellationToken);
			await _db.SaveChangesAsync(cancellationToken);

			return new FileViewModel
			{
				Id = model.Id,
				Filename = model.Filename,
				Size = model.OriginalSize
			};
		}

		/// <summary>
		/// Returns info about the file with the given id, or null if no such file exists.
		/// </summary>
		public async Task<FileViewModel> GetFileInfo(
			int id,
			CancellationToken cancellationToken = default(CancellationToken))
		{
			return await _db.Files
				.Select(file => new FileViewModel
				{
					Id = file.Id,
					Filename = file.Filename,
					Size = file.OriginalSize
				})
				.SingleOrDefaultAsync(file => file.Id == id, cancellationToken);
		}

		/// <summary>
		/// Generates a FileStreamResult for the file with the given id that can be returned from a
		/// controller. Returns null if the file does not exist.
		/// </summary>
		public async Task<FileStreamResult> GetFileStreamResult(
			int id,
			CancellationToken cancellationToken = default(CancellationToken))
		{
			var file = await _db.Files.SingleOrDefaultAsync(f => f.Id == id, cancellationToken);

			if (file == null)
			{
				return null;
			}

			var memoryStream = new MemoryStream(file.Data);

			var stream = file.Compression == Compression.Gzip
				? (Stream)new GZipStream(memoryStream, CompressionMode.Decompress)
				: (Stream)memoryStream;

			const string filetype = "application/octet-stream";
			var entityTag = $"db-file-{file.Id}";

			return new FileStreamResult(stream, filetype)
			{
				EntityTag = new EntityTagHeaderValue(entityTag),
				FileDownloadName = file.Filename,
				FileStream = stream
			};
		}
	}
}
