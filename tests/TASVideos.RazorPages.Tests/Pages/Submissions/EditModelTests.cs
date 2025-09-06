using TASVideos.Core.Services;
using TASVideos.Core.Services.Wiki;
using TASVideos.Core.Services.Youtube;
using TASVideos.MovieParsers;
using TASVideos.Pages.Submissions;
using TASVideos.Services;
using TASVideos.Tests.Base;

namespace TASVideos.RazorPages.Tests.Pages.Submissions;

[TestClass]
public class EditModelTests : TestDbBase
{
	private readonly IWikiPages _wikiPages;
	private readonly IQueueService _queueService;
	private readonly ITopicWatcher _topicWatcher;
	private readonly EditModel _page;

	public EditModelTests()
	{
		var parser = Substitute.For<IMovieParser>();
		_wikiPages = Substitute.For<IWikiPages>();
		var publisher = Substitute.For<IExternalMediaPublisher>();
		var tasvideosGrue = Substitute.For<ITASVideosGrue>();
		var deprecator = Substitute.For<IMovieFormatDeprecator>();
		_queueService = Substitute.For<IQueueService>();
		var youtubeSync = Substitute.For<IYoutubeSync>();
		var forumService = Substitute.For<IForumService>();
		_topicWatcher = Substitute.For<ITopicWatcher>();
		var fileService = Substitute.For<IFileService>();
		_page = new EditModel(_db, parser, _wikiPages, publisher, tasvideosGrue, deprecator, _queueService, youtubeSync, forumService, _topicWatcher, fileService);
	}

	[TestMethod]
	public async Task OnGet_NoSubmission_ReturnsNotFound()
	{
		_page.Id = 999;

		var actual = await _page.OnGet();

		Assert.IsInstanceOfType<NotFoundResult>(actual);
	}

	[TestMethod]
	public async Task OnGet_UserCannotEdit_ReturnsAccessDenied()
	{
		var submission = _db.AddAndSaveUnpublishedSubmission().Entity;
		var otherUser = _db.AddUser("OtherUser").Entity;

		_page.Id = submission.Id;
		AddAuthenticatedUser(_page, otherUser, [PermissionTo.SubmitMovies]);

		var result = await _page.OnGet();

		AssertAccessDenied(result);
	}

	[TestMethod]
	public async Task OnGet_SubmitterCanEdit_ReturnsPage()
	{
		var submission = _db.AddAndSaveUnpublishedSubmission().Entity;
		_page.Id = submission.Id;
		AddAuthenticatedUser(_page, submission.Submitter!, [PermissionTo.SubmitMovies]);

		const string markup = "Test submission markup";
		var wikiPage = new WikiResult { Markup = markup };
		_wikiPages.Page($"InternalSystem/SubmissionContent/S{submission.Id}").Returns(wikiPage);

		_queueService.AvailableStatuses(
			submission.Status,
			Arg.Any<IEnumerable<PermissionTo>>(),
			Arg.Any<DateTime>(),
			true,
			false,
			false)
			.Returns([SubmissionStatus.New, SubmissionStatus.Cancelled]);

		var actual = await _page.OnGet();

		Assert.IsInstanceOfType<PageResult>(actual);
		Assert.AreEqual(submission.Submitter!.UserName, _page.Submission.Submitter);
		Assert.AreEqual(markup, _page.Markup);
		Assert.AreEqual(2, _page.AvailableStatuses.Count);
	}

	[TestMethod]
	public async Task OnGet_AuthorCanEdit_ReturnsPage()
	{
		var submission = _db.AddAndSaveUnpublishedSubmission().Entity;
		var author = _db.AddUser("Author").Entity;

		_db.SubmissionAuthors.Add(new SubmissionAuthor
		{
			Submission = submission,
			Author = author,
			Ordinal = 1
		});
		await _db.SaveChangesAsync();

		_page.Id = submission.Id;
		AddAuthenticatedUser(_page, author, [PermissionTo.SubmitMovies]);

		var wikiPage = new WikiResult { Markup = "Test submission markup" };
		_wikiPages.Page($"InternalSystem/SubmissionContent/S{submission.Id}").Returns(wikiPage);

		_queueService.AvailableStatuses(
			submission.Status,
			Arg.Any<IEnumerable<PermissionTo>>(),
			submission.CreateTimestamp,
			true,
			false,
			false)
			.Returns([SubmissionStatus.New, SubmissionStatus.Cancelled]);

		var actual = await _page.OnGet();

		Assert.IsInstanceOfType<PageResult>(actual);
		Assert.AreEqual(submission.Submitter!.UserName, _page.Submission.Submitter);
	}

