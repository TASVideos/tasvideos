using System.IO.Compression;
using TASVideos.Data.Entity;

namespace TASVideos.Core.Services;

public interface IFileService
{
	Task<CompressedFile> Compress(byte[] contents);

	/// <summary>
	/// Unzips the file, and re-zips it while renaming the contained file
	/// </summary>
	Task<byte[]> CopyZip(byte[] zipBytes, string fileName);
}

internal class FileService : IFileService
{
	public async Task<CompressedFile> Compress(byte[] contents)
	{
		await using var compressedStream = new MemoryStream();
		await using (var gzipStream = new GZipStream(compressedStream, CompressionLevel.Optimal))
		{
			await using var originalStream = new MemoryStream(contents);

			// This is the default buffer size used by CopyTo
			const int bufferSize = 81920;
			await originalStream.CopyToAsync(gzipStream, bufferSize);
		}

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
		await using var submissionFileStream = new MemoryStream(zipBytes);
		using var submissionZipArchive = new ZipArchive(submissionFileStream, ZipArchiveMode.Read);
		var entries = submissionZipArchive.Entries.ToList();
		var single = entries.First();

		await using var singleStream = new MemoryStream();
		await using var stream = single.Open();
		await stream.CopyToAsync(singleStream);
		var fileBytes = singleStream.ToArray();

		await using var outStream = new MemoryStream();
		using (var archive = new ZipArchive(outStream, ZipArchiveMode.Create, true))
		{
			var fileInArchive = archive.CreateEntry(fileName, CompressionLevel.Optimal);
			// set Unix permissions UserRead | UserWrite | GroupRead | OtherRead
			// this is needed as CreateEntry will 0 the ExternalAttributes field
			// in .NET 7 this could be removed, see https://github.com/dotnet/runtime/commit/bce94154f24e2ae83e5597881029d4f1ba428b60
			fileInArchive.ExternalAttributes = (0x100 | 0x80 | 0x20 | 0x2) << 16;
			await using var entryStream = fileInArchive.Open();
			await using var fileToCompressStream = new MemoryStream(fileBytes);
			await fileToCompressStream.CopyToAsync(entryStream);
		}

		return outStream.ToArray();
	}
}

public record CompressedFile(
	int OriginalSize,
	int CompressedSize,
	Compression Type,
	byte[] Data);
