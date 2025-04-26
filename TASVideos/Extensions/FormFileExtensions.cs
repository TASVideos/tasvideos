using System.IO.Compression;
using System.Runtime.CompilerServices;
using System.Security.Claims;

using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace TASVideos.Extensions;

public static class FormFileExtensions
{
	public static bool IsZip(this IFormFile? formFile)
		=> formFile is not null
			&& formFile.FileName.EndsWith(".zip")
			&& formFile.ContentType is "application/x-zip-compressed" or "application/zip";

	public static bool IsCompressed(this IFormFile? formFile)
	{
		if (formFile is null)
		{
			return false;
		}

		var compressedExtensions = new[]
		{
			".zip", ".gz", ".bz2", ".lzma", ".xz"
		};

		var compressedContentTypes = new[]
		{
			"application/x-zip-compressed",
			"application/zip",
			"applicationx-gzip"
		};

		return compressedExtensions.Contains(Path.GetExtension(formFile.FileName))
			|| compressedContentTypes.Contains(formFile.ContentType);
	}

	public static void AddModelErrorIfOverSizeLimit(
		this IFormFile movie,
		ModelStateDictionary modelState,
		ClaimsPrincipal user,
		[CallerArgumentExpression(nameof(movie))] string movieFieldName = default!)
	{
		if (!user.Has(PermissionTo.OverrideSubmissionConstraints) && movie.Length >= SiteGlobalConstants.MaximumMovieSize)
		{
			modelState.AddModelError(movieFieldName, "File is too big, are you sure this is a valid movie file?");
		}
	}

	public static bool IsValidImage(this IFormFile? formFile)
		=> formFile?.ContentType is "image/png" or "image/jpeg";

	public static string FileExtension(this IFormFile? formFile)
		=> Path.GetExtension(formFile?.FileName ?? "");

	public static async Task<byte[]> ToBytes(this IFormFile? formFile)
	{
		if (formFile is null)
		{
			return [];
		}

		await using var memoryStream = new MemoryStream();
		await formFile.CopyToAsync(memoryStream);
		return memoryStream.ToArray();
	}

	/// <summary>
	/// Attempts to decompress the form file from the gzip format. If decompression fails, it returns the raw bytes.
	/// </summary>
	public static async Task<MemoryStream> DecompressOrTakeRaw(this IFormFile? formFile)
	{
		if (formFile is null)
		{
			return new MemoryStream();
		}

		var rawFileStream = new MemoryStream();
		await formFile.CopyToAsync(rawFileStream);

		try
		{
			rawFileStream.Position = 0;
			using var gzip = new GZipStream(rawFileStream, CompressionMode.Decompress, leaveOpen: true); // leaveOpen in case of an exception
			var decompressedFileStream = new MemoryStream(); // TODO: To avoid zip bombs we should limit the max size of this MemoryStream
			await gzip.CopyToAsync(decompressedFileStream);
			await rawFileStream.DisposeAsync(); // manually dispose because we specified leaveOpen
			decompressedFileStream.Position = 0;
			return decompressedFileStream;
		}
		catch (InvalidDataException) // happens if the file was uploaded without compression (e.g., no JavaScript), so we continue and return the raw bytes
		{
			rawFileStream.Position = 0;
			return rawFileStream;
		}
	}
}
