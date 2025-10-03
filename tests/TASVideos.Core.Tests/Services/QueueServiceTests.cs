using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using TASVideos.Core.Services.Wiki;
using TASVideos.Core.Services.Youtube;
using TASVideos.Core.Settings;
using TASVideos.Data.Entity;
using TASVideos.Data.Entity.Forum;
using TASVideos.Data.Entity.Game;
using TASVideos.Extensions;
using TASVideos.MovieParsers;
using TASVideos.MovieParsers.Result;
using static TASVideos.Data.Entity.SubmissionStatus;

namespace TASVideos.Core.Tests.Services;

[TestClass]
public class QueueServiceTests : TestDbBase
{
	private const int MinimumHoursBeforeJudgment = 72;
	private const int SubmissionRateDays = 1;
	private const int SubmissionRateSubs = 3;
	private readonly QueueService _queueService;
	private readonly IYoutubeSync _youtubeSync;
	private readonly ITASVideoAgent _tva;
	private readonly IWikiPages _wikiPages;
	private readonly ITASVideosGrue _tasvideosGrue;
	private readonly IMovieParser _movieParser;
	private readonly IFileService _fileService;

	private static DateTime TooNewToJudge => DateTime.UtcNow;

	private static DateTime OldEnoughToBeJudged
		=> DateTime.UtcNow.AddHours(-1 - MinimumHoursBeforeJudgment);

	private static readonly IEnumerable<PermissionTo> BasicUserPerms = [PermissionTo.SubmitMovies];
	private static readonly IEnumerable<PermissionTo> JudgePerms = [PermissionTo.SubmitMovies, PermissionTo.JudgeSubmissions];
	private static readonly IEnumerable<PermissionTo> PublisherPerms = [PermissionTo.SubmitMovies, PermissionTo.PublishMovies];
	private static readonly IEnumerable<PermissionTo> Override = [PermissionTo.OverrideSubmissionConstraints];

	public QueueServiceTests()
	{
		_youtubeSync = Substitute.For<IYoutubeSync>();
		_tva = Substitute.For<ITASVideoAgent>();
		_wikiPages = Substitute.For<IWikiPages>();
		var uploader = Substitute.For<IMediaFileUploader>();
		_fileService = Substitute.For<IFileService>();
		var userManager = Substitute.For<IUserManager>();
		_movieParser = Substitute.For<IMovieParser>();
		var deprecator = Substitute.For<IMovieFormatDeprecator>();
		var forumService = Substitute.For<IForumService>();
		_tasvideosGrue = Substitute.For<ITASVideosGrue>();
		var settings = new AppSettings
		{
			MinimumHoursBeforeJudgment = MinimumHoursBeforeJudgment,
			SubmissionRate = new() { Days = SubmissionRateDays, Submissions = SubmissionRateSubs }
		};
		var topicWatcher = Substitute.For<ITopicWatcher>();
		_queueService = new QueueService(settings, _db, _youtubeSync, _tva, _wikiPages, uploader, _fileService, userManager, _movieParser, deprecator, forumService, _tasvideosGrue, topicWatcher);
	}

	#region AvailableStatuses

	[TestMethod]
	public void AvailableStatuses_Published_CanNotChange()
	{
		var result = _queueService.AvailableStatuses(
			Published,
			Override,
			OldEnoughToBeJudged,
			true,
			true,
			true).ToList();

		Assert.IsNotNull(result);
		Assert.AreEqual(1, result.Count);
		Assert.AreEqual(Published, result.Single());
	}

	[DataRow(New, new[] { Cancelled })]
	[DataRow(Delayed, new[] { Cancelled })]
	[DataRow(NeedsMoreInfo, new[] { Cancelled })]
	[DataRow(JudgingUnderWay, new[] { Cancelled })]
	[DataRow(Accepted, new[] { Cancelled })]
	[DataRow(PublicationUnderway, new[] { Cancelled })]
	[DataRow(Rejected, new SubmissionStatus[0])]
	[DataRow(Cancelled, new[] { New })]
	[DataRow(Playground, new SubmissionStatus[0])]
	[TestMethod]
	public void AvailableStatuses_Submitter_BasicPerms(SubmissionStatus current, IEnumerable<SubmissionStatus> canChangeTo)
	{
		var expected = new[] { current }.Concat(canChangeTo).ToList();
		var result = _queueService.AvailableStatuses(
			current,
			BasicUserPerms,
			OldEnoughToBeJudged,
			isAuthorOrSubmitter: true,
			isJudge: false,
			isPublisher: false).ToList();

		Assert.IsNotNull(result);
		Assert.AreEqual(expected.Count, result.Count);
		foreach (var status in expected)
		{
			Assert.IsTrue(result.Contains(status));
		}
	}

	[DataRow(New, new[] { Cancelled })]
	[DataRow(Delayed, new[] { Cancelled })]
	[DataRow(NeedsMoreInfo, new[] { Cancelled })]
	[DataRow(JudgingUnderWay, new[] { Cancelled })]
	[DataRow(Accepted, new[] { Cancelled })]
	[DataRow(PublicationUnderway, new[] { Cancelled })]
	[DataRow(Rejected, new SubmissionStatus[0])]
	[DataRow(Cancelled, new[] { New })]
	[DataRow(Playground, new SubmissionStatus[0])]
	[TestMethod]
	public void AvailableStatuses_Submitter_IsJudge(SubmissionStatus current, IEnumerable<SubmissionStatus> canChangeTo)
	{
		var expected = new[] { current }.Concat(canChangeTo).ToList();
		var result = _queueService.AvailableStatuses(
			current,
			JudgePerms,
			OldEnoughToBeJudged,
			true,
			false,
			false).ToList();

		Assert.IsNotNull(result);
		Assert.AreEqual(expected.Count, result.Count);
		foreach (var status in expected)
		{
			Assert.IsTrue(result.Contains(status));
		}
	}

	[DataRow(New, new[] { Cancelled })]
	[DataRow(Delayed, new[] { Cancelled })]
	[DataRow(NeedsMoreInfo, new[] { Cancelled })]
	[DataRow(JudgingUnderWay, new[] { Cancelled })]
	[DataRow(Accepted, new[] { PublicationUnderway, Cancelled })]
	[DataRow(PublicationUnderway, new[] { Cancelled })]
	[DataRow(Rejected, new SubmissionStatus[0])]
	[DataRow(Cancelled, new[] { New })]
	[DataRow(Playground, new SubmissionStatus[0])]
	[TestMethod]
	public void AvailableStatuses_Submitter_IsPublisher(SubmissionStatus current, IEnumerable<SubmissionStatus> canChangeTo)
	{
		var expected = new[] { current }.Concat(canChangeTo).ToList();
		var result = _queueService.AvailableStatuses(
			current,
			PublisherPerms,
			OldEnoughToBeJudged,
			true,
			false,
			false).ToList();

		Assert.IsNotNull(result);
		Assert.AreEqual(expected.Count, result.Count);
		foreach (var status in expected)
		{
			Assert.IsTrue(result.Contains(status));
		}
	}

