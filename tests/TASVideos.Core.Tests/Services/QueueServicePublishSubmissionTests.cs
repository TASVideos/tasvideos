using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using TASVideos.Core.Services.Wiki;
using TASVideos.Core.Services.Youtube;
using TASVideos.Core.Settings;
using TASVideos.Data.Entity;
using TASVideos.MovieParsers;

namespace TASVideos.Core.Tests.Services;

[TestClass]
public class QueueServicePublishSubmissionTests : TestDbBase
{
	private readonly QueueService _queueService;
	private readonly IWikiPages _wikiPages;
	private readonly IMediaFileUploader _uploader;
	private readonly IFileService _fileService;
	private readonly IUserManager _userManager;
	private readonly IYoutubeSync _youtubeSync;
	private readonly ITASVideoAgent _tva;

	public QueueServicePublishSubmissionTests()
	{
		_wikiPages = Substitute.For<IWikiPages>();
		_uploader = Substitute.For<IMediaFileUploader>();
		_fileService = Substitute.For<IFileService>();
		_userManager = Substitute.For<IUserManager>();
		_youtubeSync = Substitute.For<IYoutubeSync>();
		_tva = Substitute.For<ITASVideoAgent>();

		var settings = new AppSettings
		{
			MinimumHoursBeforeJudgment = 72,
			SubmissionRate = new() { Days = 30, Submissions = 5 }
		};

		var movieParser = Substitute.For<IMovieParser>();
		var deprecator = Substitute.For<IMovieFormatDeprecator>();
		var forumService = Substitute.For<IForumService>();
		var tasvideosGrue = Substitute.For<ITASVideosGrue>();
		var topicWatcher = Substitute.For<ITopicWatcher>();
		_queueService = new QueueService(settings, _db, _youtubeSync, _tva, _wikiPages, _uploader, _fileService, _userManager, movieParser, deprecator, forumService, tasvideosGrue, topicWatcher);
	}

	[TestMethod]
	public async Task PublishSubmission_NonExistentSubmission_ReturnsError()
	{
		var request = CreateValidRequest(999);

		var result = await _queueService.Publish(request);

		Assert.IsFalse(result.Success);
		Assert.AreEqual("Submission not found or cannot be published", result.ErrorMessage);
	}

	[TestMethod]
	public async Task PublishSubmission_UnpublishableSubmission_ReturnsError()
	{
		var submission = _db.AddAndSaveUnpublishedSubmission().Entity;
		submission.Status = SubmissionStatus.New;
		await _db.SaveChangesAsync();

		var request = CreateValidRequest(submission.Id);

		var result = await _queueService.Publish(request);

		Assert.IsFalse(result.Success);
		Assert.AreEqual("Submission not found or cannot be published", result.ErrorMessage);
	}

	[TestMethod]
	public async Task PublishSubmission_DuplicateMovieFileName_ReturnsError()
	{
		var submission = _db.CreatePublishableSubmission().Entity;
		var existingPub = _db.AddPublication().Entity;
		existingPub.MovieFileName = "test-movie.bk2";
		await _db.SaveChangesAsync();

		var request = CreateValidRequest(submission.Id);

		var result = await _queueService.Publish(request);

		Assert.IsFalse(result.Success);
		Assert.IsTrue(result.ErrorMessage!.Contains("Movie filename test-movie.bk2 already exists"));
	}

	[TestMethod]
	public async Task PublishSubmission_InvalidObsoletePublication_ReturnsError()
	{
		var submission = _db.CreatePublishableSubmission().Entity;
		var request = CreateValidRequest(submission.Id, movieToObsolete: 999);

		var result = await _queueService.Publish(request);

		Assert.IsFalse(result.Success);
		Assert.AreEqual("Publication to obsolete does not exist", result.ErrorMessage);
	}

