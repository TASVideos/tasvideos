using Microsoft.AspNetCore.Http;
using TASVideos.Core.Services;
using TASVideos.Core.Services.ExternalMediaPublisher;
using TASVideos.Pages.Publications;
using TASVideos.Services;
using TASVideos.Tests.Base;

namespace TASVideos.RazorPages.Tests.Pages.Publications;

[TestClass]
public class EditFilesModelTests : TestDbBase
{
	private readonly IPublications _publications;
	private readonly IExternalMediaPublisher _publisher;
	private readonly IMediaFileUploader _uploader;
	private readonly IPublicationMaintenanceLogger _publicationMaintenanceLogger;
	private readonly EditFilesModel _page;

	public EditFilesModelTests()
	{
		_publications = Substitute.For<IPublications>();
		_publisher = Substitute.For<IExternalMediaPublisher>();
		_uploader = Substitute.For<IMediaFileUploader>();
		_publicationMaintenanceLogger = Substitute.For<IPublicationMaintenanceLogger>();
		_page = new EditFilesModel(_db, _publications, _publisher, _uploader, _publicationMaintenanceLogger);
	}

	[TestMethod]
	public async Task OnGet_PublicationNotFound_ReturnsNotFound()
	{
		_page.Id = 999;
		_publications.GetTitle(Arg.Any<int>()).Returns((string?)null);
		var result = await _page.OnGet();
		Assert.IsInstanceOfType<NotFoundResult>(result);
	}

	[TestMethod]
	public async Task OnGet_ValidPublication_PopulatesData()
	{
		var pub = _db.AddPublication().Entity;
		_publications.GetTitle(pub.Id).Returns("Test Publication");
		_db.AddScreenshot(pub, "screenshot1.png");
		_db.AddScreenshot(pub, "screenshot2.png");
		await _db.SaveChangesAsync();

		_page.Id = pub.Id;

		var result = await _page.OnGet();

		Assert.IsInstanceOfType<PageResult>(result);
		Assert.AreEqual("Test Publication", _page.Title);
		Assert.AreEqual(2, _page.Files.Count);
		Assert.IsTrue(_page.Files.Any(f => f.Path == "screenshot1.png"));
		Assert.IsTrue(_page.Files.Any(f => f.Path == "screenshot2.png"));
	}

	[TestMethod]
	public async Task OnGet_PublicationWithNoFiles_PopulatesEmptyFilesList()
	{
		var pub = _db.AddPublication().Entity;
		_publications.GetTitle(pub.Id).Returns("Test Publication");
		await _db.SaveChangesAsync();

		_page.Id = pub.Id;

		var result = await _page.OnGet();

		Assert.IsInstanceOfType<PageResult>(result);
		Assert.AreEqual("Test Publication", _page.Title);
		Assert.AreEqual(0, _page.Files.Count);
	}

	[TestMethod]
	public async Task OnPost_InvalidModelState_ReturnsPageWithFiles()
	{
		var pub = _db.AddPublication().Entity;
		pub.Title = "Test Publication";
		_db.AddScreenshot(pub);
		await _db.SaveChangesAsync();

		var user = _db.AddUser("TestUser").Entity;
		AddAuthenticatedUser(_page, user, [PermissionTo.EditPublicationFiles]);

		_page.Id = pub.Id;
		_page.NewScreenshot = CreateMockFormFile("test.png", "image/png");
		_page.ModelState.AddModelError("NewScreenshot", "Test error");

		var result = await _page.OnPost();

		Assert.IsInstanceOfType<PageResult>(result);
		Assert.AreEqual(1, _page.Files.Count);
		Assert.AreEqual("screenshot.png", _page.Files[0].Path);
		await _uploader.DidNotReceive().UploadScreenshot(Arg.Any<int>(), Arg.Any<IFormFile>(), Arg.Any<string>());
	}