	[DataRow(New, new[] { JudgingUnderWay, Cancelled })]
	[DataRow(Delayed, new[] { New, JudgingUnderWay, Cancelled })]
	[DataRow(NeedsMoreInfo, new[] { New, JudgingUnderWay, Cancelled })]
	[DataRow(JudgingUnderWay, new[] { New, Cancelled })]
	[DataRow(Accepted, new[] { New, JudgingUnderWay, Cancelled })]
	[DataRow(PublicationUnderway, new[] { New, JudgingUnderWay, Cancelled })]
	[DataRow(Rejected, new[] { New, JudgingUnderWay })]
	[DataRow(Cancelled, new[] { New, JudgingUnderWay })]
	[DataRow(Playground, new[] { New, JudgingUnderWay })]
	[TestMethod]
	public void AvailableStatuses_Judge_ButNotSubmitter_BeforeAllowedJudgmentWindow(SubmissionStatus current, IEnumerable<SubmissionStatus> canChangeTo)
	{
		var expected = new[] { current }.Concat(canChangeTo).ToList();
		var result = _queueService.AvailableStatuses(
			current,
			JudgePerms,
			TooNewToJudge,
			isAuthorOrSubmitter: false,
			isJudge: true,
			isPublisher: false).ToList();

		Assert.IsNotNull(result);
		Assert.AreEqual(expected.Count, result.Count);
		foreach (var status in expected)
		{
			Assert.IsTrue(result.Contains(status));
		}
	}

	[DataRow(New, new[] { JudgingUnderWay, Cancelled })]
	[DataRow(Delayed, new[] { New, NeedsMoreInfo, JudgingUnderWay, Accepted, Rejected, Cancelled, Playground })]
	[DataRow(NeedsMoreInfo, new[] { New, Delayed, JudgingUnderWay, Accepted, Rejected, Cancelled, Playground })]
	[DataRow(JudgingUnderWay, new[] { New, Delayed, NeedsMoreInfo, Accepted, Rejected, Cancelled, Playground })]
	[DataRow(Accepted, new[] { New, Delayed, NeedsMoreInfo, JudgingUnderWay, Rejected, Cancelled })]
	[DataRow(PublicationUnderway, new[] { New, Delayed, NeedsMoreInfo, JudgingUnderWay, Accepted, Rejected, Cancelled })]
	[DataRow(Rejected, new[] { New, JudgingUnderWay })]
	[DataRow(Cancelled, new[] { New, JudgingUnderWay })]
	[DataRow(Playground, new[] { New, JudgingUnderWay })]
	[TestMethod]
	public void AvailableStatuses_Judge_ButNotSubmitter_AfterAllowedJudgmentWindow(SubmissionStatus current, IEnumerable<SubmissionStatus> canChangeTo)
	{
		var expected = new[] { current }.Concat(canChangeTo).ToList();
		var result = _queueService.AvailableStatuses(
			current,
			JudgePerms,
			OldEnoughToBeJudged,
			isAuthorOrSubmitter: false,
			isJudge: true,
			isPublisher: false).ToList();

		Assert.IsNotNull(result);
		Assert.AreEqual(expected.Count, result.Count);
		foreach (var status in expected)
		{
			Assert.IsTrue(result.Contains(status));
		}
	}

	[DataRow(New, new SubmissionStatus[0])]
	[DataRow(Delayed, new SubmissionStatus[0])]
	[DataRow(NeedsMoreInfo, new SubmissionStatus[0])]
	[DataRow(JudgingUnderWay, new SubmissionStatus[0])]
	[DataRow(Accepted, new[] { PublicationUnderway })]
	[DataRow(PublicationUnderway, new[] { Accepted })]
	[DataRow(Rejected, new SubmissionStatus[0])]
	[DataRow(Cancelled, new SubmissionStatus[0])]
	[DataRow(Playground, new SubmissionStatus[0])]
	[TestMethod]
	public void AvailableStatuses_Publisher_ButNotSubmitter_BeforeAllowedJudgmentWindow_CanNotChangeStatus(SubmissionStatus current, IEnumerable<SubmissionStatus> canChangeTo)
	{
		var expected = new[] { current }.Concat(canChangeTo).ToList();
		var result = _queueService.AvailableStatuses(
			current,
			PublisherPerms,
			TooNewToJudge,
			isAuthorOrSubmitter: false,
			isJudge: false,
			isPublisher: true).ToList();

		Assert.IsNotNull(result);
		Assert.AreEqual(expected.Count, result.Count);
		foreach (var status in expected)
		{
			Assert.IsTrue(result.Contains(status));
		}
	}

	[DataRow(New, new SubmissionStatus[0])]
	[DataRow(Delayed, new SubmissionStatus[0])]
	[DataRow(NeedsMoreInfo, new SubmissionStatus[0])]
	[DataRow(JudgingUnderWay, new SubmissionStatus[0])]
	[DataRow(Accepted, new[] { PublicationUnderway })]
	[DataRow(PublicationUnderway, new[] { Accepted })]
	[DataRow(Rejected, new SubmissionStatus[0])]
	[DataRow(Cancelled, new SubmissionStatus[0])]
	[DataRow(Playground, new SubmissionStatus[0])]
	[TestMethod]
	public void AvailableStatuses_Publisher_ButNotSubmitter_AfterAllowedJudgmentWindow(SubmissionStatus current, IEnumerable<SubmissionStatus> canChangeTo)
	{
		var expected = new[] { current }.Concat(canChangeTo).ToList();
		var result = _queueService.AvailableStatuses(
			current,
			PublisherPerms,
			OldEnoughToBeJudged,
			isAuthorOrSubmitter: false,
			isJudge: false,
			isPublisher: true).ToList();

		Assert.IsNotNull(result);
		Assert.AreEqual(expected.Count, result.Count);
		foreach (var status in expected)
		{
			Assert.IsTrue(result.Contains(status));
		}
	}

	[TestMethod]
	public void OverrideSubmissions_AnyStatusButPublished()
	{
		var exceptPublished = Enum.GetValues<SubmissionStatus>()
			.Except([Published])
			.OrderBy(s => s)
			.ToList();

		foreach (var current in exceptPublished)
		{
			var result = _queueService.AvailableStatuses(
				current,
				Override,
				TooNewToJudge,
				false,
				false,
				false).ToList();

			Assert.IsNotNull(result);
			Assert.AreEqual(exceptPublished.Count, result.Count);
			Assert.IsTrue(result.SequenceEqual(exceptPublished));
		}
	}

	#endregion

	#region HoursRemainingFor

	[TestMethod]
	[DataRow(Accepted)]
	[DataRow(PublicationUnderway)]
	[DataRow(Published)]
	[DataRow(Rejected)]
	[DataRow(Cancelled)]
	[DataRow(Playground)]
	public void HoursRemainingForJudging_StatusCannotBeJudged_ReturnsZero(SubmissionStatus status)
	{
		var submission = Substitute.For<ISubmissionDisplay>();
		submission.Status.Returns(status);
		var actual = _queueService.HoursRemainingForJudging(submission);
		Assert.AreEqual(0, actual);
	}

	[TestMethod]
	[DataRow(New)]
	[DataRow(Delayed)]
	[DataRow(NeedsMoreInfo)]
	[DataRow(JudgingUnderWay)]
	public void HoursRemainingForJudging_CanBeJudgedAndIsRecent_ReturnsPositiveHours(SubmissionStatus status)
	{
		var submission = Substitute.For<ISubmissionDisplay>();
		submission.Status.Returns(status);
		submission.Date.Returns(DateTime.UtcNow.AddHours(-(MinimumHoursBeforeJudgment - 1)));
		var actual = _queueService.HoursRemainingForJudging(submission);
		Assert.IsTrue(actual > 0);
	}

	[TestMethod]
	[DataRow(New)]
	[DataRow(Delayed)]
	[DataRow(NeedsMoreInfo)]
	[DataRow(JudgingUnderWay)]
	public void HoursRemainingForJudging_CanBeJudgedAndIsOld_ReturnsNegativeHours(SubmissionStatus status)
	{
		var submission = Substitute.For<ISubmissionDisplay>();
		submission.Status.Returns(status);
		submission.Date.Returns(DateTime.UtcNow.AddHours(-(MinimumHoursBeforeJudgment + 1)));
		var actual = _queueService.HoursRemainingForJudging(submission);
		Assert.IsTrue(actual < 0);
	}

	#endregion