	[TestMethod]
	public async Task PublishSubmission_ValidSubmission_CreatesPublication()
	{
		var submission = _db.CreatePublishableSubmission().Entity;
		_fileService.CopyZip(Arg.Any<byte[]>(), Arg.Any<string>()).Returns([1, 2, 3, 4]);
		_wikiPages.Add(Arg.Any<WikiCreateRequest>()).Returns(new WikiResult { Markup = "Created wiki page" });

		var request = CreateValidRequest(submission.Id);

		var result = await _queueService.Publish(request);

		Assert.IsTrue(result.Success);
		Assert.IsTrue(result.PublicationId > 0); // Should be set by SaveChanges

		var publication = await _db.Publications.SingleOrDefaultAsync(p => p.SubmissionId == submission.Id);
		Assert.IsNotNull(publication);
		Assert.AreEqual("test-movie.bk2", publication.MovieFileName);

		var updatedSubmission = await _db.Submissions.FindAsync(submission.Id);
		Assert.IsNotNull(updatedSubmission);
		Assert.AreEqual(SubmissionStatus.Published, updatedSubmission.Status);

		await _uploader.Received(1).UploadScreenshot(Arg.Any<int>(), Arg.Any<IFormFile>(), Arg.Any<string?>());
		await _wikiPages.Received(1).Add(Arg.Any<WikiCreateRequest>());
		await _userManager.Received(1).AssignAutoAssignableRolesByPublication(Arg.Any<IEnumerable<int>>(), Arg.Any<string>());
		await _tva.Received(1).PostSubmissionPublished(submission.Id, Arg.Any<int>());
	}

	[TestMethod]
	public async Task PublishSubmission_WithObsoletePublication_ObsoletesPublication()
	{
		var submission = _db.CreatePublishableSubmission().Entity;
		var publicationToObsolete = _db.AddPublication().Entity;
		await _db.SaveChangesAsync();

		_fileService.CopyZip(Arg.Any<byte[]>(), Arg.Any<string>()).Returns([1, 2, 3, 4]);
		_wikiPages.Add(Arg.Any<WikiCreateRequest>()).Returns(new WikiResult { Markup = "Created wiki page" });
		_wikiPages.Page(Arg.Any<string>()).Returns(new WikiResult { Markup = "Publication page" });

		var request = CreateValidRequest(submission.Id, movieToObsolete: publicationToObsolete.Id);

		var result = await _queueService.Publish(request);

		Assert.IsTrue(result.Success);

		var obsoletedPub = await _db.Publications.FindAsync(publicationToObsolete.Id);
		Assert.IsNotNull(obsoletedPub);
		Assert.IsNotNull(obsoletedPub.ObsoletedById);
	}

	[TestMethod]
	public async Task PublishSubmission_WithYoutubeUrl_SyncsYoutubeVideo()
	{
		var submission = _db.CreatePublishableSubmission().Entity;
		_fileService.CopyZip(Arg.Any<byte[]>(), Arg.Any<string>()).Returns([1, 2, 3, 4]);
		_wikiPages.Add(Arg.Any<WikiCreateRequest>()).Returns(new WikiResult { Markup = "Created wiki page" });
		_youtubeSync.IsYoutubeUrl("https://www.youtube.com/watch?v=test123").Returns(true);

		var request = CreateValidRequest(submission.Id, onlineWatchingUrl: "https://www.youtube.com/watch?v=test123");

		var result = await _queueService.Publish(request);

		Assert.IsTrue(result.Success);

		// Verify YouTube sync was called for the main watching URL
		await _youtubeSync.Received(1).SyncYouTubeVideo(Arg.Is<YoutubeVideo>(v =>
			v.Id > 0 &&
			v.Url == "https://www.youtube.com/watch?v=test123" &&
			v.UrlDisplayName == ""));
	}

