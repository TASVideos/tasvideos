using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace TASVideos.Core.Tests.Services;

[TestClass]
public class MediaFileUploaderTests : TestDbBase
{
	private readonly MediaFileUploader _mediaFileUploader;
	private readonly string _tempWebRootPath;

	public MediaFileUploaderTests()
	{
		_tempWebRootPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
		Directory.CreateDirectory(_tempWebRootPath);
		Directory.CreateDirectory(Path.Combine(_tempWebRootPath, "media"));
		Directory.CreateDirectory(Path.Combine(_tempWebRootPath, "awards"));

		var webHostEnvironment = Substitute.For<IWebHostEnvironment>();
		webHostEnvironment.WebRootPath.Returns(_tempWebRootPath);

		_mediaFileUploader = new MediaFileUploader(_db, webHostEnvironment);
	}

	[TestCleanup]
	public override void Cleanup()
	{
		if (Directory.Exists(_tempWebRootPath))
		{
			Directory.Delete(_tempWebRootPath, true);
		}

		base.Cleanup();
	}

	[TestMethod]
	public async Task UploadScreenshot_ValidFile_CreatesFileOnDisk()
	{
		var pub = _db.AddPublication().Entity;
		const string description = "Test screenshot";
		var screenshot = CreateMockFormFile("screenshot.png", "image/png");

		await _mediaFileUploader.UploadScreenshot(pub.Id, screenshot, description);
		var expectedFilePath = Path.Combine(_tempWebRootPath, "media", $"{pub.Id}M.png");
		Assert.IsTrue(File.Exists(expectedFilePath));
		Assert.AreEqual(1, _db.PublicationFiles.Count(pf => pf.PublicationId == pub.Id));
	}

	[TestMethod]
	public async Task UploadScreenshot_DifferentExtension_PreservesExtension()
	{
		var pub = _db.AddPublication().Entity;
		const string description = "Test screenshot";
		var screenshot = CreateMockFormFile("screenshot.jpg", "image/jpeg");

		await _mediaFileUploader.UploadScreenshot(pub.Id, screenshot, description);

		var expectedFilePath = Path.Combine(_tempWebRootPath, "media", $"{pub.Id}M.jpg");
		Assert.IsTrue(File.Exists(expectedFilePath));
		Assert.AreEqual(1, _db.PublicationFiles.Count(pf => pf.PublicationId == pub.Id));
	}

	[TestMethod]
	public async Task UploadAwardImage_ValidFiles_CreatesAllVariants()
	{
		const string shortName = "test-award";
		const int year = 2023;
		var image1X = CreateMockFormFile("award.png", "image/png");
		var image2X = CreateMockFormFile("award-2x.png", "image/png");
		var image4X = CreateMockFormFile("award-4x.png", "image/png");

		var yearDir = Path.Combine(_tempWebRootPath, "awards", "2023");
		Directory.CreateDirectory(yearDir);

		await _mediaFileUploader.UploadAwardImage(image1X, image2X, image4X, shortName, year);

		var expectedPaths = new[]
		{
			Path.Combine(yearDir, "test-award_2023.png"),
			Path.Combine(yearDir, "test-award_2023-2x.png"),
			Path.Combine(yearDir, "test-award_2023-4x.png")
		};

		foreach (var path in expectedPaths)
		{
			Assert.IsTrue(File.Exists(path), $"Expected file not found: {path}");
		}
	}

	[TestMethod]
	public void DeleteAwardImage_ExistingFiles_DeletesAllVariants()
	{
		const string shortName = "test-award";
		var yearDir = Path.Combine(_tempWebRootPath, "awards", "xxxx");
		Directory.CreateDirectory(yearDir);

		var files = new[]
		{
			Path.Combine(yearDir, "test-award_xxxx.png"),
			Path.Combine(yearDir, "test-award_xxxx-2x.png"),
			Path.Combine(yearDir, "test-award_xxxx-4x.png")
		};

		foreach (var file in files)
		{
			File.WriteAllBytes(file, [1, 2, 3, 4]);
		}

		_mediaFileUploader.DeleteAwardImage(shortName);

		foreach (var file in files)
		{
			Assert.IsFalse(File.Exists(file), $"File should have been deleted: {file}");
		}
	}

	[TestMethod]
	public void DeleteAwardImage_NonExistentFiles_DoesNotThrow()
	{
		const string shortName = "non-existent-award";

		var dir = Path.Combine(_tempWebRootPath, "awards", "xxxx");
		Directory.CreateDirectory(dir);
		var expectedFilePath1X = Path.Combine(dir, shortName + "_xxxx.png");
		var expectedFilePath2X = Path.Combine(dir, shortName + "_xxxx-2x.png");
		var expectedFilePath4X = Path.Combine(dir, shortName + "_xxxx-4x.png");
		File.WriteAllBytes(expectedFilePath1X, [1, 2, 3, 4]);
		File.WriteAllBytes(expectedFilePath2X, [1, 2, 3, 4]);
		File.WriteAllBytes(expectedFilePath4X, [1, 2, 3, 4]);

		_mediaFileUploader.DeleteAwardImage(shortName);

		Assert.IsFalse(File.Exists(expectedFilePath1X));
		Assert.IsFalse(File.Exists(expectedFilePath2X));
		Assert.IsFalse(File.Exists(expectedFilePath4X));
	}

	[TestMethod]
	public void AwardExists_ExistingAward_ReturnsTrue()
	{
		const string shortName = "existing-award";
		const int year = 2023;
		var yearDir = Path.Combine(_tempWebRootPath, "awards", "2023");
		Directory.CreateDirectory(yearDir);

		var awardPath = Path.Combine(yearDir, "existing-award_2023.png");
		File.WriteAllBytes(awardPath, [1, 2, 3, 4]);

		var result = _mediaFileUploader.AwardExists(shortName, year);

		Assert.IsTrue(result);
	}

	[TestMethod]
	public void AwardExists_NonExistentAward_ReturnsFalse()
	{
		const string shortName = "non-existent-award";
		const int year = 2023;

		var result = _mediaFileUploader.AwardExists(shortName, year);

		Assert.IsFalse(result);
	}

	[TestMethod]
	public async Task DeleteFile_NonExistentFile_ReturnsNull()
	{
		var result = await _mediaFileUploader.DeleteFile(999);
		Assert.IsNull(result);
	}

	private static IFormFile CreateMockFormFile(string fileName, string contentType)
	{
		var bytes = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }; // PNG header
		var stream = new MemoryStream(bytes);
		var formFile = Substitute.For<IFormFile>();
		formFile.FileName.Returns(fileName);
		formFile.ContentType.Returns(contentType);
		formFile.Length.Returns(bytes.Length);
		formFile.CopyToAsync(Arg.Any<Stream>(), Arg.Any<CancellationToken>())
			.Returns(Task.FromResult(0))
			.AndDoes(info =>
			{
				var targetStream = info.ArgAt<Stream>(0);
				stream.CopyTo(targetStream);
				stream.Position = 0; // Reset for potential reuse
			});
		return formFile;
	}
}