	#region Delete Submission

	[TestMethod]
	public async Task CanDeleteSubmission_NotFound()
	{
		var result = await _queueService.CanDeleteSubmission(int.MaxValue);
		Assert.IsNotNull(result);
		Assert.AreEqual(DeleteSubmissionResult.DeleteStatus.NotFound, result.Status);
		Assert.IsTrue(string.IsNullOrWhiteSpace(result.ErrorMessage));
		Assert.IsTrue(string.IsNullOrWhiteSpace(result.SubmissionTitle));
	}

	[TestMethod]
	public async Task CanDeleteSubmission_CannotDeleteIfPublished()
	{
		var user = _db.AddUser(0).Entity;
		var pub = _db.AddPublication().Entity;
		pub.Submission!.Publisher = user;
		await _db.SaveChangesAsync();

		var result = await _queueService.CanDeleteSubmission(pub.SubmissionId);
		Assert.IsNotNull(result);
		Assert.AreEqual(DeleteSubmissionResult.DeleteStatus.NotAllowed, result.Status);
		Assert.IsFalse(string.IsNullOrWhiteSpace(result.ErrorMessage));
		Assert.IsTrue(string.IsNullOrWhiteSpace(result.SubmissionTitle));
	}

	[TestMethod]
	public async Task CanDeleteSubmission_Success()
	{
		var sub = _db.AddSubmission().Entity;
		const string submissionTitle = "Test Submission";
		sub.Title = submissionTitle;
		await _db.SaveChangesAsync();

		var result = await _queueService.CanDeleteSubmission(sub.Id);
		Assert.IsNotNull(result);
		Assert.AreEqual(DeleteSubmissionResult.DeleteStatus.Success, result.Status);
		Assert.IsTrue(string.IsNullOrWhiteSpace(result.ErrorMessage));
		Assert.AreEqual(submissionTitle, result.SubmissionTitle);
	}

	[TestMethod]
	public async Task DeleteSubmission_NotFound()
	{
		var result = await _queueService.DeleteSubmission(int.MaxValue);
		Assert.IsNotNull(result);
		Assert.AreEqual(DeleteSubmissionResult.DeleteStatus.NotFound, result.Status);
		Assert.IsTrue(string.IsNullOrWhiteSpace(result.ErrorMessage));
		Assert.IsTrue(string.IsNullOrWhiteSpace(result.SubmissionTitle));
	}

	[TestMethod]
	public async Task DeleteSubmission_CannotDeleteIfPublished()
	{
		var user = _db.AddUser(0).Entity;
		var pub = _db.AddPublication().Entity;
		pub.Submission!.Publisher = user;
		await _db.SaveChangesAsync();

		var result = await _queueService.DeleteSubmission(pub.SubmissionId);
		Assert.IsNotNull(result);
		Assert.AreEqual(DeleteSubmissionResult.DeleteStatus.NotAllowed, result.Status);
		Assert.IsFalse(string.IsNullOrWhiteSpace(result.ErrorMessage));
		Assert.IsTrue(string.IsNullOrWhiteSpace(result.SubmissionTitle));
	}

	[TestMethod]
	public async Task DeleteSubmission_Success()
	{
		const int submissionId = 1;
		const int pollId = 3;
		const string submissionTitle = "Test Submission";
		var user = _db.AddUser(0).Entity;
		var topic = _db.AddTopic().Entity;
		await _db.SaveChangesAsync();
		var poll = _db.ForumPolls.Add(new ForumPoll { Id = pollId, TopicId = topic.Id }).Entity;
		var pollOptions = new ForumPollOption[]
		{
			new() { PollId = pollId, Votes = [new() { User = user }] },
			new() { PollId = pollId }
		};
		_db.ForumPollOptions.AddRange(pollOptions);
		poll.PollOptions.AddRange(pollOptions);
		topic.Poll = poll;
		_db.Submissions.Add(new Submission
		{
			Id = submissionId,
			Status = New,
			Title = submissionTitle,
			TopicId = topic.Id,
			Topic = topic,
			Submitter = user
		});
		_db.SubmissionStatusHistory.Add(new SubmissionStatusHistory { SubmissionId = submissionId, Status = New });
		_db.SubmissionAuthors.Add(new SubmissionAuthor { SubmissionId = submissionId, UserId = user.Id });
		_db.ForumPosts.Add(new ForumPost { Topic = topic, TopicId = topic.Id, Text = "1", ForumId = topic.ForumId, Poster = user });
		_db.ForumPosts.Add(new ForumPost { Topic = topic, TopicId = topic.Id, Text = "2", ForumId = topic.ForumId, Poster = user });
		await _db.SaveChangesAsync();

		var result = await _queueService.DeleteSubmission(submissionId);
		Assert.IsNotNull(result);
		Assert.AreEqual(DeleteSubmissionResult.DeleteStatus.Success, result.Status);
		Assert.IsTrue(string.IsNullOrWhiteSpace(result.ErrorMessage));
		Assert.AreEqual(submissionTitle, result.SubmissionTitle);
		Assert.AreEqual(0, _db.Submissions.Count());
		Assert.AreEqual(0, _db.SubmissionStatusHistory.Count());
		Assert.AreEqual(0, _db.SubmissionAuthors.Count());
		Assert.AreEqual(0, _db.ForumTopics.Count());
		Assert.AreEqual(0, _db.ForumPolls.Count());
		Assert.AreEqual(0, _db.ForumPosts.Count());
		Assert.AreEqual(0, _db.ForumPollOptions.Count());
		Assert.AreEqual(0, _db.ForumPollOptionVotes.Count());
		await _wikiPages.Received(1).Delete(WikiHelper.ToSubmissionWikiPageName(1));
	}

	#endregion

	#region MapParsedResult

	[TestMethod]
	public async Task MapParsedResult_ThrowsIfParsingIsFailed()
	{
		await Assert.ThrowsExactlyAsync<InvalidOperationException>(() => _queueService.MapParsedResult(new TestParseResult { Success = false }));
	}

	[TestMethod]
	public async Task MapParsedResult_ErrorIfUnknownSystem()
	{
		_db.GameSystems.Add(new GameSystem { Code = "NES" });
		await _db.SaveChangesAsync();

		var actual = await _queueService.MapParsedResult(new TestParseResult { Success = true, SystemCode = "Does not exist" });

		Assert.IsNull(actual);
	}

	[TestMethod]
	public async Task MapParsedResult_NoOverride_Success()
	{
		const string system = "NES";
		const double frameRate = 60.0;
		const RegionType region = RegionType.Ntsc;
		const MovieStartType startType = MovieStartType.Savestate;
		const int frames = 42069;
		const int rerecordCount = 420;
		const string fileExtension = ".test";
		var entry = _db.GameSystems.Add(new GameSystem { Code = system });
		_db.GameSystemFrameRates.Add(new GameSystemFrameRate
		{
			GameSystemId = entry.Entity.Id,
			FrameRate = frameRate,
			RegionCode = region.ToString().ToUpper()
		});
		await _db.SaveChangesAsync();

		var parseResult = new TestParseResult
		{
			Success = true,
			SystemCode = system,
			FrameRateOverride = null,
			Region = region,
			StartType = startType,
			Frames = frames,
			RerecordCount = rerecordCount,
			FileExtension = fileExtension
		};

		var actual = await _queueService.MapParsedResult(parseResult);
		Assert.IsNotNull(actual);
		Assert.IsNotNull(actual.SystemFrameRate);
		Assert.AreEqual(frameRate, actual.SystemFrameRate.FrameRate);
		Assert.AreEqual(region.ToString().ToUpper(), actual.SystemFrameRate.RegionCode);
		Assert.AreEqual((int)startType, actual.MovieStartType);
		Assert.AreEqual(frames, actual.Frames);
		Assert.AreEqual(rerecordCount, actual.RerecordCount);
		Assert.AreEqual(fileExtension, actual.MovieExtension);
		Assert.AreEqual(system, actual.System.Code);
	}

