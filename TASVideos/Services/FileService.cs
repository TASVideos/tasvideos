using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using TASVideos.Data.Entity;

namespace TASVideos.Services
{
	public interface IFileService
	{
		Task<CompressedFile> Compress(byte[] contents);
		
		/// <summary>
		/// Unzips the file, and re-zips it while renaming the contained file
		/// </summary>
		Task<byte[]> CopyZip(byte[] zipBytes, string fileName);
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
		
		public async Task<byte[]> CopyZip(byte[] zipBytes, string fileName)
		{
			await using var newFileStream = new MemoryStream();
			using var publicationZipArchive = new ZipArchive(newFileStream, ZipArchiveMode.Create);
			await using var submissionFileStream = new MemoryStream(zipBytes);
			using var submissionZipArchive = new ZipArchive(submissionFileStream, ZipArchiveMode.Read);
			
			var publicationZipEntry = publicationZipArchive.CreateEntry(fileName);
			var submissionZipEntry = submissionZipArchive.Entries.Single();

			await using var publicationZipEntryStream = publicationZipEntry.Open();
			await using var submissionZipEntryStream = submissionZipEntry.Open();
			await submissionZipEntryStream.CopyToAsync(publicationZipEntryStream);

			return newFileStream.ToArray();
		}
	}
	
	public record CompressedFile(
		int OriginalSize,
		int CompressedSize,
		Compression Type,
		byte[] Data);
}
