using TASVideos.Core.Services.Wiki;
using TASVideos.Core.Services.Youtube;
using TASVideos.Core.Settings;
using TASVideos.Data.Entity;
using TASVideos.Data.Entity.Awards;
using TASVideos.Data.Entity.Forum;
using TASVideos.Data.Entity.Game;
using TASVideos.Extensions;
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
		var settings = new AppSettings
		{
			MinimumHoursBeforeJudgment = MinimumHoursBeforeJudgment,
			SubmissionRate = new() { Days = SubmissionRateDays, Submissions = SubmissionRateSubs }
		};
		_queueService = new QueueService(settings, _db, _youtubeSync, _tva, _wikiPages);
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
		var poll = new ForumPoll { Id = pollId, TopicId = topic.Id };
		_db.ForumPolls.Add(poll);
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
		var post1 = new ForumPost { Topic = topic, TopicId = topic.Id, Text = "1", ForumId = topic.ForumId, Poster = user };
		var post2 = new ForumPost { Topic = topic, TopicId = topic.Id, Text = "2", ForumId = topic.ForumId, Poster = user };
		_db.ForumPosts.Add(post1);
		_db.ForumPosts.Add(post2);
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

	#region Unpublish

	[TestMethod]
	public async Task CanUnpublish_NotFound()
	{
		var result = await _queueService.CanUnpublish(int.MaxValue);
		Assert.IsNotNull(result);
		Assert.AreEqual(UnpublishResult.UnpublishStatus.NotFound, result.Status);
		Assert.IsTrue(string.IsNullOrWhiteSpace(result.ErrorMessage));
		Assert.IsTrue(string.IsNullOrWhiteSpace(result.PublicationTitle));
	}

	[TestMethod]
	public async Task CanUnpublish_CannotUnpublishWithAwards()
	{
		var pub = _db.AddPublication().Entity;
		_db.PublicationAwards.Add(new PublicationAward { Publication = pub, Award = new Award() });
		await _db.SaveChangesAsync();

		var result = await _queueService.CanUnpublish(pub.Id);
		Assert.IsNotNull(result);
		Assert.AreEqual(UnpublishResult.UnpublishStatus.NotAllowed, result.Status);
		Assert.IsFalse(string.IsNullOrWhiteSpace(result.ErrorMessage));
		Assert.IsFalse(string.IsNullOrWhiteSpace(result.PublicationTitle));
	}

	[TestMethod]
	public async Task CanUnpublish_Success()
	{
		var pub = _db.AddPublication().Entity;
		const string publicationTitle = "Test Publication";
		pub.Title = publicationTitle;
		await _db.SaveChangesAsync();

		var result = await _queueService.CanUnpublish(pub.Id);
		Assert.IsNotNull(result);
		Assert.AreEqual(UnpublishResult.UnpublishStatus.Success, result.Status);
		Assert.IsTrue(string.IsNullOrWhiteSpace(result.ErrorMessage));
		Assert.AreEqual(publicationTitle, result.PublicationTitle);
	}

	[TestMethod]
	public async Task Unpublish_NotFound()
	{
		var result = await _queueService.Unpublish(int.MaxValue);
		Assert.IsNotNull(result);
		Assert.AreEqual(UnpublishResult.UnpublishStatus.NotFound, result.Status);
		Assert.IsTrue(string.IsNullOrWhiteSpace(result.ErrorMessage));
		Assert.IsTrue(string.IsNullOrWhiteSpace(result.PublicationTitle));
	}

	[TestMethod]
	public async Task Unpublish_CannotUnpublishWithAwards()
	{
		var pub = _db.AddPublication().Entity;
		_db.PublicationAwards.Add(new PublicationAward { Publication = pub, Award = new Award() });
		await _db.SaveChangesAsync();

		var result = await _queueService.Unpublish(pub.Id);
		Assert.IsNotNull(result);
		Assert.AreEqual(UnpublishResult.UnpublishStatus.NotAllowed, result.Status);
		Assert.IsFalse(string.IsNullOrWhiteSpace(result.ErrorMessage));
		Assert.IsFalse(string.IsNullOrWhiteSpace(result.PublicationTitle));
	}

	[TestMethod]
	public async Task Unpublish_NoObsoletedMovie_NoYoutube()
	{
		_youtubeSync.IsYoutubeUrl(Arg.Any<string>()).Returns(true);

		var user1 = _db.AddUser(0).Entity;
		var user2 = _db.AddUser(0).Entity;
		await _db.SaveChangesAsync();

		var pub = _db.AddPublication().Entity;
		_db.PublicationAuthors.Add(new PublicationAuthor { Publication = pub, UserId = user1.Id });
		_db.PublicationAuthors.Add(new PublicationAuthor { Publication = pub, UserId = user2.Id });
		_db.PublicationFiles.Add(new PublicationFile { Publication = pub });
		_db.PublicationFiles.Add(new PublicationFile { Publication = pub });
		_db.PublicationFlags.Add(new PublicationFlag { Publication = pub, Flag = new Flag { Token = "1" } });
		_db.PublicationFlags.Add(new PublicationFlag { Publication = pub, Flag = new Flag { Token = "2" } });
		_db.PublicationRatings.Add(new PublicationRating { Publication = pub, User = user1 });
		_db.PublicationRatings.Add(new PublicationRating { Publication = pub, User = user2 });
		_db.PublicationTags.Add(new PublicationTag { Publication = pub, Tag = new Tag { Code = "1" } });
		_db.PublicationTags.Add(new PublicationTag { Publication = pub, Tag = new Tag { Code = "2" } });
		_db.PublicationUrls.Add(new PublicationUrl
		{
			Publication = pub,
			Type = PublicationUrlType.Streaming,
			Url = "https://www.youtube.com/watch?v=dQw4w9WgXcQ"
		});
		await _db.SaveChangesAsync();
		int publicationId = pub.Id;
		int submissionId = pub.Submission!.Id;

		var result = await _queueService.Unpublish(publicationId);

		// Result must be correct
		Assert.IsNotNull(result);
		Assert.AreEqual(UnpublishResult.UnpublishStatus.Success, result.Status);
		Assert.AreEqual(pub.Title, result.PublicationTitle);
		Assert.IsTrue(string.IsNullOrWhiteSpace(result.ErrorMessage));

		// Publication sub-tables must be cleared
		Assert.AreEqual(0, _db.PublicationAuthors.Count(pa => pa.PublicationId == publicationId));
		Assert.AreEqual(0, _db.PublicationFiles.Count(pf => pf.PublicationId == publicationId));
		Assert.AreEqual(0, _db.PublicationRatings.Count(pr => pr.PublicationId == publicationId));
		Assert.AreEqual(0, _db.PublicationTags.Count(pt => pt.PublicationId == publicationId));

		// Publication is removed
		Assert.AreEqual(0, _db.Publications.Count(p => p.Id == publicationId));

		// Submission must be reset
		Assert.IsTrue(_db.Submissions.Any(s => s.Id == submissionId));

		var sub = _db.Submissions.Single(s => s.Id == submissionId);
		Assert.AreEqual(sub.PublisherId, pub.Submission!.PublisherId);
		Assert.AreEqual(PublicationUnderway, sub.Status);

		// YouTube url should be unlisted
		await _youtubeSync.Received(1).UnlistVideo(Arg.Any<string>());

		// Submission status history added for published status
		Assert.AreEqual(1, _db.SubmissionStatusHistory.Count(sh => sh.SubmissionId == submissionId));
		var statusHistory = _db.SubmissionStatusHistory.Single(sh => sh.SubmissionId == submissionId);
		Assert.AreEqual(Published, statusHistory.Status);

		// TVA post is made
		await _tva.Received(1).PostSubmissionUnpublished(submissionId);
	}

	[TestMethod]
	public async Task Unpublish_ObsoletedMovies_ResetAndSync()
	{
		_youtubeSync.IsYoutubeUrl(Arg.Any<string>()).Returns(true);

		var obsoletedPub = _db.AddPublication().Entity;
		_db.PublicationUrls.Add(new PublicationUrl
		{
			Publication = obsoletedPub,
			Type = PublicationUrlType.Streaming,
			Url = "https://www.youtube.com/watch?v=dQw4w9WgXcQ"
		});
		var pub = _db.AddPublication().Entity;

		obsoletedPub.ObsoletedBy = pub;

		await _db.SaveChangesAsync();
		var result = await _queueService.Unpublish(pub.Id);

		// Result must be correct
		Assert.IsNotNull(result);
		Assert.AreEqual(UnpublishResult.UnpublishStatus.Success, result.Status);
		Assert.AreEqual(pub.Title, result.PublicationTitle);
		Assert.IsTrue(string.IsNullOrWhiteSpace(result.ErrorMessage));

		// Obsoleted movie must no longer be obsolete
		Assert.AreEqual(1, _db.Publications.Count(p => p.Id == obsoletedPub.Id));
		var obsoletedMovie = _db.Publications.Single(p => p.Id == obsoletedPub.Id);
		Assert.IsNull(obsoletedMovie.ObsoletedById);

		// Obsoleted movie YouTube url must be synced
		await _youtubeSync.Received(1).SyncYouTubeVideo(Arg.Any<YoutubeVideo>());
	}

	#endregion

	#region MapParsedResult

	[TestMethod]
	public async Task MapParsedResult_ThrowsIfParsingIsFailed()
	{
		await Assert.ThrowsExactlyAsync<InvalidOperationException>(() => _queueService.MapParsedResult(new TestParseResult { Success = false }, new Submission()));
	}

	[TestMethod]
	public async Task MapParsedResult_ErrorIfUnknownSystem()
	{
		_db.GameSystems.Add(new GameSystem { Code = "NES" });
		await _db.SaveChangesAsync();

		var actual = await _queueService.MapParsedResult(new TestParseResult { Success = true, SystemCode = "Does not exist" }, new Submission());

		Assert.IsFalse(string.IsNullOrWhiteSpace(actual));
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

		var submission = new Submission();

		var actual = await _queueService.MapParsedResult(parseResult, submission);
		Assert.IsTrue(string.IsNullOrEmpty(actual));
		Assert.IsNotNull(submission.SystemFrameRate);
		Assert.AreEqual(frameRate, submission.SystemFrameRate.FrameRate);
		Assert.AreEqual(region.ToString().ToUpper(), submission.SystemFrameRate.RegionCode);
		Assert.AreEqual((int)startType, submission.MovieStartType);
		Assert.AreEqual(frames, submission.Frames);
		Assert.AreEqual(rerecordCount, submission.RerecordCount);
		Assert.AreEqual(fileExtension, submission.MovieExtension);
		Assert.IsNotNull(submission.System);
		Assert.AreEqual(system, submission.System.Code);
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

		var submission = new Submission();

		var actual = await _queueService.MapParsedResult(parseResult, submission);
		Assert.IsTrue(string.IsNullOrEmpty(actual));
		Assert.IsNotNull(submission.SystemFrameRate);
		Assert.AreEqual(frameRateOverride, submission.SystemFrameRate.FrameRate);
		Assert.AreEqual(region.ToString().ToUpper(), submission.SystemFrameRate.RegionCode);
		Assert.AreEqual(entry.Entity, submission.SystemFrameRate.System);
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
}