	[TestMethod]
	public async Task MapParsedResult_WithOverride_Success()
	{
		const string system = "NES";
		const double frameRateOverride = 61.0;
		const RegionType region = RegionType.Ntsc;
		var entry = _db.GameSystems.Add(new GameSystem { Code = system });
		await _db.SaveChangesAsync();
		var parseResult = new TestParseResult
		{
			Success = true,
			SystemCode = system,
			FrameRateOverride = frameRateOverride,
			Region = region
		};

		var actual = await _queueService.MapParsedResult(parseResult);
		Assert.IsNotNull(actual);
		Assert.IsNotNull(actual.SystemFrameRate);
		Assert.AreEqual(frameRateOverride, actual.SystemFrameRate.FrameRate);
		Assert.AreEqual(region.ToString().ToUpper(), actual.SystemFrameRate.RegionCode);
		Assert.AreEqual(entry.Entity, actual.SystemFrameRate.System);
	}

	#endregion

	#region ObsoleteWith

	[TestMethod]
	public async Task ObsoleteWith_NoPublication_ReturnsFalse()
	{
		var actual = await _queueService.ObsoleteWith(int.MaxValue, int.MaxValue);
		Assert.IsFalse(actual);
	}

	[TestMethod]
	public async Task ObsoleteWith_Success()
	{
		_youtubeSync.IsYoutubeUrl(Arg.Any<string>()).Returns(true);
		const string youtubeUrl = "https://www.youtube.com/watch?v=dQw4w9WgXcQ";
		const string wikiMarkup = "Test";
		var pubToObsolete = _db.AddPublication().Entity;
		pubToObsolete.PublicationUrls.Add(new() { Type = PublicationUrlType.Streaming, Url = youtubeUrl });
		var obsoletingPub = _db.AddPublication().Entity;
		await _db.SaveChangesAsync();
		_db.WikiPages.Add(new WikiPage
		{
			PageName = WikiHelper.ToPublicationWikiPageName(pubToObsolete.Id),
			Markup = wikiMarkup
		});
		await _db.SaveChangesAsync();

		var actual = await _queueService.ObsoleteWith(pubToObsolete.Id, obsoletingPub.Id);

		Assert.IsTrue(actual);
		Assert.AreEqual(1, _db.Publications.Count(p => p.Id == pubToObsolete.Id));
		var actualPub = _db.Publications.Single(p => p.Id == pubToObsolete.Id);
		Assert.AreEqual(obsoletingPub.Id, actualPub.ObsoletedById);

		await _youtubeSync.Received(1).SyncYouTubeVideo(Arg.Any<YoutubeVideo>());
	}

	#endregion

	#region ExceededSubmissionLimit

	[TestMethod]
	public async Task ExceededSubmissionLimit_RecentSubmissionsButUnderLimit_ReturnsNull()
	{
		const int submitterId = 1;
		_db.AddUser(submitterId);
		_db.Submissions.Add(new Submission { SubmitterId = submitterId });
		await _db.SaveChangesAsync();

		var actual = await _queueService.ExceededSubmissionLimit(submitterId);
		Assert.IsNull(actual);
	}

	[TestMethod]
	public async Task ExceededSubmissionLimit_ManySubmissionsButOldEnough_ReturnsNull()
	{
		const int submitterId = 1;
		_db.AddUser(submitterId);
		for (var i = 0; i < SubmissionRateSubs + 1; i++)
		{
			_db.Submissions.Add(new Submission { SubmitterId = submitterId, CreateTimestamp = DateTime.UtcNow.AddDays(-SubmissionRateDays).AddHours(-1) });
		}

		await _db.SaveChangesAsync();

		var actual = await _queueService.ExceededSubmissionLimit(submitterId);
		Assert.IsNull(actual);
	}

	[TestMethod]
	public async Task ExceededSubmissionLimit_ManyRecentSubmissions_ReturnsFutureDate()
	{
		const int submitterId = 1;
		_db.AddUser(submitterId);
		for (var i = 0; i < SubmissionRateSubs + 1; i++)
		{
			_db.Submissions.Add(new Submission { SubmitterId = submitterId, CreateTimestamp = DateTime.UtcNow.AddDays(-SubmissionRateDays).AddHours(1 + i) });
		}

		await _db.SaveChangesAsync();

		var actual = await _queueService.ExceededSubmissionLimit(submitterId);
		Assert.IsNotNull(actual);
		Assert.IsTrue(actual.Value > DateTime.UtcNow);
	}

	#endregion

	#region GetSubmissionCount

	[TestMethod]
	public async Task GetSubmissionCount_UserDoesNotExist_ReturnsZero()
	{
		var actual = await _queueService.GetSubmissionCount(int.MaxValue);
		Assert.AreEqual(0, actual);
	}

	[TestMethod]
	public async Task GetSubmissionCount_UserHasNotSubmitted_ReturnsZero()
	{
		const int userId = 1;
		_db.AddUser(userId);

		var actual = await _queueService.GetSubmissionCount(userId);
		Assert.AreEqual(0, actual);
	}

	[TestMethod]
	public async Task GetSubmissionCount_UserIsAuthorButNotSubmitter_ReturnsZero()
	{
		const int authorId = 1;
		_db.AddUser(authorId);
		var sub = _db.AddSubmission();
		_db.SubmissionAuthors.Add(new SubmissionAuthor { UserId = authorId, Submission = sub.Entity });
		await _db.SaveChangesAsync();

		var actual = await _queueService.GetSubmissionCount(authorId);
		Assert.AreEqual(0, actual);
	}

	[TestMethod]
	public async Task GetSubmissionCount_ReturnsSubmissionCount()
	{
		var sub = _db.AddSubmission();
		await _db.SaveChangesAsync();

		var actual = await _queueService.GetSubmissionCount(sub.Entity.Submitter!.Id);
		Assert.AreEqual(1, actual);
	}

	#endregion

	#region Submit

	[TestMethod]
	public async Task Submit_MapParsedResultFails_ReturnsFailedResult()
	{
		var user = _db.AddUser(0).Entity;
		var request = CreateValidSubmitRequest(user, "INVALID_SYSTEM");

		var result = await _queueService.Submit(request);

		Assert.IsNotNull(result);
		Assert.IsFalse(result.Success);
		Assert.IsNotNull(result.ErrorMessage);
		Assert.IsTrue(result.ErrorMessage.Contains("INVALID_SYSTEM"));
	}