	[TestMethod]
	public async Task OnPost_InvalidStatus_ValidationError()
	{
		var submission = _db.AddAndSaveUnpublishedSubmission().Entity;
		_page.Id = submission.Id;
		_page.Submission = new EditModel.SubmissionEdit
		{
			GameName = "Test Game",
			Status = SubmissionStatus.Published,
			Authors = ["TestAuthor"]
		};

		AddAuthenticatedUser(_page, submission.Submitter!, [PermissionTo.SubmitMovies]);

		_queueService.AvailableStatuses(
			Arg.Any<SubmissionStatus>(),
			Arg.Any<IEnumerable<PermissionTo>>(),
			Arg.Any<DateTime>(),
			Arg.Any<bool>(),
			Arg.Any<bool>(),
			Arg.Any<bool>())
			.Returns([SubmissionStatus.New, SubmissionStatus.Cancelled]);

		var actual = await _page.OnPost();

		Assert.IsInstanceOfType<PageResult>(actual);
		Assert.IsFalse(_page.ModelState.IsValid);
		Assert.IsTrue(_page.ModelState.ContainsKey("Submission.Status"));
	}

	[TestMethod]
	public async Task OnPost_ValidEdit_UpdatesSubmissionAndRedirects()
	{
		var submission = _db.AddAndSaveUnpublishedSubmission().Entity;
		submission.TopicId = 1;

		_page.Id = submission.Id;
		_page.Submission = new EditModel.SubmissionEdit
		{
			GameName = "Updated Game",
			GameVersion = "Updated Version",
			Status = SubmissionStatus.New,
			Authors = ["UpdatedAuthor"],
			Emulator = "Updated Emulator",
			Goal = "Updated Goal",
			RomName = "Updated ROM"
		};
		_page.MarkupChanged = true;
		_page.Markup = "Updated markup";

		AddAuthenticatedUser(_page, submission.Submitter!, [PermissionTo.SubmitMovies]);

		_queueService.AvailableStatuses(
			Arg.Any<SubmissionStatus>(),
			Arg.Any<IEnumerable<PermissionTo>>(),
			Arg.Any<DateTime>(),
			Arg.Any<bool>(),
			Arg.Any<bool>(),
			Arg.Any<bool>())
			.Returns([SubmissionStatus.New, SubmissionStatus.Cancelled]);

		var wikiPage = new WikiResult { Markup = "Updated markup" };
		_wikiPages.Add(Arg.Any<WikiCreateRequest>()).Returns(wikiPage);

		var result = await _page.OnPost();

		AssertRedirect(result, "View", submission.Id);
		Assert.AreEqual("Updated Game", _page.Submission.GameName);
		Assert.AreEqual("Updated Version", _page.Submission.GameVersion);
		Assert.AreEqual("Updated Emulator", _page.Submission.Emulator);
	}

	[TestMethod]
	public async Task OnPost_JudgeClaimsSubmission_SetsJudge()
	{
		var submission = _db.AddAndSaveUnpublishedSubmission().Entity;
		_page.Id = submission.Id;
		_page.Submission = new EditModel.SubmissionEdit
		{
			GameName = "Test Game",
			Status = SubmissionStatus.JudgingUnderWay,
			Authors = ["TestAuthor"]
		};

		var judge = _db.AddUser("Judge").Entity;
		AddAuthenticatedUser(_page, judge, [PermissionTo.JudgeSubmissions]);

		_queueService.AvailableStatuses(
			Arg.Any<SubmissionStatus>(),
			Arg.Any<IEnumerable<PermissionTo>>(),
			Arg.Any<DateTime>(),
			Arg.Any<bool>(),
			Arg.Any<bool>(),
			Arg.Any<bool>())
			.Returns([SubmissionStatus.JudgingUnderWay]);

		var actual = await _page.OnPost();

		Assert.IsInstanceOfType<RedirectToPageResult>(actual);
		Assert.AreEqual(SubmissionStatus.JudgingUnderWay, _page.Submission.Status);
	}

