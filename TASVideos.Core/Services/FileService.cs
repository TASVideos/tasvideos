using System.IO.Compression;

namespace TASVideos.Core.Services;

public interface IFileService
{
	Task<CompressedFile> Compress(byte[] contents);
	Task<string> DecompressGzipToString(byte[] contents);

	/// <summary>
	/// Unzips the file, and re-zips it while renaming the contained file
	/// </summary>
	Task<byte[]> CopyZip(byte[] zipBytes, string fileName);
	Task<byte[]> ZipFile(byte[] fileBytes, string fileName);

	Task<ZippedFile?> GetSubmissionFile(int id);
	Task<ZippedFile?> GetPublicationFile(int id);
	Task<ZippedFile?> GetAdditionalPublicationFile(int publicationId, int fileId);
}

internal class FileService(ApplicationDbContext db) : IFileService
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

	public async Task<string> DecompressGzipToString(byte[] contents)
	{
		await using var ms = new MemoryStream(contents);
		await using var gz = new SharpCompress.Compressors.Deflate.GZipStream(ms, SharpCompress.Compressors.CompressionMode.Decompress);
		using var unzip = new StreamReader(gz);
		return await unzip.ReadToEndAsync();
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

		return await ZipFile(fileBytes, fileName);
	}

	public async Task<byte[]> ZipFile(byte[] fileBytes, string fileName)
	{
		await using var outStream = new MemoryStream();
		using (var archive = new ZipArchive(outStream, ZipArchiveMode.Create, true))
		{
			var fileInArchive = archive.CreateEntry(fileName, CompressionLevel.Optimal);
			await using var entryStream = fileInArchive.Open();
			await using var fileToCompressStream = new MemoryStream(fileBytes);
			await fileToCompressStream.CopyToAsync(entryStream);
		}

		return outStream.ToArray();
	}

	public async Task<ZippedFile?> GetSubmissionFile(int id)
	{
		var data = await db.Submissions
			.Where(s => s.Id == id)
			.Select(s => s.MovieFile)
			.SingleOrDefaultAsync();

		return data is not null
			? new ZippedFile(data, $"submission{id}")
			: null;
	}

	public async Task<ZippedFile?> GetPublicationFile(int id)
	{
		return await db.Publications
			.Where(p => p.Id == id)
			.Select(p => new ZippedFile(p.MovieFile, p.MovieFileName))
			.SingleOrDefaultAsync();
	}

	public async Task<ZippedFile?> GetAdditionalPublicationFile(int publicationId, int fileId)
	{
		var result = await db.PublicationFiles
			.Where(pf => pf.PublicationId == publicationId)
			.Where(pf => pf.Id == fileId)
			.Select(pf => new { pf.FileData, pf.Path })
			.SingleOrDefaultAsync();

		return result?.FileData is not null
			? new ZippedFile(result.FileData, result.Path)
			: null;
	}
}

public record CompressedFile(
	int OriginalSize,
	int CompressedSize,
	Compression Type,
	byte[] Data);

public record ZippedFile(byte[] Data, string Path);
