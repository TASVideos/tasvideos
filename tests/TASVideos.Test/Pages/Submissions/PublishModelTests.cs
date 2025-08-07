using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using TASVideos.Core.Services;
using TASVideos.Core.Services.Wiki;
using TASVideos.Core.Services.Youtube;
using TASVideos.Data.Entity;
using TASVideos.Pages.Submissions;
using TASVideos.Services;
using TASVideos.Tests.Base;

namespace TASVideos.RazorPages.Tests.Pages.Submissions;

[TestClass]
public class PublishModelTests : TestDbBase
{
	private readonly IWikiPages _wikiPages;
	private readonly PublishModel _page;

	public PublishModelTests()
	{
		var publisher = Substitute.For<IExternalMediaPublisher>();
		_wikiPages = Substitute.For<IWikiPages>();
		var uploader = Substitute.For<IMediaFileUploader>();
		var tasVideoAgent = Substitute.For<ITASVideoAgent>();
		var userManager = Substitute.For<IUserManager>();
		var fileService = Substitute.For<IFileService>();
		var youtubeSync = Substitute.For<IYoutubeSync>();
		var queueService = Substitute.For<IQueueService>();
		var env = Substitute.For<IWebHostEnvironment>();
		_page = new PublishModel(_db, publisher, _wikiPages, uploader, tasVideoAgent, userManager, fileService, youtubeSync, queueService, env);
	}

	[TestMethod]
	public async Task OnGet_NoSubmission_ReturnsNotFound()
	{
		_page.Id = 999;

		var actual = await _page.OnGet();

		Assert.IsInstanceOfType<NotFoundResult>(actual);
	}

	[TestMethod]
	public async Task OnGet_SubmissionCannotBePublished_ReturnsAccessDenied()
	{
		var submission = _db.AddAndSaveUnpublishedSubmission().Entity;
		submission.Status = SubmissionStatus.New;
		await _db.SaveChangesAsync();

		_page.Id = submission.Id;

		var actual = await _page.OnGet();

		Assert.IsInstanceOfType<RedirectToPageResult>(actual);
		var redirectResult = (RedirectToPageResult)actual;
		Assert.AreEqual("/Account/AccessDenied", redirectResult.PageName);
	}

	[TestMethod]
	public async Task OnGet_ValidSubmission_PopulatesData()
	{
		var submission = _db.CreatePublishableSubmission().Entity;
		_page.Id = submission.Id;
		const string markup = "Test submission markup";
		var wikiPage = new WikiResult { Markup = markup };
		_wikiPages.Page($"InternalSystem/SubmissionContent/S{submission.Id}").Returns(wikiPage);

		var actual = await _page.OnGet();

		Assert.IsInstanceOfType<PageResult>(actual);
		Assert.AreEqual(submission.Title, _page.Submission.Title);
		Assert.IsTrue(_page.Submission.CanPublish);
		Assert.AreEqual(markup, _page.Markup);
	}

	[TestMethod]
	public async Task OnPost_InvalidScreenshot_ShowsError()
	{
		var submission = _db.CreatePublishableSubmission().Entity;
		_page.Id = submission.Id;
		_page.Submission = CreateValidPublishModel();

		var invalidScreenshot = CreateInvalidScreenshotFile();
		_page.Submission = new PublishModel.SubmissionPublishModel
		{
			MovieDescription = "Test description",
			MovieFilename = "test-movie",
			MovieExtension = "bk2",
			OnlineWatchingUrl = "https://example.com/video",
			Screenshot = invalidScreenshot,
			ScreenshotDescription = "Test screenshot",
			SelectedFlags = [],
			SelectedTags = []
		};

		var actual = await _page.OnPost();

		Assert.IsInstanceOfType<PageResult>(actual);
		Assert.IsFalse(_page.ModelState.IsValid);
		Assert.IsTrue(_page.ModelState.ContainsKey("Submission.Screenshot"));
	}

	[TestMethod]
	public async Task OnPost_DuplicateMovieFileName_ShowsError()
	{
		var submission = _db.CreatePublishableSubmission().Entity;
		_page.Id = submission.Id;
		_page.Submission = CreateValidPublishModel();

		// Add existing publication with the same filename
		var existingPub = _db.AddPublication().Entity;
		existingPub.MovieFileName = _page.Submission.MovieFilename + "." + _page.Submission.MovieExtension;
		await _db.SaveChangesAsync();

		var actual = await _page.OnPost();

		Assert.IsInstanceOfType<PageResult>(actual);
		Assert.IsFalse(_page.ModelState.IsValid);
		Assert.IsTrue(_page.ModelState.ContainsKey("Submission.MovieFilename"));
	}

	[TestMethod]
	public async Task OnGetObsoletePublication_ValidId_ReturnsPublicationData()
	{
		var pub = _db.AddPublication().Entity;
		var tag = _db.Tags.Add(new Tag { Id = 1, Code = "test", DisplayName = "Test" }).Entity;
		_db.PublicationTags.Add(new PublicationTag { Publication = pub, Tag = tag });
		await _db.SaveChangesAsync();

		var wikiPage = new WikiResult { Markup = "Test publication markup" };
		_wikiPages.Page($"InternalSystem/PublicationContent/M{pub.Id}").Returns(ValueTask.FromResult((IWikiPage?)wikiPage));

		var actual = await _page.OnGetObsoletePublication(pub.Id);

		Assert.IsInstanceOfType<JsonResult>(actual);
		var jsonResult = (JsonResult)actual;
		Assert.IsNotNull(jsonResult.Value);
	}

	[TestMethod]
	public async Task OnGetObsoletePublication_InvalidId_ReturnsBadRequest()
	{
		var actual = await _page.OnGetObsoletePublication(999);

		Assert.IsInstanceOfType<BadRequestObjectResult>(actual);
		var badRequestResult = (BadRequestObjectResult)actual;
		Assert.IsNotNull(badRequestResult.Value);
		Assert.IsTrue(badRequestResult.Value.ToString()!.Contains("Unable to find publication"));
	}

	private static PublishModel.SubmissionPublishModel CreateValidPublishModel() => new()
	{
		MovieDescription = "Test description",
		MovieFilename = "test-movie",
		MovieExtension = "bk2",
		OnlineWatchingUrl = "https://example.com/video",
		Screenshot = CreateValidScreenshotFile(),
		ScreenshotDescription = "Test screenshot",
		SelectedFlags = [],
		SelectedTags = []
	};

	private static IFormFile CreateValidScreenshotFile()
	{
		var bytes = new byte[] { 0x89, 0x50, 0x4E, 0x47 }; // PNG header
		var stream = new MemoryStream(bytes);
		return new FormFile(stream, 0, bytes.Length, "screenshot", "screenshot.png")
		{
			Headers = new HeaderDictionary(),
			ContentType = "image/png"
		};
	}

	private static IFormFile CreateInvalidScreenshotFile()
	{
		byte[] bytes = [0x00, 0x00, 0x00, 0x00]; // Invalid header
		var stream = new MemoryStream(bytes);
		return new FormFile(stream, 0, bytes.Length, "screenshot", "screenshot.txt")
		{
			Headers = new HeaderDictionary(),
			ContentType = "text/plain"
		};
	}
}
