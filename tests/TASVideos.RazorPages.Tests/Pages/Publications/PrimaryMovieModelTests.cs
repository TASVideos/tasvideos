using TASVideos.Core.Services;
using TASVideos.Core.Services.ExternalMediaPublisher;
using TASVideos.Pages.Publications;
using TASVideos.Services;
using TASVideos.Tests.Base;

namespace TASVideos.RazorPages.Tests.Pages.Publications;

[TestClass]
public class PrimaryMovieModelTests : TestDbBase
{
	private readonly IExternalMediaPublisher _publisher;
	private readonly IPublicationMaintenanceLogger _maintenanceLogger;
	private readonly PrimaryMoviesModel _page;

	public PrimaryMovieModelTests()
	{
		_publisher = Substitute.For<IExternalMediaPublisher>();
		_maintenanceLogger = Substitute.For<IPublicationMaintenanceLogger>();
		_page = new PrimaryMoviesModel(_db, _publisher, _maintenanceLogger);
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
		var pub = _db.AddPublication().Entity;
		pub.Title = "Test Publication";
		pub.MovieFileName = "original.zip";
		await _db.SaveChangesAsync();
		_page.Id = pub.Id;

		var result = await _page.OnGet();

		Assert.IsInstanceOfType<PageResult>(result);
		Assert.AreEqual("Test Publication", _page.PublicationTitle);
		Assert.AreEqual("original.zip", _page.OriginalFileName);
	}

	[TestMethod]
	public async Task OnPost_PublicationNotFound_ReturnsNotFound()
	{
		_page.Id = 999;
		var result = await _page.OnPost();
		Assert.IsInstanceOfType<NotFoundResult>(result);
	}

	[TestMethod]
	public async Task OnPost_FileNameAlreadyExists_AddsModelError()
	{
		var pub1 = _db.AddPublication().Entity;
		pub1.MovieFileName = "existing.zip";

		var pub2 = _db.AddPublication().Entity;
		pub2.MovieFileName = "different.zip";

		await _db.SaveChangesAsync();

		_page.Id = pub2.Id;
		_page.PrimaryMovieFile = CreateMockFormFile("existing.zip", "application/zip");
		_page.Reason = "Test reason";

		var result = await _page.OnPost();

		Assert.IsInstanceOfType<PageResult>(result);
		Assert.IsFalse(_page.ModelState.IsValid);
		Assert.IsTrue(_page.ModelState.ContainsKey(nameof(_page.PrimaryMovieFile)));
		var error = _page.ModelState[nameof(_page.PrimaryMovieFile)]?.Errors.FirstOrDefault();
		Assert.IsNotNull(error);
		Assert.IsTrue(error.ErrorMessage.Contains("existing.zip already exists"));
	}

	[TestMethod]
	public async Task OnPost_ValidFile_UpdatesMovieFileAndRedirects()
	{
		var publication = _db.AddPublication().Entity;
		publication.MovieFileName = "original.zip";
		publication.Title = "Test Publication";
		publication.MovieFile = [1, 2, 3];
		var user = _db.AddUser("TestUser").Entity;
		await _db.SaveChangesAsync();
		AddAuthenticatedUser(_page, user, [PermissionTo.ReplacePrimaryMovieFile]);

		_page.Id = publication.Id;
		_page.PrimaryMovieFile = CreateMockFormFile("new.zip", "application/zip");
		_page.Reason = "Improved movie file";

		var result = await _page.OnPost();

		AssertRedirect(result, "Edit", publication.Id);
		var updatedPublication = await _db.Publications.FindAsync(publication.Id);
		Assert.IsNotNull(updatedPublication);
		Assert.AreEqual("new.zip", updatedPublication.MovieFileName);
		Assert.AreEqual(4, updatedPublication.MovieFile.Length);

		await _maintenanceLogger.Received(1).Log(publication.Id, user.Id, "Primary movie file replaced, Reason: Improved movie file");
		await _publisher.Received(1).Send(Arg.Any<Post>());
	}

	[TestMethod]
	public async Task OnPost_InvalidModelState_ReturnsPageWithData()
	{
		var publication = _db.AddPublication().Entity;
		publication.Title = "Test Publication";
		publication.MovieFileName = "original.zip";
		await _db.SaveChangesAsync();

		_page.Id = publication.Id;
		_page.PrimaryMovieFile = CreateMockFormFile("new.zip", "application/zip");
		_page.Reason = ""; // Missing required Reason field

		// Simulate model validation failure
		_page.ModelState.AddModelError(nameof(_page.Reason), "The Reason field is required.");

		var result = await _page.OnPost();

		Assert.IsInstanceOfType<PageResult>(result);
		Assert.AreEqual("Test Publication", _page.PublicationTitle);
		Assert.AreEqual("original.zip", _page.OriginalFileName);

		await _publisher.DidNotReceive().Send(Arg.Any<Post>());
	}

	[TestMethod]
	public async Task OnPost_FileSizeOverLimit_AddsModelError()
	{
		var publication = _db.AddPublication().Entity;
		publication.Title = "Test Publication";
		publication.MovieFileName = "original.zip";
		await _db.SaveChangesAsync();

		var user = _db.AddUser("TestUser").Entity;
		AddAuthenticatedUser(_page, user, []); // No override permissions

		var oversizedFile = CreateMockFormFile("huge.zip", "application/zip");
		oversizedFile.Length.Returns(SiteGlobalConstants.MaximumMovieSize + 1);

		_page.Id = publication.Id;
		_page.PrimaryMovieFile = oversizedFile;
		_page.Reason = "Test reason";

		var result = await _page.OnPost();

		Assert.IsInstanceOfType<PageResult>(result);
		Assert.IsFalse(_page.ModelState.IsValid);

		await _publisher.DidNotReceive().Send(Arg.Any<Post>());
	}

	[TestMethod]
	public async Task OnPost_FileSizeOverLimitWithOverridePermission_AllowsUpdate()
	{
		var publication = _db.AddPublication().Entity;
		publication.Title = "Test Publication";
		publication.MovieFileName = "original.zip";

		var user = _db.AddUser("TestUser").Entity;
		await _db.SaveChangesAsync();

		AddAuthenticatedUser(_page, user, [PermissionTo.ReplacePrimaryMovieFile, PermissionTo.OverrideSubmissionConstraints]);

		var oversizedFile = CreateMockFormFile("huge.zip", "application/zip");
		oversizedFile.Length.Returns(SiteGlobalConstants.MaximumMovieSize + 1);

		_page.Id = publication.Id;
		_page.PrimaryMovieFile = oversizedFile;
		_page.Reason = "Override size limit";

		var result = await _page.OnPost();

		AssertRedirect(result, "Edit");
		await _publisher.Received(1).Send(Arg.Any<Post>());
	}
}
