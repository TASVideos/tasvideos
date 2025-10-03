using Microsoft.AspNetCore.Http;
using TASVideos.Core.Services;
using TASVideos.Core.Services.Wiki;
using TASVideos.Extensions;
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
		var queueService = Substitute.For<IQueueService>();
		_page = new PublishModel(_db, publisher, _wikiPages, queueService);
	}

	#region OnGet

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

		var result = await _page.OnGet();

		AssertAccessDenied(result);
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
	public async Task OnGetObsoletePublication_ValidId_ReturnsPublicationData()
	{
		var pub = _db.AddPublication().Entity;
		var tag1 = _db.AttachTag(pub, "test1").Entity;
		var tag2 = _db.AttachTag(pub, "test2").Entity;
		await _db.SaveChangesAsync();

		const string expectedMarkup = "Test publication markup content";
		var queueService = Substitute.For<IQueueService>();
		var expectedResult = new ObsoletePublicationResult(pub.Title, [tag1.Id, tag2.Id], expectedMarkup);
		queueService.GetObsoletePublicationTags(pub.Id).Returns(expectedResult);

		var newPage = new PublishModel(_db, Substitute.For<IExternalMediaPublisher>(), _wikiPages, queueService);
		var actual = await newPage.OnGetObsoletePublication(pub.Id);

		Assert.IsInstanceOfType<JsonResult>(actual);
		var jsonResult = (JsonResult)actual;
		Assert.IsNotNull(jsonResult.Value);
		Assert.AreEqual(expectedResult, jsonResult.Value);
		await queueService.Received(1).GetObsoletePublicationTags(pub.Id);
	}

	[TestMethod]
	public async Task OnGetObsoletePublication_InvalidId_ReturnsBadRequest()
	{
		const int invalidId = 999;
		var queueService = Substitute.For<IQueueService>();
		queueService.GetObsoletePublicationTags(invalidId).Returns((ObsoletePublicationResult?)null);

		var newPage = new PublishModel(_db, Substitute.For<IExternalMediaPublisher>(), _wikiPages, queueService);
		var actual = await newPage.OnGetObsoletePublication(invalidId);

		Assert.IsInstanceOfType<BadRequestObjectResult>(actual);
		var badRequestResult = (BadRequestObjectResult)actual;
		Assert.IsNotNull(badRequestResult.Value);
		Assert.AreEqual($"Unable to find publication with an id of {invalidId}", badRequestResult.Value);
		await queueService.Received(1).GetObsoletePublicationTags(invalidId);
	}

	[TestMethod]
	public async Task OnGetObsoletePublication_ServiceThrowsException_PropagatesException()
	{
		const int publicationId = 123;
		var queueService = Substitute.For<IQueueService>();
		var expectedException = new Exception("Database connection failed");
		queueService.GetObsoletePublicationTags(publicationId).Returns(Task.FromException<ObsoletePublicationResult?>(expectedException));

		var newPage = new PublishModel(_db, Substitute.For<IExternalMediaPublisher>(), _wikiPages, queueService);

		var actualException = await Assert.ThrowsExactlyAsync<Exception>(() => newPage.OnGetObsoletePublication(publicationId));
		Assert.AreEqual(expectedException.Message, actualException.Message);
		await queueService.Received(1).GetObsoletePublicationTags(publicationId);
	}

	#endregion

	#region OnPost

	[TestMethod]
	public async Task OnPost_NonExistentSubmission_ReturnsNotFound()
	{
		var queueService = Substitute.For<IQueueService>();
		queueService.Publish(Arg.Any<PublishSubmissionRequest>())
			.Returns(new FailedPublishSubmissionResult("Submission not found or cannot be published"));

		var newPage = new PublishModel(_db, Substitute.For<IExternalMediaPublisher>(), _wikiPages, queueService)
		{
			Id = 999,
			Submission = CreateValidPublishModel()
		};

		var actual = await newPage.OnPost();

		Assert.IsInstanceOfType<NotFoundResult>(actual);
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
		var publishModel = CreateValidPublishModel();

		var queueService = Substitute.For<IQueueService>();
		queueService.Publish(Arg.Any<PublishSubmissionRequest>())
			.Returns(new FailedPublishSubmissionResult("Movie filename test-movie.bk2 already exists"));

		var newPage = new PublishModel(_db, Substitute.For<IExternalMediaPublisher>(), _wikiPages, queueService)
		{
			Id = submission.Id,
			Submission = publishModel
		};

		var actual = await newPage.OnPost();

		Assert.IsInstanceOfType<PageResult>(actual);
		Assert.IsFalse(newPage.ModelState.IsValid);
		Assert.IsTrue(newPage.ModelState.ContainsKey("Submission.MovieFilename"));
	}

	[TestMethod]
	public async Task OnPost_UnpublishableSubmissionStatus_ReturnsNotFound()
	{
		var submission = _db.AddAndSaveUnpublishedSubmission().Entity;
		submission.Status = SubmissionStatus.New;
		await _db.SaveChangesAsync();

		var queueService = Substitute.For<IQueueService>();
		queueService.Publish(Arg.Any<PublishSubmissionRequest>())
			.Returns(new FailedPublishSubmissionResult("Submission not found or cannot be published"));

		var newPage = new PublishModel(_db, Substitute.For<IExternalMediaPublisher>(), _wikiPages, queueService)
		{
			Id = submission.Id,
			Submission = CreateValidPublishModel()
		};

		var actual = await newPage.OnPost();

		Assert.IsInstanceOfType<NotFoundResult>(actual);
	}

	[TestMethod]
	public async Task OnPost_InvalidMovieToObsolete_ShowsError()
	{
		var submission = _db.CreatePublishableSubmission().Entity;
		var publishModel = new PublishModel.SubmissionPublishModel
		{
			MovieDescription = "Test description",
			MovieFilename = "test-movie",
			MovieExtension = "bk2",
			OnlineWatchingUrl = "https://example.com/video",
			Screenshot = CreateValidScreenshotFile(),
			ScreenshotDescription = "Test screenshot",
			SelectedFlags = [],
			SelectedTags = [],
			MovieToObsolete = 999
		};

		var queueService = Substitute.For<IQueueService>();
		queueService.Publish(Arg.Any<PublishSubmissionRequest>())
			.Returns(new FailedPublishSubmissionResult("Publication to obsolete does not exist"));

		var newPage = new PublishModel(_db, Substitute.For<IExternalMediaPublisher>(), _wikiPages, queueService)
		{
			Id = submission.Id,
			Submission = publishModel
		};

		var actual = await newPage.OnPost();

		Assert.IsInstanceOfType<PageResult>(actual);
		Assert.IsFalse(newPage.ModelState.IsValid);
		Assert.IsTrue(newPage.ModelState.ContainsKey("Submission.MovieToObsolete"));
	}

	[TestMethod]
	public async Task OnPost_ValidSubmission_CreatesPublicationAndRedirects()
	{
		var submission = _db.CreatePublishableSubmission().Entity;
		_page.Id = submission.Id;
		_page.Submission = CreateValidPublishModel();
		_page.Markup = "Test publication markup content";

		var queueService = Substitute.For<IQueueService>();

		// Create a new page instance with the mocked service
		var newPage = new PublishModel(_db, Substitute.For<IExternalMediaPublisher>(), _wikiPages, queueService)
		{
			Id = submission.Id,
			Submission = CreateValidPublishModel(),
			Markup = "Test publication markup content"
		};

		// Set up the publication for post-transaction queries
		var publication = _db.AddPublication().Entity;
		await _db.SaveChangesAsync();
		var expectedPublicationId = publication.Id;

		queueService.Publish(Arg.Any<PublishSubmissionRequest>())
			.Returns(new PublishSubmissionResult(null, expectedPublicationId, "", "", []));

		var publicationPageName = WikiHelper.ToPublicationWikiPageName(expectedPublicationId);
		_wikiPages.Page(publicationPageName).Returns(new WikiResult { Markup = "Publication wiki page" });

		var actual = await newPage.OnPost();

		Assert.IsInstanceOfType<RedirectResult>(actual);
		var redirectResult = (RedirectResult)actual;
		Assert.IsTrue(redirectResult.Url.Contains($"{expectedPublicationId}M"), "Should redirect to publication page");

		// Verify the service was called with correct parameters
		await queueService.Received(1).Publish(Arg.Is<PublishSubmissionRequest>(r =>
			r.SubmissionId == submission.Id &&
			r.MovieDescription == newPage.Submission.MovieDescription &&
			r.MovieFilename == newPage.Submission.MovieFilename));
	}

	[TestMethod]
	public async Task OnPost_ServiceReturnsError_ShowsError()
	{
		var submission = _db.CreatePublishableSubmission().Entity;
		_page.Id = submission.Id;
		_page.Submission = CreateValidPublishModel();

		var queueService = Substitute.For<IQueueService>();
		queueService.Publish(Arg.Any<PublishSubmissionRequest>())
			.Returns(new FailedPublishSubmissionResult("Test error message"));

		// Create a new page instance with the mocked service
		var newPage = new PublishModel(_db, Substitute.For<IExternalMediaPublisher>(), _wikiPages, queueService)
		{
			Id = submission.Id,
			Submission = CreateValidPublishModel()
		};

		var actual = await newPage.OnPost();

		Assert.IsInstanceOfType<PageResult>(actual);
		Assert.IsFalse(newPage.ModelState.IsValid);
		Assert.IsTrue(newPage.ModelState.ContainsKey(""));
	}

	#endregion

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

	[TestMethod]
	public void RequiresPermission() => AssertHasPermission(typeof(PublishModel), PermissionTo.PublishMovies);
}