	[TestMethod]
	public async Task Submit_Success_CreatesSubmissionAndWikiPage()
	{
		_db.AddForumConstantEntities();
		var user = _db.AddUser(0).Entity;
		var gameSystem = _db.GameSystems.Add(new GameSystem { Code = "NES" }).Entity;
		_db.GameSystemFrameRates.Add(new()
		{
			GameSystemId = gameSystem.Id,
			FrameRate = 60.0,
			RegionCode = "NTSC"
		});
		await _db.SaveChangesAsync();

		const int expectedTopicId = 12345;
		_tva.PostSubmissionTopic(Arg.Any<int>(), Arg.Any<string>()).Returns(expectedTopicId);
		_youtubeSync.IsYoutubeUrl(Arg.Any<string>()).Returns(false);

		var request = CreateValidSubmitRequest(user, "NES");

		var result = await _queueService.Submit(request);

		Assert.IsNotNull(result);
		Assert.IsTrue(result.Success);
		Assert.IsNull(result.ErrorMessage);
		Assert.IsTrue(result.Id > 0);
		Assert.IsFalse(string.IsNullOrWhiteSpace(result.Title));

		var actualSub = await _db.Submissions.FindAsync(result.Id);
		Assert.IsNotNull(actualSub);
		Assert.AreEqual(user.Id, actualSub.SubmitterId);
		Assert.AreEqual(request.GameName, actualSub.GameName);
		Assert.AreEqual(request.RomName, actualSub.RomName);
		Assert.AreEqual(request.GoalName, actualSub.Branch);
		Assert.AreEqual(expectedTopicId, actualSub.TopicId);
		Assert.AreEqual(gameSystem.Id, actualSub.SystemId);
		Assert.IsTrue(actualSub.Title.Contains(request.GameName));

		var actualSubAuthors = await _db.SubmissionAuthors
			.Where(sa => sa.SubmissionId == result.Id)
			.ToListAsync();
		Assert.AreEqual(request.Authors.Count, actualSubAuthors.Count);

		await _wikiPages.Received(1).Add(Arg.Is<WikiCreateRequest>(r =>
			r.PageName == LinkConstants.SubmissionWikiPage + result.Id &&
			r.Markup == request.Markup &&
			r.AuthorId == user.Id));

		await _tva.Received(1).PostSubmissionTopic(result.Id, result.Title);
	}

	[TestMethod]
	public async Task Submit_WithYouTubeLink_DownloadsScreenshot()
	{
		var user = _db.AddUser(0).Entity;
		var gameSystem = _db.GameSystems.Add(new GameSystem { Code = "NES" }).Entity;
		_db.GameSystemFrameRates.Add(new()
		{
			GameSystemId = gameSystem.Id,
			FrameRate = 60.0,
			RegionCode = "NTSC"
		});
		await _db.SaveChangesAsync();

		_youtubeSync.ConvertToEmbedLink("https://www.youtube.com/watch?v=dQw4w9WgXcQ")
			.Returns("https://www.youtube.com/embed/dQw4w9WgXcQ");
		_youtubeSync.IsYoutubeUrl("https://www.youtube.com/embed/dQw4w9WgXcQ").Returns(true);
		_tva.PostSubmissionTopic(Arg.Any<int>(), Arg.Any<string>()).Returns(12345);

		var request = CreateValidSubmitRequest(user, "NES")
			with
		{ EncodeEmbeddedLink = "https://www.youtube.com/watch?v=dQw4w9WgXcQ" };

		var result = await _queueService.Submit(request);

		Assert.IsNotNull(result);
		Assert.IsTrue(result.Success);
		Assert.IsNull(result.ErrorMessage);
		Assert.IsNotNull(result.Screenshot);

		var submission = await _db.Submissions.FindAsync(result.Id);
		Assert.IsNotNull(submission);
		Assert.AreEqual("https://www.youtube.com/embed/dQw4w9WgXcQ", submission.EncodeEmbedLink);
	}

	[TestMethod]
	public async Task Submit_WithHashInformation_SetsHashCorrectly()
	{
		var user = _db.AddUser(0).Entity;
		var gameSystem = _db.GameSystems.Add(new GameSystem { Code = "NES" }).Entity;
		_db.GameSystemFrameRates.Add(new()
		{
			GameSystemId = gameSystem.Id,
			FrameRate = 60.0,
			RegionCode = "NTSC"
		});
		await _db.SaveChangesAsync();

		var parseResult = new TestParseResult
		{
			Success = true,
			SystemCode = "NES",
			Region = RegionType.Ntsc,
			Hashes = new Dictionary<HashType, string>
			{
				[HashType.Sha1] = "abc123def456"
			}
		};

		var request = CreateValidSubmitRequest(user, "NES", parseResult);

		var result = await _queueService.Submit(request);

		Assert.IsNotNull(result);
		Assert.IsTrue(result.Success);

		var actualSub = await _db.Submissions.FindAsync(result.Id);
		Assert.IsNotNull(actualSub);
		Assert.AreEqual("Sha1", actualSub.HashType);
		Assert.AreEqual("abc123def456", actualSub.Hash);
	}

	[TestMethod]
	public async Task Submit_WithFrameRateOverride_CreatesNewFrameRate()
	{
		_db.AddForumConstantEntities();
		var user = _db.AddUser(0).Entity;
		var gameSystem = _db.GameSystems.Add(new GameSystem { Code = "NES" }).Entity;
		await _db.SaveChangesAsync();

		const double customFrameRate = 59.85;
		var parseResult = new TestParseResult
		{
			Success = true,
			SystemCode = "NES",
			Region = RegionType.Ntsc,
			FrameRateOverride = customFrameRate
		};

		var request = CreateValidSubmitRequest(user, "NES", parseResult);

		var result = await _queueService.Submit(request);

		Assert.IsNotNull(result);
		Assert.IsTrue(result.Success);
		Assert.IsNull(result.ErrorMessage);

		var actualSub = await _db.Submissions
			.Include(s => s.SystemFrameRate)
			.SingleOrDefaultAsync(s => s.Id == result.Id);
		Assert.IsNotNull(actualSub);
		Assert.IsNotNull(actualSub.SystemFrameRate);
		Assert.AreEqual(customFrameRate, actualSub.SystemFrameRate.FrameRate);
		Assert.AreEqual("NTSC", actualSub.SystemFrameRate.RegionCode);
		Assert.AreEqual(gameSystem.Id, actualSub.SystemFrameRate.GameSystemId);
	}

	[TestMethod]
	public async Task Submit_WithAnnotationsAndWarnings_SetsCorrectly()
	{
		var user = _db.AddUser(0).Entity;
		var gameSystem = _db.GameSystems.Add(new GameSystem { Code = "NES" }).Entity;
		_db.GameSystemFrameRates.Add(new()
		{
			GameSystemId = gameSystem.Id,
			FrameRate = 60.0,
			RegionCode = "NTSC"
		});
		await _db.SaveChangesAsync();

		const string annotations = "Test annotations with important information";
		var parseResult = new SubmitTestParseResult
		{
			Success = true,
			SystemCode = "NES",
			Region = RegionType.Ntsc,
			Annotations = annotations,
			WarningsList = [ParseWarnings.MissingRerecordCount, ParseWarnings.SystemIdInferred]
		};
		var request = CreateValidSubmitRequest(user, "NES", parseResult);

		var result = await _queueService.Submit(request);

		Assert.IsNotNull(result);
		Assert.IsTrue(result.Success);
		Assert.IsNull(result.ErrorMessage);

		var actualSub = await _db.Submissions.FindAsync(result.Id);
		Assert.IsNotNull(actualSub);
		Assert.AreEqual(annotations, actualSub.Annotations);
		Assert.IsFalse(string.IsNullOrWhiteSpace(actualSub.Warnings));
		Assert.IsTrue(actualSub.Warnings.Contains("MissingRerecordCount"));
		Assert.IsTrue(actualSub.Warnings.Contains("SystemIdInferred"));
	}

	[TestMethod]
	public async Task Submit_DatabaseTransactionRollsBackOnException()
	{
		var user = _db.AddUser(0).Entity;
		var gameSystem = _db.GameSystems.Add(new GameSystem { Code = "NES" }).Entity;
		_db.GameSystemFrameRates.Add(new()
		{
			GameSystemId = gameSystem.Id,
			FrameRate = 60.0,
			RegionCode = "NTSC"
		});
		await _db.SaveChangesAsync();

		_youtubeSync.IsYoutubeUrl(Arg.Any<string>()).Returns(false);
		_wikiPages.When(w => w.Add(Arg.Any<WikiCreateRequest>()))
			.Do(_ => throw new Exception("Wiki creation failed"));

		var request = CreateValidSubmitRequest(user, "NES");

		var result = await _queueService.Submit(request);

		Assert.IsNotNull(result);
		Assert.IsFalse(result.Success);
		Assert.IsFalse(string.IsNullOrWhiteSpace(result.ErrorMessage));

		// We cannot currently test rollback since the rollback is fake for tests
		////var submitterSubs = await _db.Submissions.Where(s => s.SubmitterId == user.Id).ToListAsync();
		////Assert.AreEqual(0, submitterSubs.Count, "Expected no submissions to be created due to rollback");
	}

