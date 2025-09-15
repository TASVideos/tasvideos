using TASVideos.Core.Services;
using TASVideos.Core.Services.ExternalMediaPublisher;
using TASVideos.MovieParsers.Result;
using TASVideos.Pages.UserFiles;
using TASVideos.Services;
using TASVideos.Tests.Base;

namespace TASVideos.RazorPages.Tests.Pages.UserFiles;

[TestClass]
public class UploadModelTests : TestDbBase
{
	private readonly IUserFiles _userFiles = Substitute.For<IUserFiles>();
	private readonly IExternalMediaPublisher _publisher = Substitute.For<IExternalMediaPublisher>();

	[TestMethod]
	public async Task OnGet_InitializesPropertiesCorrectly()
	{
		_db.AddGameSystem("NES");
		_db.AddGame("Super Mario Bros.");
		await _db.SaveChangesAsync();

		_userFiles.SupportedFileExtensions().Returns([".bk2", ".fm2", ".lsmv"]);
		_userFiles.StorageUsed(Arg.Any<int>()).Returns(1024);
		var page = new UploadModel(_userFiles, _db, _publisher);

		await page.OnGet();

		Assert.AreEqual(3, page.SupportedFileExtensions.Count);
		Assert.IsTrue(page.SupportedFileExtensions.Contains("bk2"));
		Assert.IsTrue(page.SupportedFileExtensions.Contains("fm2"));
		Assert.IsTrue(page.SupportedFileExtensions.Contains("lsmv"));
		Assert.AreEqual(1024, page.StorageUsed);
		Assert.IsTrue(page.AvailableSystems.Count > 0);
		Assert.IsTrue(page.AvailableGames.Count > 0);
	}

	[TestMethod]
	public async Task OnPost_InvalidModel_ReturnsPage()
	{
		var page = new UploadModel(_userFiles, _db, _publisher);
		page.ModelState.AddModelError("Title", "Required field");
		_userFiles.SupportedFileExtensions().Returns([".bk2"]);

		var result = await page.OnPost();

		Assert.IsInstanceOfType<PageResult>(result);
		await _userFiles.Received(1).SupportedFileExtensions();
	}

	[TestMethod]
	public async Task OnPost_CompressedFile_ReturnsPageWithError()
	{
		var user = _db.AddUser("TestUser").Entity;
		var compressedFile = CreateMockFormFile("test.zip", "application/zip");
		var page = new UploadModel(_userFiles, _db, _publisher)
		{
			UserFile = compressedFile,
			Title = "Test File"
		};
		AddAuthenticatedUser(page, user, [PermissionTo.UploadUserFiles]);

		_userFiles.SupportedFileExtensions().Returns([".zip"]);

		var result = await page.OnPost();

		Assert.IsInstanceOfType<PageResult>(result);
		Assert.IsTrue(page.ModelState.ContainsKey("UserFile"));
		var error = page.ModelState["UserFile"]!.Errors.FirstOrDefault();
		Assert.IsNotNull(error);
		Assert.AreEqual("Compressed files are not supported.", error.ErrorMessage);
	}

	[TestMethod]
	public async Task OnPost_UnsupportedFileType_ReturnsPageWithError()
	{
		var user = _db.AddUser("TestUser").Entity;
		var file = CreateMockFormFile("test.txt", "text/plain");
		var page = new UploadModel(_userFiles, _db, _publisher)
		{
			UserFile = file,
			Title = "Test File"
		};
		AddAuthenticatedUser(page, user, [PermissionTo.UploadUserFiles]);

		_userFiles.SupportedFileExtensions().Returns([".bk2", ".fm2"]);

		var result = await page.OnPost();

		Assert.IsInstanceOfType<PageResult>(result);
		Assert.IsTrue(page.ModelState.ContainsKey("UserFile"));
		var error = page.ModelState["UserFile"]!.Errors.FirstOrDefault();
		Assert.IsNotNull(error);
		Assert.AreEqual("Unsupported file type: .txt", error.ErrorMessage);
	}

	[TestMethod]
	public async Task OnPost_InsufficientSpace_ReturnsPageWithError()
	{
		var user = _db.AddUser("TestUser").Entity;
		var file = CreateMockFormFile("test.bk2", "application/octet-stream");
		var page = new UploadModel(_userFiles, _db, _publisher)
		{
			UserFile = file,
			Title = "Test File"
		};
		AddAuthenticatedUser(page, user, [PermissionTo.UploadUserFiles]);

		_userFiles.SupportedFileExtensions().Returns([".bk2"]);
		_userFiles.SpaceAvailable(user.Id, Arg.Any<long>()).Returns(false);

		var result = await page.OnPost();

		Assert.IsInstanceOfType<PageResult>(result);
		Assert.IsTrue(page.ModelState.ContainsKey("UserFile"));
		var error = page.ModelState["UserFile"]!.Errors.FirstOrDefault();
		Assert.IsNotNull(error);
		Assert.AreEqual("File exceeds your available storage space. Remove unnecessary files and try again.", error.ErrorMessage);
	}

