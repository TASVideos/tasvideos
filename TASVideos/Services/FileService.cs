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

namespace TASVideos.Services
{
	public interface IFileService
	{
		/// <summary>
		/// Stores the file with the given name in the database, returning the assigned id.
		/// </summary>
		Task<FileDto> Store(string filename, byte[] contents, CancellationToken cancellationToken = default(CancellationToken));

		/// <summary>
		/// Returns info about the file with the given id, or null if no such file exists.
		/// </summary>
		Task<FileDto> GetMetadata(int id, CancellationToken cancellationToken = default(CancellationToken));

		/// <summary>
		/// Generates a FileStreamResult for the file with the given id that can be returned from a
		/// controller. Returns null if the file does not exist.
		/// </summary>
		Task<FileStreamResult> GetFileStreamResult(int id, CancellationToken cancellationToken = default(CancellationToken));
	}

	public class FileService : IFileService
	{
		private readonly ApplicationDbContext _db;

		public FileService(ApplicationDbContext db)
		{
			_db = db;
		}

		public async Task<FileDto> Store(
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

			return new FileDto
			{
				Id = model.Id,
				Filename = model.Filename,
				Size = model.OriginalSize
			};
		}

		public async Task<FileDto> GetMetadata(
			int id,
			CancellationToken cancellationToken = default(CancellationToken))
		{
			return await _db.Files
				.Select(file => new FileDto
				{
					Id = file.Id,
					Filename = file.Filename,
					Size = file.OriginalSize
				})
				.SingleOrDefaultAsync(file => file.Id == id, cancellationToken);
		}

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

			const string fileType = "application/octet-stream";
			var entityTag = $"db-file-{file.Id}";

			return new FileStreamResult(stream, fileType)
			{
				EntityTag = new EntityTagHeaderValue(entityTag),
				FileDownloadName = file.Filename,
				FileStream = stream
			};
		}
	}
}
