using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace TASVideos.Core.Services;

public interface IMediaFileUploader
{
	Task<string> UploadScreenshot(int publicationId, IFormFile screenshot, string? description);
	Task<DeletedFile?> DeleteFile(int publicationFileId);
	Task UploadAwardImage(IFormFile image, IFormFile image2X, IFormFile image4X, string shortName, int? year = null);
	void DeleteAwardImage(string shortName);
	bool AwardExists(string shortName, int year);
}

internal class MediaFileUploader(ApplicationDbContext db, IWebHostEnvironment env) : IMediaFileUploader
{
	private const string AwardLocation = "awards";
	private const string MediaLocation = "media";

	public async Task<string> UploadScreenshot(int publicationId, IFormFile screenshot, string? description)
	{
		await using var memoryStream = new MemoryStream();
		await screenshot.CopyToAsync(memoryStream);
		var screenshotBytes = memoryStream.ToArray();

		string screenshotFileName = $"{publicationId}M{Path.GetExtension(screenshot.FileName)}";
		string screenshotPath = Path.Combine(env.WebRootPath, MediaLocation, screenshotFileName);

		var screenShotExists = File.Exists(screenshotPath);
		await File.WriteAllBytesAsync(screenshotPath, screenshotBytes);

		List<PublicationFile> publicationFiles = [];
		if (screenShotExists)
		{
			// Should never be more than 1, but just in case
			publicationFiles = await db.PublicationFiles
				.Where(pf => pf.PublicationId == publicationId && pf.Path == screenshotFileName)
				.ToListAsync();
		}

		if (screenShotExists && publicationFiles.Any())
		{
			foreach (var file in publicationFiles)
			{
				if (file.Description != description)
				{
					file.Description = description;
				}
			}
		}
		else
		{
			db.PublicationFiles.Add(new PublicationFile
			{
				PublicationId = publicationId,
				Path = screenshotFileName,
				Type = FileType.Screenshot,
				Description = description
			});
		}

		await db.SaveChangesAsync();
		return screenshotFileName;
	}

	public async Task<DeletedFile?> DeleteFile(int publicationFileId)
	{
		var file = await db.PublicationFiles
			.SingleOrDefaultAsync(pf => pf.Id == publicationFileId);

		if (file is null)
		{
			return null;
		}

		string path = Path.Combine(env.WebRootPath, file.Path);
		File.Delete(path);

		db.PublicationFiles.Remove(file);
		await db.SaveChangesAsync();
		return new DeletedFile(file.Id, file.Type, file.Path);
	}

	public async Task UploadAwardImage(IFormFile image, IFormFile image2X, IFormFile image4X, string shortName, int? year = null)
	{
		await UploadAwardImageBase(image, shortName, year, AwardSizeVariant.X1);
		await UploadAwardImageBase(image2X, shortName, year, AwardSizeVariant.X2);
		await UploadAwardImageBase(image4X, shortName, year, AwardSizeVariant.X4);
	}

	public void DeleteAwardImage(string shortName)
	{
		DeleteAwardImageBase(shortName, AwardSizeVariant.X1);
		DeleteAwardImageBase(shortName, AwardSizeVariant.X2);
		DeleteAwardImageBase(shortName, AwardSizeVariant.X4);
	}

	private async Task UploadAwardImageBase(IFormFile image, string shortName, int? year, AwardSizeVariant size)
	{
		await using var memoryStream = new MemoryStream();
		await image.CopyToAsync(memoryStream);
		var screenshotBytes = memoryStream.ToArray();
		string screenshotPath = ToFullAwardPath(shortName, year, size);

		await File.WriteAllBytesAsync(screenshotPath, screenshotBytes);
	}

	private void DeleteAwardImageBase(string shortName, AwardSizeVariant size)
	{
		string path = ToFullAwardPath(shortName, size: size);
		if (File.Exists(path))
		{
			File.Delete(path);
		}
	}

	public bool AwardExists(string shortName, int year)
	{
		var path = ToFullAwardPath(shortName, year);
		return File.Exists(path);
	}

	private string ToFullAwardPath(string shortName, int? year = null, AwardSizeVariant size = AwardSizeVariant.X1)
	{
		string yearString = year is not null ? ((int)year).ToString() : "xxxx";
		string suffix = size switch
		{
			AwardSizeVariant.X1 => "",
			AwardSizeVariant.X2 => "-2x",
			AwardSizeVariant.X4 => "-4x",
			_ => "",
		};
		return Path.Combine(env.WebRootPath, AwardLocation, yearString, $"{shortName}_{yearString}{suffix}.png");
	}
}

public record DeletedFile(int Id, FileType Type, string Path);

public enum AwardSizeVariant
{
	X1,
	X2,
	X4,
}