	[TestMethod]
	public async Task PublishSubmission_WithAlternateYoutubeUrl_SyncsAlternateYoutubeVideo()
	{
		var submission = _db.CreatePublishableSubmission().Entity;
		_fileService.CopyZip(Arg.Any<byte[]>(), Arg.Any<string>()).Returns([1, 2, 3, 4]);
		_wikiPages.Add(Arg.Any<WikiCreateRequest>()).Returns(new WikiResult { Markup = "Created wiki page" });
		_youtubeSync.IsYoutubeUrl("https://www.youtube.com/watch?v=alternate123").Returns(true);

		var request = CreateValidRequest(
			submission.Id,
			alternateOnlineWatchingUrl: "https://www.youtube.com/watch?v=alternate123",
			alternateOnlineWatchUrlName: "Alternate Video");

		var result = await _queueService.Publish(request);

		Assert.IsTrue(result.Success);

		// Verify YouTube sync was called for the alternate watching URL
		await _youtubeSync.Received(1).SyncYouTubeVideo(Arg.Is<YoutubeVideo>(v =>
			v.Id > 0 &&
			v.Url == "https://www.youtube.com/watch?v=alternate123" &&
			v.UrlDisplayName == "Alternate Video"));
	}

	[TestMethod]
	public async Task PublishSubmission_WithBothYoutubeUrls_SyncsBothYoutubeVideos()
	{
		var submission = _db.CreatePublishableSubmission().Entity;
		_fileService.CopyZip(Arg.Any<byte[]>(), Arg.Any<string>()).Returns([1, 2, 3, 4]);
		_wikiPages.Add(Arg.Any<WikiCreateRequest>()).Returns(new WikiResult { Markup = "Created wiki page" });
		_youtubeSync.IsYoutubeUrl("https://www.youtube.com/watch?v=main123").Returns(true);
		_youtubeSync.IsYoutubeUrl("https://www.youtube.com/watch?v=alternate123").Returns(true);

		var request = CreateValidRequest(
			submission.Id,
			onlineWatchingUrl: "https://www.youtube.com/watch?v=main123",
			alternateOnlineWatchingUrl: "https://www.youtube.com/watch?v=alternate123",
			alternateOnlineWatchUrlName: "Commentary Track");

		var result = await _queueService.Publish(request);

		Assert.IsTrue(result.Success);

		// Verify YouTube sync was called for both URLs
		await _youtubeSync.Received(1).SyncYouTubeVideo(Arg.Is<YoutubeVideo>(v =>
			v.Url == "https://www.youtube.com/watch?v=main123" && v.UrlDisplayName == ""));
		await _youtubeSync.Received(1).SyncYouTubeVideo(Arg.Is<YoutubeVideo>(v =>
			v.Url == "https://www.youtube.com/watch?v=alternate123" && v.UrlDisplayName == "Commentary Track"));
	}

	[TestMethod]
	public async Task PublishSubmission_WithNonYoutubeUrls_DoesNotSyncVideos()
	{
		var submission = _db.CreatePublishableSubmission().Entity;
		_fileService.CopyZip(Arg.Any<byte[]>(), Arg.Any<string>()).Returns([1, 2, 3, 4]);
		_wikiPages.Add(Arg.Any<WikiCreateRequest>()).Returns(new WikiResult { Markup = "Created wiki page" });
		_youtubeSync.IsYoutubeUrl(Arg.Any<string>()).Returns(false);

		var request = CreateValidRequest(submission.Id, onlineWatchingUrl: "https://example.com/video");

		var result = await _queueService.Publish(request);

		Assert.IsTrue(result.Success);

		// Verify YouTube sync was never called
		await _youtubeSync.DidNotReceive().SyncYouTubeVideo(Arg.Any<YoutubeVideo>());
	}

	private static PublishSubmissionRequest CreateValidRequest(
		int submissionId,
		int? movieToObsolete = null,
		string onlineWatchingUrl = "https://example.com/video",
		string? alternateOnlineWatchingUrl = null,
		string? alternateOnlineWatchUrlName = null)
	{
		var screenshot = CreateValidScreenshotFile();
		return new PublishSubmissionRequest(
			submissionId,
			"Test movie description",
			"test-movie",
			"bk2",
			onlineWatchingUrl,
			alternateOnlineWatchingUrl,
			alternateOnlineWatchUrlName,
			null,
			screenshot,
			"Test screenshot",
			[],
			[],
			movieToObsolete,
			1);
	}

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
}
