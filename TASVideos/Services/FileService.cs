using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using TASVideos.Data.Entity;

namespace TASVideos.Services
{
	public interface IFileService
	{
		Task<CompressedFile> Compress(byte[] contents);
	}

	public class FileService : IFileService
	{
		public async Task<CompressedFile> Compress(byte[] contents)
		{
			byte[] gzipContents;

			await using (var compressedStream = new MemoryStream())
			{
				await using (var gzipStream = new GZipStream(compressedStream, CompressionLevel.Optimal))
				{
					await using var originalStream = new MemoryStream(contents);

					// This is the default buffer size used by CopyTo
					const int bufferSize = 81920;
					await originalStream.CopyToAsync(gzipStream, bufferSize);
				}

				gzipContents = compressedStream.ToArray();
			}

			var result = new CompressedFile
			{
				OriginalSize = contents.Length
			};


			if (gzipContents.Length < contents.Length)
			{
				result.CompressedSize = gzipContents.Length;
				result.Type = Compression.Gzip;
				result.Data = gzipContents;
			}
			else
			{
				result.CompressedSize = contents.Length;
				result.Type = Compression.None;
				result.Data = contents;
			}

			return result;
		}
	}
}