	[TestMethod]
	public async Task Submit_WithExternalAuthors_NormalizesExternalAuthors()
	{
		var user = _db.AddUser(0).Entity;
		var gameSystem = _db.GameSystems.Add(new GameSystem { Code = "NES" }).Entity;
		_db.GameSystemFrameRates.Add(new()
		{
			GameSystemId = gameSystem.Id,
			FrameRate = 60.0,
			RegionCode = "NTSC"
		});
		await _db.SaveChangesAsync();
		var request = CreateValidSubmitRequest(user, "NES")
			with
		{ ExternalAuthors = "External Author 1, External Author 2" };

		var result = await _queueService.Submit(request);

		Assert.IsNotNull(result);
		Assert.IsTrue(result.Success, $"Expected success but got failure. Error: {result.ErrorMessage}");

		var actualSub = await _db.Submissions.FindAsync(result.Id);
		Assert.IsNotNull(actualSub);

		// Saved additional authors should have no spaces around commas
		Assert.AreEqual("External Author 1,External Author 2", actualSub.AdditionalAuthors);
	}

	private static SubmitRequest CreateValidSubmitRequest(User submitter, string systemCode, IParseResult? customParseResult = null)
	{
		var parseResult = customParseResult ?? new TestParseResult
		{
			Success = true,
			SystemCode = systemCode,
			Region = RegionType.Ntsc,
			StartType = MovieStartType.PowerOn,
			Frames = 1000,
			RerecordCount = 50,
			FileExtension = ".fm2"
		};

		return new SubmitRequest(
			GameName: "Test Game",
			RomName: "Test ROM",
			GameVersion: "1.0",
			GoalName: "any%",
			Emulator: "FCEUX 2.6.4",
			EncodeEmbeddedLink: null,
			Authors: [submitter.UserName],
			ExternalAuthors: null,
			Markup: "Test submission markup content",
			MovieFile: "MOVIE_FILE_CONTENT"u8.ToArray(),
			ParseResult: parseResult,
			Submitter: submitter);
	}

	private class SubmitTestParseResult : IParseResult
	{
		public bool Success { get; init; }
		public IEnumerable<string> Errors { get; } = [];
		public IEnumerable<ParseWarnings> Warnings => WarningsList;
		public List<ParseWarnings> WarningsList { get; init; } = [];
		public string FileExtension => "";
		public RegionType Region { get; init; }
		public int Frames => 0;
		public string SystemCode { get; init; } = "";
		public int RerecordCount => 0;
		public MovieStartType StartType => MovieStartType.PowerOn;
		public double? FrameRateOverride => null;
		public long? CycleCount => null;
		public string? Annotations { get; init; }
		public Dictionary<HashType, string> Hashes { get; } = [];
	}

	#endregion

	#region GetObsoletePublicationTags

	[TestMethod]
	public async Task GetObsoletePublicationTags_PublicationNotFound_ReturnsNull()
	{
		var result = await _queueService.GetObsoletePublicationTags(int.MaxValue);
		Assert.IsNull(result);
	}

	[TestMethod]
	public async Task GetObsoletePublicationTags_PublicationExists_ReturnsCorrectData()
	{
		var tag1 = _db.Tags.Add(new Tag { Code = "Test1" }).Entity;
		var tag2 = _db.Tags.Add(new Tag { Code = "Test2" }).Entity;
		var pub = _db.AddPublication().Entity;
		const string publicationTitle = "Test Publication Title";
		pub.Title = publicationTitle;
		_db.PublicationTags.Add(new PublicationTag { Publication = pub, Tag = tag1 });
		_db.PublicationTags.Add(new PublicationTag { Publication = pub, Tag = tag2 });
		await _db.SaveChangesAsync();

		const string wikiMarkup = "Test wiki page content";
		var expectedPageName = WikiHelper.ToPublicationWikiPageName(pub.Id);
		var wikiResult = new WikiResult { Markup = wikiMarkup };
		_wikiPages.Page(expectedPageName).Returns(wikiResult);

		var result = await _queueService.GetObsoletePublicationTags(pub.Id);

		Assert.IsNotNull(result);
		Assert.AreEqual(publicationTitle, result.Title);
		Assert.AreEqual(2, result.Tags.Count);
		Assert.IsTrue(result.Tags.Contains(tag1.Id));
		Assert.IsTrue(result.Tags.Contains(tag2.Id));
		Assert.AreEqual(wikiMarkup, result.Markup);
		await _wikiPages.Received(1).Page(expectedPageName);
	}

	[TestMethod]
	public async Task GetObsoletePublicationTags_PublicationWithNoTags_ReturnsEmptyTagsList()
	{
		var pub = _db.AddPublication().Entity;
		const string publicationTitle = "Test Publication Without Tags";
		pub.Title = publicationTitle;
		await _db.SaveChangesAsync();

		const string wikiMarkup = "Wiki content for publication without tags";
		var expectedPageName = WikiHelper.ToPublicationWikiPageName(pub.Id);
		var wikiResult = new WikiResult { Markup = wikiMarkup };
		_wikiPages.Page(expectedPageName).Returns(wikiResult);

		var result = await _queueService.GetObsoletePublicationTags(pub.Id);

		Assert.IsNotNull(result);
		Assert.AreEqual(publicationTitle, result.Title);
		Assert.AreEqual(0, result.Tags.Count);
		Assert.AreEqual(wikiMarkup, result.Markup);
		await _wikiPages.Received(1).Page(expectedPageName);
	}

	#endregion

	#region UpdateSubmission

	[TestMethod]
	public async Task UpdateSubmission_NonExistentSubmission_ReturnsError()
	{
		var request = CreateValidUpdateSubmissionRequest(999);

		var result = await _queueService.UpdateSubmission(request);

		Assert.IsFalse(result.Success);
		Assert.AreEqual("Submission not found", result.ErrorMessage);
	}

	[TestMethod]
	public async Task UpdateSubmission_ValidUpdate_UpdatesSubmissionAndReturnsSuccess()
	{
		var submission = _db.AddAndSaveUnpublishedSubmission().Entity;

		var request = CreateValidUpdateSubmissionRequest(
			submission.Id,
			gameName: "Updated Game Name",
			goal: "Updated Goal",
			emulator: "Updated Emulator");

		_wikiPages.Add(Arg.Any<WikiCreateRequest>()).Returns(new WikiResult { Markup = "Updated wiki content" });

		var result = await _queueService.UpdateSubmission(request);

		Assert.IsTrue(result.Success);
		Assert.AreEqual(New, result.PreviousStatus);
		Assert.IsTrue(result.SubmissionTitle.Contains("Updated Game Name"));

		var updatedSubmission = await _db.Submissions.FindAsync(submission.Id);
		Assert.IsNotNull(updatedSubmission);
		Assert.AreEqual("Updated Game Name", updatedSubmission.GameName);
		Assert.AreEqual("Updated Emulator", updatedSubmission.EmulatorVersion);
	}

