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
	private readonly IExternalMediaPublisher _publisher;
	private readonly IMediaFileUploader _uploader;
	private readonly IPublicationMaintenanceLogger _publicationMaintenanceLogger;
	private readonly EditFilesModel _page;

	public EditFilesModelTests()
	{
		_publisher = Substitute.For<IExternalMediaPublisher>();
		_uploader = Substitute.For<IMediaFileUploader>();
		_publicationMaintenanceLogger = Substitute.For<IPublicationMaintenanceLogger>();
		_page = new EditFilesModel(_db, _publisher, _uploader, _publicationMaintenanceLogger);
	}

	[TestMethod]
	public async Task OnGet_PublicationNotFound_ReturnsNotFound()
	{
		_page.Id = 999;
		var result = await _page.OnGet();
		Assert.IsInstanceOfType<NotFoundResult>(result);
	}

	[TestMethod]
	public async Task OnGet_ValidPublication_PopulatesData()
	{
		var publication = _db.AddPublication().Entity;
		publication.Title = "Test Publication";
		_db.PublicationFiles.Add(new PublicationFile
		{
			PublicationId = publication.Id,
			Path = "screenshot1.png",
			Type = FileType.Screenshot,
			Description = "Screenshot 1"
		});
		_db.PublicationFiles.Add(new PublicationFile
		{
			PublicationId = publication.Id,
			Path = "screenshot2.png",
			Type = FileType.Screenshot,
			Description = "Screenshot 2"
		});
		await _db.SaveChangesAsync();

		_page.Id = publication.Id;

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
		var publication = _db.AddPublication().Entity;
		publication.Title = "Test Publication";
		await _db.SaveChangesAsync();

		_page.Id = publication.Id;

		var result = await _page.OnGet();

		Assert.IsInstanceOfType<PageResult>(result);
		Assert.AreEqual("Test Publication", _page.Title);
		Assert.AreEqual(0, _page.Files.Count);
	}

	[TestMethod]
	public async Task OnPost_InvalidModelState_ReturnsPageWithFiles()
	{
		var publication = _db.AddPublication().Entity;
		publication.Title = "Test Publication";

		var file = new PublicationFile
		{
			PublicationId = publication.Id,
			Path = "screenshot.png",
			Type = FileType.Screenshot
		};
		_db.PublicationFiles.Add(file);
		await _db.SaveChangesAsync();

		var user = _db.AddUser("TestUser").Entity;
		AddAuthenticatedUser(_page, user, [PermissionTo.EditPublicationFiles]);

		_page.Id = publication.Id;
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
		var publication = _db.AddPublication().Entity;
		publication.Title = "Test Publication";
		await _db.SaveChangesAsync();

		var user = _db.AddUser("TestUser").Entity;
		AddAuthenticatedUser(_page, user, [PermissionTo.EditPublicationFiles]);

		_page.Id = publication.Id;
		_page.Title = publication.Title;
		_page.NewScreenshot = CreateMockFormFile("screenshot.png", "image/png");
		_page.Description = "Test screenshot";

		var expectedPath = $"{publication.Id}M.png";
		var expectedData = new byte[] { 1, 2, 3, 4 };
		_uploader.UploadScreenshot(publication.Id, _page.NewScreenshot, "Test screenshot")
			.Returns((expectedPath, expectedData));

		var result = await _page.OnPost();

		Assert.IsInstanceOfType<RedirectToPageResult>(result);
		var redirect = (RedirectToPageResult)result;
		Assert.AreEqual("EditFiles", redirect.PageName);
		Assert.AreEqual(publication.Id, redirect.RouteValues!["Id"]);
		await _uploader.Received(1).UploadScreenshot(publication.Id, _page.NewScreenshot, "Test screenshot");
		await _publicationMaintenanceLogger.Received(1).Log(publication.Id, user.Id, Arg.Any<string>());
		await _publisher.Received(1).Send(Arg.Any<Post>());
	}

	[TestMethod]
	public async Task OnPost_FileUploadWithoutDescription_UploadsWithNullDescription()
	{
		var publication = _db.AddPublication().Entity;
		publication.Title = "Test Publication";
		await _db.SaveChangesAsync();

		var user = _db.AddUser("TestUser").Entity;
		AddAuthenticatedUser(_page, user, [PermissionTo.EditPublicationFiles]);

		_page.Id = publication.Id;
		_page.Title = publication.Title;
		_page.NewScreenshot = CreateMockFormFile("screenshot.png", "image/png");
		_page.Description = null;

		var expectedPath = $"{publication.Id}M.png";
		var expectedData = new byte[] { 1, 2, 3, 4 };
		_uploader.UploadScreenshot(publication.Id, _page.NewScreenshot, null)
			.Returns((expectedPath, expectedData));

		var result = await _page.OnPost();

		Assert.IsInstanceOfType<RedirectToPageResult>(result);
		await _uploader.Received(1).UploadScreenshot(publication.Id, _page.NewScreenshot, null);
		await _publicationMaintenanceLogger.Received(1).Log(publication.Id, user.Id, Arg.Any<string>());
		await _publisher.Received(1).Send(Arg.Any<Post>());
	}

	[TestMethod]
	public async Task OnPostDelete_FileNotFound_ReturnsRedirectWithoutLogging()
	{
		var publication = _db.AddPublication().Entity;
		await _db.SaveChangesAsync();

		var user = _db.AddUser("TestUser").Entity;
		AddAuthenticatedUser(_page, user, [PermissionTo.EditPublicationFiles]);

		_page.Id = publication.Id;
		const int nonExistentFileId = 999;

		_uploader.DeleteFile(nonExistentFileId).Returns((DeletedFile?)null);

		var result = await _page.OnPostDelete(nonExistentFileId);

		Assert.IsInstanceOfType<RedirectToPageResult>(result);
		var redirect = (RedirectToPageResult)result;
		Assert.AreEqual("EditFiles", redirect.PageName);
		Assert.AreEqual(publication.Id, redirect.RouteValues!["Id"]);
		await _uploader.Received(1).DeleteFile(nonExistentFileId);
		await _publicationMaintenanceLogger.DidNotReceive().Log(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string>());
		await _publisher.DidNotReceive().Send(Arg.Any<Post>());
	}

	[TestMethod]
	public async Task OnPostDelete_ValidFile_DeletesFileAndLogs()
	{
		var publication = _db.AddPublication().Entity;
		publication.Title = "Test Publication";
		await _db.SaveChangesAsync();

		var user = _db.AddUser("TestUser").Entity;
		AddAuthenticatedUser(_page, user, [PermissionTo.EditPublicationFiles]);

		_page.Id = publication.Id;
		_page.Title = publication.Title;
		const int fileId = 123;
		var deletedFile = new DeletedFile(fileId, FileType.Screenshot, "screenshot.png");

		_uploader.DeleteFile(fileId).Returns(deletedFile);

		var result = await _page.OnPostDelete(fileId);

		Assert.IsInstanceOfType<RedirectToPageResult>(result);
		var redirect = (RedirectToPageResult)result;
		Assert.AreEqual("EditFiles", redirect.PageName);
		Assert.AreEqual(publication.Id, redirect.RouteValues!["Id"]);
		await _uploader.Received(1).DeleteFile(fileId);
		await _publicationMaintenanceLogger.Received(1).Log(publication.Id, user.Id, Arg.Any<string>());
		await _publisher.Received(1).Send(Arg.Any<Post>());
	}
}
