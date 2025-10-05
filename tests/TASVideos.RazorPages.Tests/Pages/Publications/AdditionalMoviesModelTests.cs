using Microsoft.AspNetCore.Http;
using TASVideos.Core.Services;
using TASVideos.Core.Services.ExternalMediaPublisher;
using TASVideos.Data;
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
		_page = new AdditionalMoviesModel(_publications, _publisher, _maintenanceLogger, _queueService);
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
		const int pubId = 123;
		const string pubTitle = "title";
		const string description = "Description";
		const string path = "Path";
		await _db.SaveChangesAsync();

		_publications.GetTitle(pubId).Returns(pubTitle);
		_publications.GetAvailableMovieFiles(pubId).Returns([new FileEntry(1, description, path)]);

		_page.Id = pubId;

		var result = await _page.OnGet();

		Assert.IsInstanceOfType<PageResult>(result);
		Assert.AreEqual(pubTitle, _page.PublicationTitle);
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
		const int pubId = 123;
		_publications.GetTitle(pubId).Returns("title");
		_publications.GetAvailableMovieFiles(pubId).Returns([]);

		_page.Id = pubId;
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
		const int pubId = 123;
		_publications.GetTitle(pubId).Returns("title");

		_page.Id = pubId;
		_page.AdditionalMovieFile = CreateMockFormFile("test.bk2", "application/octet-stream");
		_page.DisplayName = "Test Movie";

		var parseResult = Substitute.For<IParseResult>();
		parseResult.Success.Returns(false);
		parseResult.Errors.Returns(["Parse error occurred"]);
		_queueService.ParseMovieFile(Arg.Any<IFormFile>()).Returns((parseResult, Array.Empty<byte>()));

		var result = await _page.OnPost();

		Assert.IsInstanceOfType<PageResult>(result);
		Assert.IsFalse(_page.ModelState.IsValid);
	}

	[TestMethod]
	public async Task OnPost_ValidMovieFileParseSuccess_AddsFileAndRedirects()
	{
		const int pubId = 123;
		_publications.GetTitle(pubId).Returns("title");

		var parseResult = Substitute.For<IParseResult>();
		parseResult.Success.Returns(true);
		const string movieName = "test.bk2";
		const string displayName = "Test Movie";
		byte[] movieFile = [1, 2, 3, 4];
		_queueService.ParseMovieFile(Arg.Any<IFormFile>()).Returns((parseResult, movieFile));
		_publications.AddMovieFile(pubId, movieName, displayName, movieFile).Returns(SaveResult.Success);

		_page.Id = pubId;
		_page.AdditionalMovieFile = CreateMockFormFile(movieName, "application/octet-stream");
		_page.DisplayName = displayName;

		var result = await _page.OnPost();

		Assert.IsInstanceOfType<RedirectToPageResult>(result);
		var redirect = (RedirectToPageResult)result;
		Assert.AreEqual("AdditionalMovies", redirect.PageName);
		Assert.AreEqual("success", _page.MessageType);

		await _maintenanceLogger.Received(1).Log(pubId, Arg.Any<int>(), Arg.Any<string>());
		await _publisher.Received(1).Send(Arg.Any<Post>());
	}

	[TestMethod]
	public async Task OnPostDelete_FileNotFound_RedirectsWithError()
	{
		_page.Id = 1;

		var result = await _page.OnPostDelete(999);

		Assert.IsInstanceOfType<RedirectToPageResult>(result);
		var redirect = (RedirectToPageResult)result;
		Assert.AreEqual("AdditionalMovies", redirect.PageName);
		Assert.AreEqual("danger", _page.MessageType);
	}

	[TestMethod]
	public async Task OnPostDelete_SaveFailure_RedirectsWithError()
	{
		const int pubId = 123;
		const int pubFileId = 567;
		_publications.RemoveFile(pubFileId).Returns((new PublicationFile { PublicationId = pubId }, SaveResult.ConcurrencyFailure));
		_page.Id = pubId;

		var result = await _page.OnPostDelete(pubFileId);
		Assert.IsInstanceOfType<RedirectToPageResult>(result);
		var redirect = (RedirectToPageResult)result;
		Assert.AreEqual("AdditionalMovies", redirect.PageName);
		Assert.AreEqual("danger", _page.MessageType);
	}

	[TestMethod]
	public async Task OnPostDelete_ValidFile_DeletesFileAndRedirects()
	{
		const int pubId = 123;
		const int pubFileId = 567;

		_publications.RemoveFile(pubFileId).Returns((new PublicationFile { PublicationId = pubId }, SaveResult.Success));
		_page.Id = pubId;

		var result = await _page.OnPostDelete(pubFileId);

		Assert.IsInstanceOfType<RedirectToPageResult>(result);
		var redirect = (RedirectToPageResult)result;
		Assert.AreEqual("AdditionalMovies", redirect.PageName);

		await _maintenanceLogger.Received(1).Log(pubId, Arg.Any<int>(), Arg.Any<string>());
		await _publisher.Received(1).Send(Arg.Any<Post>());
	}
}