	[TestMethod]
	public async Task UpdateSubmission_WithStatusChangeFromNewToPlayground_ReturnsSuccess()
	{
		var submission = _db.AddAndSaveUnpublishedSubmission().Entity;
		submission.Status = New;
		await _db.SaveChangesAsync();

		var request = CreateValidUpdateSubmissionRequest(
			submission.Id,
			status: Playground);

		_wikiPages.Add(Arg.Any<WikiCreateRequest>()).Returns(new WikiResult { Markup = "Wiki content" });

		var result = await _queueService.UpdateSubmission(request);

		Assert.IsTrue(result.Success);
		Assert.AreEqual(New, result.PreviousStatus);
		var updatedSub = await _db.Submissions.FindAsync(submission.Id);
		Assert.IsNotNull(updatedSub);
		Assert.AreEqual(Playground, updatedSub.Status);
	}

	[TestMethod]
	public async Task UpdateSubmission_WithAuthorChanges_UpdatesSubmissionAuthors()
	{
		var submission = _db.AddAndSaveUnpublishedSubmission().Entity;
		_db.AddUser("Author1");
		_db.AddUser("Author2");
		await _db.SaveChangesAsync();

		var request = CreateValidUpdateSubmissionRequest(
			submission.Id,
			authors: ["Author1", "Author2"]);

		_wikiPages.Add(Arg.Any<WikiCreateRequest>()).Returns(new WikiResult { Markup = "Wiki content" });

		var result = await _queueService.UpdateSubmission(request);

		Assert.IsTrue(result.Success);

		var updatedSub = await _db.Submissions
			.Include(s => s.SubmissionAuthors)
			.ThenInclude(sa => sa.Author)
			.SingleOrDefaultAsync(s => s.Id == submission.Id);

		Assert.IsNotNull(updatedSub);
		Assert.AreEqual(2, updatedSub.SubmissionAuthors.Count);
		var authorNames = updatedSub.SubmissionAuthors
			.OrderBy(sa => sa.Ordinal)
			.Select(sa => sa.Author!.UserName)
			.ToList();
		Assert.AreEqual("Author1", authorNames[0]);
		Assert.AreEqual("Author2", authorNames[1]);
	}

	[TestMethod]
	public async Task UpdateSubmission_WithMarkupChanges_CallsWikiPagesAdd()
	{
		var submission = _db.AddAndSaveUnpublishedSubmission().Entity;
		const string newMarkup = "Updated wiki markup content";
		const string revisionMessage = "Test revision";

		var request = CreateValidUpdateSubmissionRequest(
			submission.Id,
			markupChanged: true,
			markup: newMarkup,
			revisionMessage: revisionMessage,
			minorEdit: true);

		_wikiPages.Add(Arg.Any<WikiCreateRequest>()).Returns(new WikiResult { Markup = newMarkup });

		var result = await _queueService.UpdateSubmission(request);

		Assert.IsTrue(result.Success);

		await _wikiPages.Received(1).Add(Arg.Is<WikiCreateRequest>(req =>
			req.PageName.EndsWith($"S{submission.Id}") &&
			req.Markup == newMarkup &&
			req.RevisionMessage == revisionMessage &&
			req.MinorEdit == true));
	}

	[TestMethod]
	public async Task UpdateSubmission_WithoutMarkupChanges_DoesNotCallWikiPagesAdd()
	{
		var submission = _db.AddAndSaveUnpublishedSubmission().Entity;

		var request = CreateValidUpdateSubmissionRequest(
			submission.Id,
			markupChanged: false);

		var result = await _queueService.UpdateSubmission(request);

		Assert.IsTrue(result.Success);

		await _wikiPages.DidNotReceive().Add(Arg.Any<WikiCreateRequest>());
	}

	[TestMethod]
	public async Task UpdateSubmission_StatusChangeToRejected_CallsGrueRejectAndMove()
	{
		var submission = _db.AddAndSaveUnpublishedSubmission().Entity;
		submission.Status = New;
		await _db.SaveChangesAsync();

		var request = CreateValidUpdateSubmissionRequest(
			submission.Id,
			status: Rejected);

		_wikiPages.Add(Arg.Any<WikiCreateRequest>()).Returns(new WikiResult { Markup = "Wiki content" });

		var result = await _queueService.UpdateSubmission(request);

		Assert.IsTrue(result.Success);
		Assert.AreEqual(New, result.PreviousStatus);

		await _tasvideosGrue.Received(1).RejectAndMove(submission.Id);
	}

	[TestMethod]
	public async Task UpdateSubmission_StatusChangeToCancelled_CallsGrueRejectAndMove()
	{
		var submission = _db.AddAndSaveUnpublishedSubmission().Entity;
		submission.Status = New;
		await _db.SaveChangesAsync();

		var request = CreateValidUpdateSubmissionRequest(
			submission.Id,
			status: Cancelled);

		_wikiPages.Add(Arg.Any<WikiCreateRequest>()).Returns(new WikiResult { Markup = "Wiki content" });

		var result = await _queueService.UpdateSubmission(request);

		Assert.IsTrue(result.Success);
		Assert.AreEqual(New, result.PreviousStatus);

		await _tasvideosGrue.Received(1).RejectAndMove(submission.Id);
	}

	[TestMethod]
	public async Task UpdateSubmission_WithMovieFileParsingFailure_ReturnsError()
	{
		var submission = _db.AddAndSaveUnpublishedSubmission().Entity;
		var movieFile = CreateMockMovieFile("test.bk2", "invalid movie data");

		var request = CreateValidUpdateSubmissionRequest(
			submission.Id,
			replaceMovieFile: movieFile);

		var result = await _queueService.UpdateSubmission(request);

		Assert.IsFalse(result.Success);
		Assert.IsTrue(result.ErrorMessage!.Contains("Movie file parsing failed") ||
			result.ErrorMessage!.Contains("Unknown system type"));
	}

	[TestMethod]
	public async Task UpdateSubmission_WithDeprecatedMovieFormat_ReturnsError()
	{
		var submission = _db.AddAndSaveUnpublishedSubmission().Entity;
		var movieFile = CreateMockMovieFile("test.deprecated", "deprecated movie data");

		var request = CreateValidUpdateSubmissionRequest(
			submission.Id,
			replaceMovieFile: movieFile);

		var result = await _queueService.UpdateSubmission(request);

		Assert.IsFalse(result.Success);
		Assert.IsFalse(string.IsNullOrWhiteSpace(result.ErrorMessage));
	}

	[TestMethod]
	public async Task UpdateSubmission_WithoutTopic_UpdatesSuccessfully()
	{
		var submission = _db.AddAndSaveUnpublishedSubmission().Entity;
		await _db.SaveChangesAsync();

		var request = CreateValidUpdateSubmissionRequest(
			submission.Id,
			gameName: "New Game Name");

		_wikiPages.Add(Arg.Any<WikiCreateRequest>()).Returns(new WikiResult { Markup = "Wiki content" });

		var result = await _queueService.UpdateSubmission(request);

		Assert.IsTrue(result.Success);

		var updatedSubmission = await _db.Submissions.FindAsync(submission.Id);
		Assert.IsNotNull(updatedSubmission);
		Assert.AreEqual("New Game Name", updatedSubmission.GameName);
	}

	private static IFormFile CreateMockMovieFile(string fileName, string content)
	{
		var bytes = System.Text.Encoding.UTF8.GetBytes(content);
		var stream = new MemoryStream(bytes);
		var file = Substitute.For<IFormFile>();
		file.FileName.Returns(fileName);
		file.Length.Returns(bytes.Length);
		file.OpenReadStream().Returns(stream);
		file.CopyToAsync(Arg.Any<Stream>()).Returns(async (ci) =>
		{
			var targetStream = (Stream)ci[0];
			stream.Seek(0, SeekOrigin.Begin);
			await stream.CopyToAsync(targetStream);
		});
		return file;
	}

