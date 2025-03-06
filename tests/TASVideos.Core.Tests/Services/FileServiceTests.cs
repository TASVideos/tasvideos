using System.IO.Compression;
using System.Reflection;
using TASVideos.Data.Entity;

namespace TASVideos.Core.Tests.Services;

[TestClass]
public class FileServiceTests : TestDbBase
{
	private readonly FileService _fileService;

	public FileServiceTests()
	{
		_fileService = new FileService(_db);
	}

	private static Stream Embedded(string name)
	{
		var stream = Assembly.GetAssembly(typeof(FileServiceTests))?.GetManifestResourceStream("TASVideos.Core.Tests.Services.TestFiles." + name);
		return stream ?? throw new InvalidOperationException($"Unable to find embedded resource {name}");
	}

	[TestMethod]
	public async Task CopyZip_BasicTest()
	{
		// Arrange
		const string newName = "1M";
		var stream = Embedded("2Frames.zip");
		await using var ms = new MemoryStream();
		await stream.CopyToAsync(ms);
		var bytes = ms.ToArray();

		// Act
		var result = await _fileService.CopyZip(bytes, newName);

		// Assert
		Assert.IsNotNull(result);
		Assert.IsTrue(result.Any());

		await using var resultStream = new MemoryStream(result);
		using var resultZipArchive = new ZipArchive(resultStream, ZipArchiveMode.Read);

		Assert.AreEqual(1, resultZipArchive.Entries.Count);
		var entry = resultZipArchive.Entries.Single();
		Assert.AreEqual(newName, entry.Name);
	}

	[TestMethod]
	public async Task Compress_ReturnsGzipIfSmaller()
	{
		// Arrange
		var bytes = Enumerable.Repeat(0, 100).Select(i => (byte)i).ToArray();

		// Act
		var result = await _fileService.Compress(bytes);

		// Assert
		Assert.IsNotNull(result);
		Assert.AreEqual(bytes.Length, result.OriginalSize);
		Assert.IsTrue(result.CompressedSize < result.OriginalSize);
		Assert.AreEqual(Compression.Gzip, result.Type);
	}

	[TestMethod]
	public async Task Compress_ReturnsUncompressedIfNotSmaller()
	{
		// Arrange
		var bytes = new byte[] { 31, 139, 8, 0, 0, 0, 0, 0, 0, 10 };

		// Act
		var result = await _fileService.Compress(bytes);

		// Assert
		Assert.IsNotNull(result);
		Assert.AreEqual(bytes.Length, result.OriginalSize);
		Assert.IsTrue(result.CompressedSize >= result.OriginalSize);
		Assert.AreEqual(Compression.None, result.Type);
	}

	[TestMethod]
	public async Task GetSubmissionFile_NotFound_ReturnsNull()
	{
		var actual = await _fileService.GetSubmissionFile(int.MaxValue);
		Assert.IsNull(actual);
	}

	[TestMethod]
	public async Task GetSubmissionFile_Found_ReturnsFile()
	{
		byte[] zippedData = [0xFF, 0xFF];
		var submission = _db.AddSubmission();
		submission.Entity.MovieFile = zippedData;
		await _db.SaveChangesAsync();

		var actual = await _fileService.GetSubmissionFile(submission.Entity.Id);

		Assert.IsNotNull(actual);
		Assert.AreEqual($"submission{submission.Entity.Id}", actual.Path);
		Assert.IsTrue(zippedData.SequenceEqual(actual.Data));
	}

	[TestMethod]
	public async Task GetPublicationFile_NotFound_ReturnsNull()
	{
		var actual = await _fileService.GetPublicationFile(int.MaxValue);
		Assert.IsNull(actual);
	}

	[TestMethod]
	public async Task GetPublicationFile_Found_ReturnsFile()
	{
		const string movieFileName = "movieFileName";
		byte[] zippedData = [0xFF, 0xFF];
		var publication = _db.AddPublication();
		publication.Entity.MovieFile = zippedData;
		publication.Entity.MovieFileName = movieFileName;
		await _db.SaveChangesAsync();

		var actual = await _fileService.GetPublicationFile(publication.Entity.Id);

		Assert.IsNotNull(actual);
		Assert.AreEqual(movieFileName, actual.Path);
		Assert.IsTrue(zippedData.SequenceEqual(actual.Data));
	}

	[TestMethod]
	public async Task GetAdditionalPublicationFile_NotFound_ReturnsNull()
	{
		var actual = await _fileService.GetAdditionalPublicationFile(int.MaxValue, int.MaxValue);
		Assert.IsNull(actual);
	}

	[TestMethod]
	public async Task GetAdditionalPublicationFile_AdditionalFileNotFound_ReturnsNull()
	{
		var publication = _db.AddPublication();

		var actual = await _fileService.GetAdditionalPublicationFile(publication.Entity.Id, int.MaxValue);
		Assert.IsNull(actual);
	}

	[TestMethod]
	public async Task GetAdditionalPublicationFile_Found_ReturnsFile()
	{
		const string movieFileName = "movieFileName";
		byte[] zippedData = [0xFF, 0xFF];
		var publication = _db.AddPublication();
		var pubFile = _db.PublicationFiles.Add(new PublicationFile { PublicationId = publication.Entity.Id, FileData = zippedData, Path = movieFileName });
		await _db.SaveChangesAsync();

		var actual = await _fileService.GetAdditionalPublicationFile(publication.Entity.Id, pubFile.Entity.Id);
		Assert.IsNotNull(actual);
		Assert.AreEqual(movieFileName, actual.Path);
		Assert.IsTrue(zippedData.SequenceEqual(actual.Data));
	}
}