	[TestMethod]
	public async Task OnPost_UploadFails_ReturnsPageWithError()
	{
		var user = _db.AddUser("TestUser").Entity;
		var file = CreateMockFormFile("test.bk2", "application/octet-stream");
		var page = new UploadModel(_userFiles, _db, _publisher)
		{
			UserFile = file,
			Title = "Test File"
		};
		AddAuthenticatedUser(page, user, [PermissionTo.UploadUserFiles]);

		_userFiles.SupportedFileExtensions().Returns([".bk2"]);
		_userFiles.SpaceAvailable(user.Id, Arg.Any<long>()).Returns(true);

		var parseResult = Substitute.For<IParseResult>();
		parseResult.Success.Returns(false);
		parseResult.Errors.Returns(["Invalid movie file format"]);
		_userFiles.Upload(user.Id, Arg.Any<UserFileUpload>()).Returns(Task.FromResult(((long?)null, (IParseResult?)parseResult)));

		var result = await page.OnPost();

		Assert.IsInstanceOfType<PageResult>(result);
		Assert.IsFalse(page.ModelState.IsValid);
	}

	[TestMethod]
	public async Task OnPost_HiddenUpload_SendsCorrectPublishInfo()
	{
		var user = _db.AddUser("TestUser").Entity;
		var file = CreateMockFormFile("test.bk2", "application/octet-stream");
		var page = new UploadModel(_userFiles, _db, _publisher)
		{
			UserFile = file,
			Title = "Hidden File",
			Hidden = true
		};
		AddAuthenticatedUser(page, user, [PermissionTo.UploadUserFiles]);

		_userFiles.SupportedFileExtensions().Returns([".bk2"]);
		_userFiles.SpaceAvailable(user.Id, Arg.Any<long>()).Returns(true);

		var parseResult = Substitute.For<IParseResult>();
		parseResult.Success.Returns(true);
		_userFiles.Upload(user.Id, Arg.Any<UserFileUpload>()).Returns(Task.FromResult(((long?)456, (IParseResult?)parseResult)));

		var result = await page.OnPost();

		Assert.IsInstanceOfType<RedirectToPageResult>(result);
		await _publisher.Received(1).Send(Arg.Is<Post>(p => p.Type == PostType.Administrative));
	}

	[TestMethod]
	public async Task OnPost_SuccessfulUpload()
	{
		var user = _db.AddUser("TestUser").Entity;
		var file = CreateMockFormFile("test.bk2", "application/octet-stream");
		var page = new UploadModel(_userFiles, _db, _publisher)
		{
			UserFile = file,
			Title = "Test Title",
			Description = "Test Description",
			System = 1,
			Game = 2,
			Hidden = false
		};
		AddAuthenticatedUser(page, user, [PermissionTo.UploadUserFiles]);

		_userFiles.SupportedFileExtensions().Returns([".bk2"]);
		_userFiles.SpaceAvailable(user.Id, Arg.Any<long>()).Returns(true);

		var parseResult = Substitute.For<IParseResult>();
		parseResult.Success.Returns(true);
		_userFiles.Upload(user.Id, Arg.Any<UserFileUpload>()).Returns(Task.FromResult(((long?)123, (IParseResult?)parseResult)));

		var result = await page.OnPost();

		Assert.IsInstanceOfType<RedirectToPageResult>(result);
		var redirect = (RedirectToPageResult)result;
		Assert.AreEqual("/Profile/UserFiles", redirect.PageName);
		await _publisher.Received(1).Send(Arg.Is<Post>(p => p.Type == PostType.General));
		await _userFiles.Received(1).Upload(user.Id, Arg.Is<UserFileUpload>(req =>
			req.Title == "Test Title" &&
			req.Description == "Test Description" &&
			req.SystemId == 1 &&
			req.GameId == 2 &&
			req.FileName == "test.bk2" &&
			req.Hidden == false));
	}

	[TestMethod]
	public void RequiresPermission() => AssertHasPermission(typeof(UploadModel), PermissionTo.UploadUserFiles);
}