	private static UpdateSubmissionRequest CreateValidUpdateSubmissionRequest(
		int submissionId,
		string userName = "TestUser",
		IFormFile? replaceMovieFile = null,
		int? intendedPublicationClass = null,
		int? rejectionReason = null,
		string gameName = "Test Game",
		string? gameVersion = null,
		string? romName = null,
		string? goal = null,
		string? emulator = null,
		string? encodeEmbedLink = null,
		List<string>? authors = null,
		string? externalAuthors = null,
		SubmissionStatus status = SubmissionStatus.New,
		bool markupChanged = false,
		string? markup = null,
		string? revisionMessage = null,
		bool minorEdit = false,
		int userId = 1)
	{
		return new UpdateSubmissionRequest(
			submissionId,
			userName,
			replaceMovieFile,
			intendedPublicationClass,
			rejectionReason,
			gameName,
			gameVersion,
			romName,
			goal,
			emulator,
			encodeEmbedLink,
			authors ?? ["TestAuthor"],
			externalAuthors,
			status,
			markupChanged,
			markup,
			revisionMessage,
			minorEdit,
			userId);
	}

	#endregion

	#region ParseMovieFile

	[TestMethod]
	public async Task ParseMovieFile_NonZipFile_ParsesFileAndZipsResult()
	{
		var formFile = Substitute.For<IFormFile>();
		formFile.FileName.Returns("test.bk2");
		formFile.ContentType.Returns("application/octet-stream");

		var fileStream = new MemoryStream([1, 2, 3, 4]);
		formFile.CopyToAsync(Arg.Any<Stream>()).Returns(Task.CompletedTask)
			.AndDoes(x => fileStream.CopyTo((Stream)x.Args()[0]));

		var parseResult = Substitute.For<IParseResult>();
		parseResult.Success.Returns(true);
		parseResult.FileExtension.Returns(".bk2");

		_movieParser.ParseFile("test.bk2", Arg.Any<Stream>()).Returns(parseResult);

		var zippedBytes = new byte[] { 5, 6, 7, 8, 9 };
		_fileService.ZipFile(Arg.Any<byte[]>(), "test.bk2").Returns(zippedBytes);

		var (result, movieBytes) = await _queueService.ParseMovieFileOrZip(formFile);

		Assert.AreEqual(parseResult, result);
		Assert.AreEqual(zippedBytes, movieBytes);
		await _movieParser.Received(1).ParseFile("test.bk2", Arg.Any<Stream>());
		await _fileService.Received(1).ZipFile(Arg.Any<byte[]>(), "test.bk2");
	}

	[TestMethod]
	public async Task ParseMovieFile_ZipFile_ParsesZipAndReturnsRawBytes()
	{
		var formFile = Substitute.For<IFormFile>();
		formFile.FileName.Returns("test.zip");
		formFile.ContentType.Returns("application/zip");

		var fileStream = new MemoryStream([1, 2, 3, 4]);
		formFile.CopyToAsync(Arg.Any<Stream>()).Returns(Task.CompletedTask)
			.AndDoes(x => fileStream.CopyTo((Stream)x.Args()[0]));

		var parseResult = Substitute.For<IParseResult>();
		parseResult.Success.Returns(true);

		_movieParser.ParseZip(Arg.Any<Stream>()).Returns(parseResult);

		var (result, movieBytes) = await _queueService.ParseMovieFileOrZip(formFile);

		Assert.AreEqual(parseResult, result);
		Assert.AreEqual(4, movieBytes.Length); // Raw file bytes
		await _movieParser.Received(1).ParseZip(Arg.Any<Stream>());
		await _fileService.DidNotReceive().ZipFile(Arg.Any<byte[]>(), Arg.Any<string>());
	}

	#endregion

	#region ClaimForJudging

	[TestMethod]
	public async Task ClaimForJudging_NonExistentSubmission_ReturnsError()
	{
		var result = await _queueService.ClaimForJudging(999, 1, "TestUser");

		Assert.IsFalse(result.Success);
		Assert.AreEqual("Submission not found", result.ErrorMessage);
	}

	[TestMethod]
	public async Task ClaimForJudging_WrongStatus_ReturnsError()
	{
		var submission = _db.Submissions.Add(new Submission
		{
			Status = JudgingUnderWay,
			Submitter = _db.AddUser(1).Entity
		}).Entity;
		await _db.SaveChangesAsync();

		var result = await _queueService.ClaimForJudging(submission.Id, 2, "TestUser");

		Assert.IsFalse(result.Success);
		Assert.AreEqual("Submission can not be claimed", result.ErrorMessage);
	}

	[TestMethod]
	public async Task ClaimForJudging_ValidRequest_Success()
	{
		_db.AddForumConstantEntities();
		var user = _db.AddUser(100).Entity;
		var judge = _db.AddUser(101).Entity;
		var submission = _db.Submissions.Add(new Submission
		{
			Status = New,
			Submitter = user,
			Title = "Test Submission"
		}).Entity;

		var wikiPage = Substitute.For<IWikiPage>();
		wikiPage.PageName.Returns("1S");
		wikiPage.Markup.Returns("Original markup");
		_wikiPages.Page(Arg.Any<string>(), Arg.Any<int?>()).Returns(wikiPage);
		_wikiPages.Add(Arg.Any<WikiCreateRequest>()).Returns(wikiPage);
		await _db.SaveChangesAsync();

		var result = await _queueService.ClaimForJudging(submission.Id, judge.Id, judge.UserName);

		Assert.IsTrue(result.Success);
		Assert.AreEqual("Test Submission", result.SubmissionTitle);
		Assert.IsNull(result.ErrorMessage);

		await _db.Entry(submission).ReloadAsync();
		Assert.AreEqual(JudgingUnderWay, submission.Status);
		Assert.AreEqual(judge.Id, submission.JudgeId);
	}

	#endregion

	#region ClaimForPublishing

	[TestMethod]
	public async Task ClaimForPublishing_NonExistentSubmission_ReturnsError()
	{
		var result = await _queueService.ClaimForPublishing(999, 1, "TestUser");

		Assert.IsFalse(result.Success);
		Assert.AreEqual("Submission not found", result.ErrorMessage);
	}

	[TestMethod]
	public async Task ClaimForPublishing_WrongStatus_ReturnsError()
	{
		var submission = _db.Submissions.Add(new Submission
		{
			Status = New,
			Submitter = _db.AddUser(1).Entity
		}).Entity;
		await _db.SaveChangesAsync();

		var result = await _queueService.ClaimForPublishing(submission.Id, 2, "TestUser");

		Assert.IsFalse(result.Success);
		Assert.AreEqual("Submission can not be claimed", result.ErrorMessage);
	}

	[TestMethod]
	public async Task ClaimForPublishing_ValidRequest_Success()
	{
		_db.AddForumConstantEntities();
		var user = _db.AddUser(200).Entity;
		var publisher = _db.AddUser(201).Entity;
		var submission = _db.Submissions.Add(new Submission
		{
			Status = Accepted,
			Submitter = user,
			Title = "Test Submission"
		}).Entity;

		var wikiPage = Substitute.For<IWikiPage>();
		wikiPage.PageName.Returns("1S");
		wikiPage.Markup.Returns("Original markup");
		_wikiPages.Page(Arg.Any<string>(), Arg.Any<int?>()).Returns(wikiPage);
		_wikiPages.Add(Arg.Any<WikiCreateRequest>()).Returns(wikiPage);
		await _db.SaveChangesAsync();

		var result = await _queueService.ClaimForPublishing(submission.Id, publisher.Id, publisher.UserName);

		Assert.IsTrue(result.Success);
		Assert.AreEqual("Test Submission", result.SubmissionTitle);
		Assert.IsNull(result.ErrorMessage);

		await _db.Entry(submission).ReloadAsync();
		Assert.AreEqual(PublicationUnderway, submission.Status);
		Assert.AreEqual(publisher.Id, submission.PublisherId);
	}

	#endregion
}