	[TestMethod]
	public async Task OnPost_SubmissionRejected_CallsGrueRejectAndMove()
	{
		var submission = _db.AddAndSaveUnpublishedSubmission().Entity;
		submission.Status = SubmissionStatus.New;
		await _db.SaveChangesAsync();

		_page.Id = submission.Id;
		_page.Submission = new EditModel.SubmissionEdit
		{
			GameName = "Test Game",
			Status = SubmissionStatus.Rejected,
			Authors = ["TestAuthor"],
			RejectionReason = 1
		};

		_db.SubmissionRejectionReasons.Add(new SubmissionRejectionReason
		{
			Id = 1,
			DisplayName = "Test Rejection"
		});
		await _db.SaveChangesAsync();

		var judge = _db.AddUser("Judge").Entity;
		AddAuthenticatedUser(_page, judge, [PermissionTo.JudgeSubmissions]);

		_queueService.AvailableStatuses(
			Arg.Any<SubmissionStatus>(),
			Arg.Any<IEnumerable<PermissionTo>>(),
			Arg.Any<DateTime>(),
			Arg.Any<bool>(),
			Arg.Any<bool>(),
			Arg.Any<bool>())
			.Returns([SubmissionStatus.Rejected]);

		var actual = await _page.OnPost();

		Assert.IsInstanceOfType<RedirectToPageResult>(actual);
		Assert.AreEqual(SubmissionStatus.Rejected, _page.Submission.Status);
	}

	[TestMethod]
	public async Task OnGetClaimForJudging_WithoutPermission_ReturnsAccessDenied()
	{
		var user = _db.AddUser("User").Entity;
		AddAuthenticatedUser(_page, user, [PermissionTo.SubmitMovies]);

		var result = await _page.OnGetClaimForJudging();

		AssertAccessDenied(result);
	}

	[TestMethod]
	public async Task OnGetClaimForJudging_ValidSubmission_ClaimsAndRedirects()
	{
		var submission = _db.AddAndSaveUnpublishedSubmission().Entity;
		submission.Status = SubmissionStatus.New;
		submission.TopicId = 1;
		await _db.SaveChangesAsync();

		_page.Id = submission.Id;

		var judge = _db.AddUser("Judge").Entity;
		AddAuthenticatedUser(_page, judge, [PermissionTo.JudgeSubmissions]);

		var wikiPage = new WikiResult
		{
			PageName = $"Submission/{submission.Id}",
			Markup = "Original markup"
		};
		_wikiPages.Page($"InternalSystem/SubmissionContent/S{submission.Id}").Returns(wikiPage);
		_wikiPages.Add(Arg.Any<WikiCreateRequest>()).Returns(wikiPage);

		var actual = await _page.OnGetClaimForJudging();

		Assert.IsInstanceOfType<RedirectToPageResult>(actual);

		var updatedSubmission = await _db.Submissions.FindAsync(submission.Id);
		Assert.IsNotNull(updatedSubmission);
		Assert.AreEqual(SubmissionStatus.JudgingUnderWay, updatedSubmission.Status);
		Assert.AreEqual(judge.Id, updatedSubmission.JudgeId);

		await _topicWatcher.Received(1).WatchTopic(1, judge.Id, true);
	}

	[TestMethod]
	public async Task OnGetClaimForPublishing_WithoutPermission_ReturnsAccessDenied()
	{
		var user = _db.AddUser("User").Entity;
		AddAuthenticatedUser(_page, user, [PermissionTo.SubmitMovies]);

		var result = await _page.OnGetClaimForPublishing();

		AssertAccessDenied(result);
	}
}