	[TestMethod]
	public async Task OnPost_ValidFile_UploadsFileAndRedirects()
	{
		var pub = _db.AddPublication().Entity;
		pub.Title = "Test Publication";
		await _db.SaveChangesAsync();

		var user = _db.AddUser("TestUser").Entity;
		AddAuthenticatedUser(_page, user, [PermissionTo.EditPublicationFiles]);

		_page.Id = pub.Id;
		_page.Title = pub.Title;
		_page.NewScreenshot = CreateMockFormFile("screenshot.png", "image/png");
		_page.Description = "Test screenshot";

		var expectedPath = $"{pub.Id}M.png";
		var expectedData = new byte[] { 1, 2, 3, 4 };
		_uploader.UploadScreenshot(pub.Id, _page.NewScreenshot, "Test screenshot")
			.Returns((expectedPath, expectedData));

		var result = await _page.OnPost();

		Assert.IsInstanceOfType<RedirectToPageResult>(result);
		var redirect = (RedirectToPageResult)result;
		Assert.AreEqual("EditFiles", redirect.PageName);
		Assert.AreEqual(pub.Id, redirect.RouteValues!["Id"]);
		await _uploader.Received(1).UploadScreenshot(pub.Id, _page.NewScreenshot, "Test screenshot");
		await _publicationMaintenanceLogger.Received(1).Log(pub.Id, user.Id, Arg.Any<string>());
		await _publisher.Received(1).Send(Arg.Any<Post>());
	}

	[TestMethod]
	public async Task OnPost_FileUploadWithoutDescription_UploadsWithNullDescription()
	{
		var pub = _db.AddPublication().Entity;
		pub.Title = "Test Publication";
		await _db.SaveChangesAsync();

		var user = _db.AddUser("TestUser").Entity;
		AddAuthenticatedUser(_page, user, [PermissionTo.EditPublicationFiles]);

		_page.Id = pub.Id;
		_page.Title = pub.Title;
		_page.NewScreenshot = CreateMockFormFile("screenshot.png", "image/png");
		_page.Description = null;

		var expectedPath = $"{pub.Id}M.png";
		var expectedData = new byte[] { 1, 2, 3, 4 };
		_uploader.UploadScreenshot(pub.Id, _page.NewScreenshot, null)
			.Returns((expectedPath, expectedData));

		var result = await _page.OnPost();

		Assert.IsInstanceOfType<RedirectToPageResult>(result);
		await _uploader.Received(1).UploadScreenshot(pub.Id, _page.NewScreenshot, null);
		await _publicationMaintenanceLogger.Received(1).Log(pub.Id, user.Id, Arg.Any<string>());
		await _publisher.Received(1).Send(Arg.Any<Post>());
	}

	[TestMethod]
	public async Task OnPostDelete_FileNotFound_ReturnsRedirectWithoutLogging()
	{
		var pub = _db.AddPublication().Entity;
		await _db.SaveChangesAsync();

		var user = _db.AddUser("TestUser").Entity;
		AddAuthenticatedUser(_page, user, [PermissionTo.EditPublicationFiles]);

		_page.Id = pub.Id;
		const int nonExistentFileId = 999;

		_uploader.DeleteFile(nonExistentFileId).Returns((DeletedFile?)null);

		var result = await _page.OnPostDelete(nonExistentFileId);

		Assert.IsInstanceOfType<RedirectToPageResult>(result);
		var redirect = (RedirectToPageResult)result;
		Assert.AreEqual("EditFiles", redirect.PageName);
		Assert.AreEqual(pub.Id, redirect.RouteValues!["Id"]);
		await _uploader.Received(1).DeleteFile(nonExistentFileId);
		await _publicationMaintenanceLogger.DidNotReceive().Log(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string>());
		await _publisher.DidNotReceive().Send(Arg.Any<Post>());
	}

	[TestMethod]
	public async Task OnPostDelete_ValidFile_DeletesFileAndLogs()
	{
		var pub = _db.AddPublication().Entity;
		pub.Title = "Test Publication";
		await _db.SaveChangesAsync();

		var user = _db.AddUser("TestUser").Entity;
		AddAuthenticatedUser(_page, user, [PermissionTo.EditPublicationFiles]);

		_page.Id = pub.Id;
		_page.Title = pub.Title;
		const int fileId = 123;
		var deletedFile = new DeletedFile(fileId, FileType.Screenshot, "screenshot.png");

		_uploader.DeleteFile(fileId).Returns(deletedFile);

		var result = await _page.OnPostDelete(fileId);

		Assert.IsInstanceOfType<RedirectToPageResult>(result);
		var redirect = (RedirectToPageResult)result;
		Assert.AreEqual("EditFiles", redirect.PageName);
		Assert.AreEqual(pub.Id, redirect.RouteValues!["Id"]);
		await _uploader.Received(1).DeleteFile(fileId);
		await _publicationMaintenanceLogger.Received(1).Log(pub.Id, user.Id, Arg.Any<string>());
		await _publisher.Received(1).Send(Arg.Any<Post>());
	}
}
