using Microsoft.EntityFrameworkCore;
using TASVideos.Core.Services;
using TASVideos.Core.Services.ExternalMediaPublisher;
using TASVideos.Data.Entity;
using TASVideos.MovieParsers;
using TASVideos.MovieParsers.Result;
using TASVideos.Pages.Publications;
using TASVideos.Services;
using TASVideos.Tests.Base;
using static TASVideos.RazorPages.Tests.RazorTestHelpers;

namespace TASVideos.RazorPages.Tests.Pages.Publications;

[TestClass]
public class AdditionalMoviesModelTests : TestDbBase
{
	private readonly IExternalMediaPublisher _publisher;
	private readonly IPublicationMaintenanceLogger _maintenanceLogger;
	private readonly IMovieParser _parser;
	private readonly AdditionalMoviesModel _page;

	public AdditionalMoviesModelTests()
	{
		_publisher = Substitute.For<IExternalMediaPublisher>();
		_maintenanceLogger = Substitute.For<IPublicationMaintenanceLogger>();
		_parser = Substitute.For<IMovieParser>();
		_page = new AdditionalMoviesModel(_db, _publisher, _maintenanceLogger, _parser);
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
		const string description = "Description";
		const string path = "Path";
		_db.PublicationFiles.Add(new PublicationFile
		{
			PublicationId = pub.Id,
			Description = description,
			Path = path,
			FileData = [0, 1, 2]
		});
		await _db.SaveChangesAsync();

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

		var result = await _page.OnPost();

		Assert.IsInstanceOfType<NotFoundResult>(result);
	}

	[TestMethod]
	public async Task OnPost_NonZipFile_AddsModelError()
	{
		var pub = _db.AddPublication().Entity;
		await _db.SaveChangesAsync();

		_page.Id = pub.Id;
		_page.AdditionalMovieFile = CreateMockFormFile("test.bk2", "application/octet-stream");

		var result = await _page.OnPost();

		Assert.IsInstanceOfType<PageResult>(result);
		Assert.IsFalse(_page.ModelState.IsValid);
		Assert.IsTrue(_page.ModelState.ContainsKey(nameof(_page.AdditionalMovieFile)));
	}

	[TestMethod]
	public async Task OnPost_ValidZipFileParseError_AddsModelError()
	{
		var pub = _db.AddPublication().Entity;
		_page.Id = pub.Id;
		_page.AdditionalMovieFile = CreateMockFormFile("test.zip", "application/zip");
		_page.DisplayName = "Test Movie";

		var parseResult = Substitute.For<IParseResult>();
		parseResult.Success.Returns(false);
		parseResult.Errors.Returns(["Parse error occurred"]);
		_parser.ParseZip(Arg.Any<Stream>()).Returns(parseResult);

		var result = await _page.OnPost();

		Assert.IsInstanceOfType<PageResult>(result);
		Assert.IsFalse(_page.ModelState.IsValid);
	}

	[TestMethod]
	public async Task OnPost_ValidZipFileParseSuccess_AddsFileAndRedirects()
	{
		var pub = _db.AddPublication().Entity;
		var user = _db.AddUser("TestUser").Entity;
		await _db.SaveChangesAsync();
		AddAuthenticatedUser(_page, user, [PermissionTo.CreateAdditionalMovieFiles]);

		var parseResult = Substitute.For<IParseResult>();
		parseResult.Success.Returns(true);
		_parser.ParseZip(Arg.Any<Stream>()).Returns(parseResult);

		_page.Id = pub.Id;
		_page.AdditionalMovieFile = CreateMockFormFile("test.zip", "application/zip");
		_page.DisplayName = "Test Movie";

		var result = await _page.OnPost();

		Assert.IsInstanceOfType<RedirectToPageResult>(result);
		var redirect = (RedirectToPageResult)result;
		Assert.AreEqual("AdditionalMovies", redirect.PageName);

		var addedFile = await _db.PublicationFiles
			.SingleOrDefaultAsync(pf => pf.PublicationId == pub.Id);

		Assert.IsNotNull(addedFile);
		Assert.AreEqual("test.zip", addedFile.Path);
		Assert.AreEqual("Test Movie", addedFile.Description);
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
		var file = new PublicationFile
		{
			PublicationId = pub.Id,
			Path = "test.zip",
			Description = "Test Movie",
			Type = FileType.MovieFile,
			FileData = [1, 2, 3]
		};
		_db.PublicationFiles.Add(file);
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
