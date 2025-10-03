using Microsoft.AspNetCore.Http;
using TASVideos.Core.Services;
using TASVideos.Core.Services.ExternalMediaPublisher;
using TASVideos.MovieParsers.Result;
using TASVideos.Pages.Publications;
using TASVideos.Services;
using TASVideos.Tests.Base;

namespace TASVideos.RazorPages.Tests.Pages.Publications;

[TestClass]
public class AdditionalMoviesModelTests : TestDbBase
{
	private readonly IExternalMediaPublisher _publisher;
	private readonly IPublicationMaintenanceLogger _maintenanceLogger;
	private readonly IQueueService _queueService;
	private readonly IPublications _publications;
	private readonly AdditionalMoviesModel _page;

	public AdditionalMoviesModelTests()
	{
		_publications = Substitute.For<IPublications>();
		_publisher = Substitute.For<IExternalMediaPublisher>();
		_maintenanceLogger = Substitute.For<IPublicationMaintenanceLogger>();
		_queueService = Substitute.For<IQueueService>();
		_page = new AdditionalMoviesModel(_db, _publications, _publisher, _maintenanceLogger, _queueService);
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
		const string description = "Description";
		const string path = "Path";
		var file = _db.AddMovieFile(pub, path).Entity;
		file.Description = description;
		await _db.SaveChangesAsync();
		_publications.GetTitle(pub.Id).Returns(pub.Title);

		_page.Id = pub.Id;

		var result = await _page.OnGet();

		Assert.IsInstanceOfType<PageResult>(result);
		Assert.AreEqual(pub.Title, _page.PublicationTitle);
		Assert.AreEqual(1, _page.AvailableMovieFiles.Count);
		Assert.AreEqual(description, _page.AvailableMovieFiles[0].Description);
		Assert.AreEqual(path, _page.AvailableMovieFiles[0].FileName);
	}

	[TestMethod]
	public async Task OnPost_PublicationNotFound_ReturnsNotFound()
	{
		_page.Id = 999;
		_publications.GetTitle(Arg.Any<int>()).Returns((string?)null);
		var result = await _page.OnPost();
		Assert.IsInstanceOfType<NotFoundResult>(result);
	}

	[TestMethod]
	public async Task OnPost_ZipFile_AddsModelError()
	{
		var pub = _db.AddPublication().Entity;
		await _db.SaveChangesAsync();
		_publications.GetTitle(pub.Id).Returns(pub.Title);

		_page.Id = pub.Id;
		_page.AdditionalMovieFile = CreateMockFormFile("test.zip", "application/zip");
		_page.DisplayName = "Test Movie";

		var result = await _page.OnPost();

		Assert.IsInstanceOfType<PageResult>(result);
		Assert.IsFalse(_page.ModelState.IsValid, "ModelState should be invalid due to zip file rejection");
		Assert.IsTrue(_page.ModelState.ContainsKey(nameof(_page.AdditionalMovieFile)));
		Assert.IsTrue(_page.ModelState[nameof(_page.AdditionalMovieFile)]!.Errors[0].ErrorMessage.Contains("Zip files are not allowed"));
	}

	[TestMethod]
	public async Task OnPost_ValidMovieFileParseError_AddsModelError()
	{
		var pub = _db.AddPublication().Entity;
		await _db.SaveChangesAsync();
		_publications.GetTitle(pub.Id).Returns(pub.Title);

		_page.Id = pub.Id;
		_page.AdditionalMovieFile = CreateMockFormFile("test.bk2", "application/octet-stream");
		_page.DisplayName = "Test Movie";

		var parseResult = Substitute.For<IParseResult>();
		parseResult.Success.Returns(false);
		parseResult.Errors.Returns(["Parse error occurred"]);
		_queueService.ParseMovieFile(Arg.Any<IFormFile>()).Returns((parseResult, new byte[0]));

		var result = await _page.OnPost();

		Assert.IsInstanceOfType<PageResult>(result);
		Assert.IsFalse(_page.ModelState.IsValid);
	}

	[TestMethod]
	public async Task OnPost_ValidMovieFileParseSuccess_AddsFileAndRedirects()
	{
		var pub = _db.AddPublication().Entity;
		var user = _db.AddUser("TestUser").Entity;
		await _db.SaveChangesAsync();
		_publications.GetTitle(pub.Id).Returns(pub.Title);
		AddAuthenticatedUser(_page, user, [PermissionTo.CreateAdditionalMovieFiles]);

		var parseResult = Substitute.For<IParseResult>();
		parseResult.Success.Returns(true);
		_queueService.ParseMovieFile(Arg.Any<IFormFile>()).Returns((parseResult, new byte[] { 1, 2, 3, 4 }));

		_page.Id = pub.Id;
		_page.AdditionalMovieFile = CreateMockFormFile("test.bk2", "application/octet-stream");
		_page.DisplayName = "Test BK2 Movie";

		var result = await _page.OnPost();

		Assert.IsInstanceOfType<RedirectToPageResult>(result);
		var redirect = (RedirectToPageResult)result;
		Assert.AreEqual("AdditionalMovies", redirect.PageName);

		var addedFile = await _db.PublicationFiles
			.SingleOrDefaultAsync(pf => pf.PublicationId == pub.Id);

		Assert.IsNotNull(addedFile);
		Assert.AreEqual("test.bk2", addedFile.Path);
		Assert.AreEqual("Test BK2 Movie", addedFile.Description);
		Assert.AreEqual(FileType.MovieFile, addedFile.Type);

		await _maintenanceLogger.Received(1).Log(pub.Id, user.Id, Arg.Any<string>());
		await _publisher.Received(1).Send(Arg.Any<Post>());
	}

	[TestMethod]
	public async Task OnPostDelete_FileNotFound_RedirectsWithoutError()
	{
		_page.Id = 1;

		var result = await _page.OnPostDelete(999);

		Assert.IsInstanceOfType<RedirectToPageResult>(result);
		var redirect = (RedirectToPageResult)result;
		Assert.AreEqual("AdditionalMovies", redirect.PageName);
	}

	[TestMethod]
	public async Task OnPostDelete_ValidFile_DeletesFileAndRedirects()
	{
		var pub = _db.AddPublication().Entity;
		var user = _db.AddUser("TestUser").Entity;
		var file = _db.AddMovieFile(pub).Entity;
		await _db.SaveChangesAsync();
		AddAuthenticatedUser(_page, user, [PermissionTo.CreateAdditionalMovieFiles]);
		_page.Id = pub.Id;

		var result = await _page.OnPostDelete(file.Id);

		Assert.IsInstanceOfType<RedirectToPageResult>(result);
		var redirect = (RedirectToPageResult)result;
		Assert.AreEqual("AdditionalMovies", redirect.PageName);

		var deletedFile = await _db.PublicationFiles.FindAsync(file.Id);
		Assert.IsNull(deletedFile);

		await _maintenanceLogger.Received(1).Log(pub.Id, user.Id, Arg.Any<string>());
		await _publisher.Received(1).Send(Arg.Any<Post>());
	}
}
