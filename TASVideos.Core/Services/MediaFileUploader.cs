﻿using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace TASVideos.Core.Services;

public interface IMediaFileUploader
{
	Task<string> UploadScreenshot(int publicationId, IFormFile screenshot, string? description);
	Task<DeletedFile?> DeleteFile(int publicationFileId);
	Task UploadAwardImage(IFormFile image, IFormFile image2X, IFormFile image4X, string shortName, int? year = null);
	void DeleteAwardImage(string fileName);
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
		string suffix = year.HasValue ? year.Value.ToString() : "xxxx";
		await UploadAwardImageBase(image, $"{shortName}_{suffix}.png");
		await UploadAwardImageBase(image2X, $"{shortName}_{suffix}-2x.png");
		await UploadAwardImageBase(image4X, $"{shortName}_{suffix}-4x.png");
	}

	public void DeleteAwardImage(string shortName)
	{
		DeleteAwardImageBase($"{shortName}.png");
		DeleteAwardImageBase($"{shortName}-2x.png");
		DeleteAwardImageBase($"{shortName}-4x.png");
	}

	private async Task UploadAwardImageBase(IFormFile image, string fileName)
	{
		await using var memoryStream = new MemoryStream();
		await image.CopyToAsync(memoryStream);
		var screenshotBytes = memoryStream.ToArray();
		string screenshotPath = ToFullAwardPath(fileName);

		await File.WriteAllBytesAsync(screenshotPath, screenshotBytes);
	}

	private void DeleteAwardImageBase(string fileName)
	{
		string path = ToFullAwardPath(fileName);
		if (File.Exists(path))
		{
			File.Delete(path);
		}
	}

	public bool AwardExists(string shortName, int year)
	{
		var path = ToFullAwardPath($"{shortName}_{year}.png");
		return File.Exists(path);
	}

	private string ToFullAwardPath(string fileName) => Path.Combine(env.WebRootPath, AwardLocation, fileName);
}

public record DeletedFile(int Id, FileType Type, string Path);
