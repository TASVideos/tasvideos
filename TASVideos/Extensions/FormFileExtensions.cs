using System.IO.Compression;

namespace TASVideos.Extensions;

public static class FormFileExtensions
{
	public static bool IsZip(this IFormFile? formFile)
	{
		if (formFile is null)
		{
			return false;
		}

		var acceptableContentTypes = new[]
		{
			"application/x-zip-compressed",
			"application/zip"
		};

		return formFile.FileName.EndsWith(".zip")
			&& acceptableContentTypes.Contains(formFile.ContentType);
	}

	public static bool IsCompressed(this IFormFile? formFile)
	{
		if (formFile is null)
		{
			return false;
		}

		var compressedExtensions = new[]
		{
			".zip", ".gz", "bz2", ".lzma", ".xz"
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

	public static bool LessThanMovieSizeLimit(this IFormFile? formFile)
	{
		if (formFile is null)
		{
			return true;
		}

		return formFile.Length < SiteGlobalConstants.MaximumMovieSize;
	}

	public static bool IsValidImage(this IFormFile? formFile)
	{
		var validImageTypes = new[]
		{
			"image/png", "image/jpeg"
		};

		return validImageTypes.Contains(formFile?.ContentType);
	}

	public static string FileExtension(this IFormFile? formFile)
	{
		return formFile is null
			? ""
			: Path.GetExtension(formFile.FileName);
	}

	public static async Task<byte[]> ActualFileData(this IFormFile formFile)
	{
		// TODO: TO avoid zip bombs we should limit the max size of tempStream
		var tempStream = new MemoryStream((int)formFile.Length);
		await using var gzip = new GZipStream(formFile.OpenReadStream(), CompressionMode.Decompress);
		await gzip.CopyToAsync(tempStream);
		return tempStream.ToArray();
	}

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
}
