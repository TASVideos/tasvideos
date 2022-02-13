using System.IO.Compression;
using Microsoft.AspNetCore.Mvc;
using TASVideos.Data.Entity;

namespace TASVideos.Core.Services;

public interface IFileService
{
	Task<CompressedFile> Compress(byte[] contents);

	/// <summary>
	/// Unzips the file, and re-zips it while renaming the contained file
	/// </summary>
	Task<byte[]> CopyZip(byte[] zipBytes, string fileName);

	Microsoft.AspNetCore.Mvc.IActionResult CreateDownloadResult(UserFile file);
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
			await using var entryStream = fileInArchive.Open();
			await using var fileToCompressStream = new MemoryStream(fileBytes);
			await fileToCompressStream.CopyToAsync(entryStream);
		}

		return outStream.ToArray();
	}

	public Microsoft.AspNetCore.Mvc.IActionResult CreateDownloadResult(UserFile file)
	{
		return new DownloadResult(file);
	}

	private class DownloadResult : Microsoft.AspNetCore.Mvc.IActionResult
	{
		public UserFile File { get; init; }

		public DownloadResult(UserFile file)
		{
			File = file;
		}

		public Task ExecuteResultAsync(ActionContext context)
		{
			var res = context.HttpContext.Response;

			res.Headers.Add("Content-Length", File.LogicalLength.ToString());
			if (File.CompressionType == Compression.Gzip)
			{
				res.Headers.Add("Content-Encoding", "gzip");
			}
			res.Headers.Add("Content-Type", "application/octet-stream");
			var contentDisposition = new Microsoft.Net.Http.Headers.ContentDispositionHeaderValue("attachment");
			contentDisposition.SetHttpFileName(File.FileName);
			res.Headers.ContentDisposition = contentDisposition.ToString();
			
			res.StatusCode = 200;
			return res.Body.WriteAsync(File.Content, 0, File.Content.Length);
		}
	}
}

public record CompressedFile(
	int OriginalSize,
	int CompressedSize,
	Compression Type,
	byte[] Data);
