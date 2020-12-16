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
			await using var compressedStream = new MemoryStream();
			await using var gzipStream = new GZipStream(compressedStream, CompressionLevel.Optimal);
			await using var originalStream = new MemoryStream(contents);

			// This is the default buffer size used by CopyTo
			const int bufferSize = 81920;
			await originalStream.CopyToAsync(gzipStream, bufferSize);

			byte[] gzipContents = compressedStream.ToArray();

			if (gzipContents.Length < contents.Length)
			{
				return new CompressedFile(
					contents.Length,
					gzipContents.Length,
					Compression.Gzip,
					gzipContents);
			}
			
			return new CompressedFile(
				contents.Length,
				contents.Length,
				Compression.None,
				contents);
		}
	}
	
	public record CompressedFile(
		int OriginalSize,
		int CompressedSize,
		Compression Type,
		byte[] Data);
}
