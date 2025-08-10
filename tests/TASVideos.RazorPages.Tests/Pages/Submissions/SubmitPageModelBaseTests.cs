using Microsoft.AspNetCore.Http;
using TASVideos.Core.Services;
using TASVideos.Data.Entity;
using TASVideos.MovieParsers;
using TASVideos.MovieParsers.Result;
using TASVideos.Pages.Submissions;
using TASVideos.Tests.Base;
using static TASVideos.RazorPages.Tests.RazorTestHelpers;

namespace TASVideos.RazorPages.Tests.Pages.Submissions;

[TestClass]
public class SubmitPageModelBaseTests : TestDbBase
{
	private readonly IMovieParser _movieParser;
	private readonly IFileService _fileService;
	private readonly TestSubmitPageModel _page;

	public SubmitPageModelBaseTests()
	{
		_movieParser = Substitute.For<IMovieParser>();
		_fileService = Substitute.For<IFileService>();
		_page = new TestSubmitPageModel(_movieParser, _fileService);
	}

	[TestMethod]
	public async Task ParseMovieFile_NonZipFile_ParsesFileAndZipsResult()
	{
		var formFile = Substitute.For<IFormFile>();
		formFile.FileName.Returns("test.bk2");
		formFile.ContentType.Returns("application/octet-stream");

		var fileStream = new MemoryStream([1, 2, 3, 4]);
		formFile.OpenReadStream().Returns(fileStream);
		formFile.CopyToAsync(Arg.Any<Stream>()).Returns(Task.CompletedTask)
			.AndDoes(x => fileStream.CopyTo((Stream)x.Args()[0]));

		var parseResult = Substitute.For<IParseResult>();
		parseResult.Success.Returns(true);
		parseResult.FileExtension.Returns(".bk2");

		_movieParser.ParseFile("test.bk2", Arg.Any<Stream>()).Returns(parseResult);

		var zippedBytes = new byte[] { 5, 6, 7, 8, 9 };
		_fileService.ZipFile(Arg.Any<byte[]>(), "test.bk2").Returns(zippedBytes);

		var (result, movieBytes) = await _page.ParseMovieFile(formFile);

		Assert.AreEqual(parseResult, result);
		Assert.AreEqual(zippedBytes, movieBytes);
		await _movieParser.Received(1).ParseFile("test.bk2", Arg.Any<Stream>());
		await _fileService.Received(1).ZipFile(Arg.Any<byte[]>(), "test.bk2");
	}

	[TestMethod]
	public async Task ParseMovieFile_ZipFile_ParsesZipAndReturnsRawBytes()
	{
		var formFile = Substitute.For<IFormFile>();
		formFile.FileName.Returns("test.zip");
		formFile.ContentType.Returns("application/zip");

		var fileStream = new MemoryStream([1, 2, 3, 4]);
		formFile.OpenReadStream().Returns(fileStream);
		formFile.CopyToAsync(Arg.Any<Stream>()).Returns(Task.CompletedTask)
			.AndDoes(x => fileStream.CopyTo((Stream)x.Args()[0]));

		var parseResult = Substitute.For<IParseResult>();
		parseResult.Success.Returns(true);

		_movieParser.ParseZip(Arg.Any<Stream>()).Returns(parseResult);

		var (result, movieBytes) = await _page.ParseMovieFile(formFile);

		Assert.AreEqual(parseResult, result);
		Assert.AreEqual(4, movieBytes.Length); // Raw file bytes
		await _movieParser.Received(1).ParseZip(Arg.Any<Stream>());
		await _fileService.DidNotReceive().ZipFile(Arg.Any<byte[]>(), Arg.Any<string>());
	}

	[TestMethod]
	public void CanEditSubmission_UserHasEditPermission_ReturnsTrue()
	{
		var user = _db.AddUser("TestUser").Entity;
		AddAuthenticatedUser(_page, user, [PermissionTo.EditSubmissions]);

		var result = _page.CanEditSubmission("OtherUser", ["AnotherUser"]);

		Assert.IsTrue(result);
	}

	[TestMethod]
	public void CanEditSubmission_UserIsOriginalSubmitter_ReturnsTrue()
	{
		var user = _db.AddUser("TestUser").Entity;
		AddAuthenticatedUser(_page, user, [PermissionTo.SubmitMovies]);

		var result = _page.CanEditSubmission("TestUser", ["OtherUser"]);

		Assert.IsTrue(result);
	}

	[TestMethod]
	public void CanEditSubmission_UserIsAuthor_ReturnsTrue()
	{
		var user = _db.AddUser("TestUser").Entity;
		AddAuthenticatedUser(_page, user, [PermissionTo.SubmitMovies]);

		var result = _page.CanEditSubmission("OtherUser", ["TestUser", "AnotherUser"]);

		Assert.IsTrue(result);
	}

	[TestMethod]
	public void CanEditSubmission_UserNotAuthorOrSubmitter_ReturnsFalse()
	{
		var user = _db.AddUser("TestUser").Entity;
		AddAuthenticatedUser(_page, user, [PermissionTo.SubmitMovies]);

		var result = _page.CanEditSubmission("OtherUser", ["AnotherUser", "ThirdUser"]);

		Assert.IsFalse(result);
	}

	[TestMethod]
	public void CanEditSubmission_UserIsSubmitterButHasNoSubmitPermission_ReturnsFalse()
	{
		var user = _db.AddUser("TestUser").Entity;
		AddAuthenticatedUser(_page, user, []); // No permissions

		var result = _page.CanEditSubmission("TestUser", ["TestUser"]);

		Assert.IsFalse(result);
	}

	[TestMethod]
	public void CanEditSubmission_AnonymousUser_ReturnsFalse()
	{
		var result = _page.CanEditSubmission("TestUser", ["TestUser"]);

		Assert.IsFalse(result);
	}

	[TestMethod]
	public void CanEditSubmission_WithEditAndSubmitPermissions_ReturnsTrue()
	{
		var user = _db.AddUser("TestUser").Entity;
		AddAuthenticatedUser(_page, user, [PermissionTo.EditSubmissions, PermissionTo.SubmitMovies]);

		var result = _page.CanEditSubmission("OtherUser", ["AnotherUser"]);

		Assert.IsTrue(result);
	}

	// Test helper class to access protected methods
	private class TestSubmitPageModel(IMovieParser parser, IFileService fileService)
		: SubmitPageModelBase(parser, fileService)
	{
		// Expose protected methods for testing
		public new async Task<(IParseResult ParseResult, byte[] MovieFileBytes)> ParseMovieFile(IFormFile movieFile)
			=> await base.ParseMovieFile(movieFile);

		public new bool CanEditSubmission(string? submitter, ICollection<string> authors)
			=> base.CanEditSubmission(submitter, authors);
	}
}
