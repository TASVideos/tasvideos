﻿using TASVideos.Core.Services;
using TASVideos.Core.Services.Youtube;
using TASVideos.Core.Settings;
using TASVideos.Data.Entity;
using TASVideos.Data.Entity.Awards;
using TASVideos.Data.Entity.Game;
using TASVideos.MovieParsers.Result;
using static TASVideos.Data.Entity.SubmissionStatus;

namespace TASVideos.Core.Tests.Services;

[TestClass]
public class QueueServiceTests
{
	private const int MinimumHoursBeforeJudgment = 72;
	private readonly QueueService _queueService;
	private readonly TestDbContext _db;
	private readonly Mock<IYoutubeSync> _youtubeSync;
	private readonly Mock<ITASVideoAgent> _tva;

	private static DateTime TooNewToJudge => DateTime.UtcNow;

	private static DateTime OldEnoughToBeJudged
		=> DateTime.UtcNow.AddHours(-1 - MinimumHoursBeforeJudgment);

	private static readonly IEnumerable<PermissionTo> BasicUserPerms = new[] { PermissionTo.SubmitMovies };
	private static readonly IEnumerable<PermissionTo> JudgePerms = new[] { PermissionTo.SubmitMovies, PermissionTo.JudgeSubmissions };
	private static readonly IEnumerable<PermissionTo> PublisherPerms = new[] { PermissionTo.SubmitMovies, PermissionTo.PublishMovies };
	private static readonly IEnumerable<PermissionTo> Override = new[] { PermissionTo.OverrideSubmissionStatus };

	public QueueServiceTests()
	{
		_db = TestDbContext.Create();
		_youtubeSync = new Mock<IYoutubeSync>();
		_tva = new Mock<ITASVideoAgent>();
		var settings = new AppSettings { MinimumHoursBeforeJudgment = MinimumHoursBeforeJudgment };
		_queueService = new QueueService(settings, _db, _youtubeSync.Object, _tva.Object);
	}

	[TestMethod]
	public void Published_CanNotChange()
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
	[TestMethod]
	public void Submitter_BasicPerms(SubmissionStatus current, IEnumerable<SubmissionStatus> canChangeTo)
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
	[TestMethod]
	public void Submitter_IsJudge(SubmissionStatus current, IEnumerable<SubmissionStatus> canChangeTo)
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
	[TestMethod]
	public void Submitter_IsPublisher(SubmissionStatus current, IEnumerable<SubmissionStatus> canChangeTo)
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
	[TestMethod]
	public void Judge_ButNotSubmitter_BeforeAllowedJudgmentWindow(SubmissionStatus current, IEnumerable<SubmissionStatus> canChangeTo)
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
	[DataRow(Delayed, new[] { New, NeedsMoreInfo, JudgingUnderWay, Accepted, Rejected, Cancelled })]
	[DataRow(NeedsMoreInfo, new[] { New, Delayed, JudgingUnderWay, Accepted, Rejected, Cancelled })]
	[DataRow(JudgingUnderWay, new[] { New, Delayed, NeedsMoreInfo, Accepted, Rejected, Cancelled })]
	[DataRow(Accepted, new[] { New, Delayed, NeedsMoreInfo, JudgingUnderWay, Rejected, Cancelled })]
	[DataRow(PublicationUnderway, new[] { New, Delayed, NeedsMoreInfo, JudgingUnderWay, Accepted, Rejected, Cancelled })]
	[DataRow(Rejected, new[] { New, JudgingUnderWay })]
	[DataRow(Cancelled, new[] { New, JudgingUnderWay })]
	[TestMethod]
	public void Judge_ButNotSubmitter_AfterAllowedJudgmentWindow(SubmissionStatus current, IEnumerable<SubmissionStatus> canChangeTo)
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
	[TestMethod]
	public void Publisher_ButNotSubmitter_BeforeAllowedJudgmentWindow_CanNotChangeStatus(SubmissionStatus current, IEnumerable<SubmissionStatus> canChangeTo)
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
	[TestMethod]
	public void Publisher_ButNotSubmitter_AfterAllowedJudgmentWindow(SubmissionStatus current, IEnumerable<SubmissionStatus> canChangeTo)
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
		var exceptPublished = Enum.GetValues(typeof(SubmissionStatus))
			.Cast<SubmissionStatus>()
			.Except(new[] { Published })
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
		int publicationId = 1;
		int awardId = 2;
		_db.Publications.Add(new Publication { Id = publicationId });
		_db.PublicationAwards.Add(new PublicationAward { PublicationId = publicationId, AwardId = awardId });
		await _db.SaveChangesAsync();

		var result = await _queueService.CanUnpublish(publicationId);
		Assert.IsNotNull(result);
		Assert.AreEqual(UnpublishResult.UnpublishStatus.NotAllowed, result.Status);
		Assert.IsTrue(!string.IsNullOrWhiteSpace(result.ErrorMessage));
		Assert.IsTrue(string.IsNullOrWhiteSpace(result.PublicationTitle));
	}

	[TestMethod]
	public async Task CanUnpublish_Success()
	{
		int publicationId = 1;
		_db.Publications.Add(new Publication { Id = publicationId, Title = "Test Publication" });
		await _db.SaveChangesAsync();

		var result = await _queueService.CanUnpublish(publicationId);
		Assert.IsNotNull(result);
		Assert.AreEqual(UnpublishResult.UnpublishStatus.Success, result.Status);
		Assert.IsTrue(string.IsNullOrWhiteSpace(result.ErrorMessage));
		Assert.IsTrue(!string.IsNullOrWhiteSpace(result.PublicationTitle));
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
		int publicationId = 1;
		int awardId = 2;
		_db.Publications.Add(new Publication
		{
			Id = publicationId,
			Submission = new Submission { PublisherId = publicationId } // A quirk of InMemoryDatabase, 1:1 relationships need the other object for the .Include() to work
		});
		_db.PublicationAwards.Add(new PublicationAward { PublicationId = publicationId, AwardId = awardId });
		await _db.SaveChangesAsync();

		var result = await _queueService.Unpublish(publicationId);
		Assert.IsNotNull(result);
		Assert.AreEqual(UnpublishResult.UnpublishStatus.NotAllowed, result.Status);
		Assert.IsTrue(!string.IsNullOrWhiteSpace(result.ErrorMessage));
		Assert.IsTrue(string.IsNullOrWhiteSpace(result.PublicationTitle));
	}

	[TestMethod]
	public async Task Unpublish_NoObsoletedMovie_NoYoutube()
	{
		_youtubeSync
			.Setup(m => m.IsYoutubeUrl(It.IsAny<string>()))
			.Returns(true);

		const int publicationId = 1;
		const string publicationTitle = "Test Publication";

		const int publisherId = 3;
		const int submissionId = 2;
		var submission = new Submission
		{
			Id = submissionId,
			Status = Published,
			PublisherId = publisherId
		};

		_db.Submissions.Add(submission);
		_db.Publications.Add(new Publication
		{
			Id = publicationId,
			Title = publicationTitle,
			Submission = submission,
			SubmissionId = submissionId
		});
		_db.PublicationAuthors.Add(new PublicationAuthor { PublicationId = publicationId, UserId = 1 });
		_db.PublicationAuthors.Add(new PublicationAuthor { PublicationId = publicationId, UserId = 2 });
		_db.PublicationFiles.Add(new PublicationFile { PublicationId = publicationId });
		_db.PublicationFiles.Add(new PublicationFile { PublicationId = publicationId });
		_db.PublicationFlags.Add(new PublicationFlag { PublicationId = publicationId, FlagId = 1 });
		_db.PublicationFlags.Add(new PublicationFlag { PublicationId = publicationId, FlagId = 2 });
		_db.PublicationRatings.Add(new PublicationRating { PublicationId = publicationId, UserId = 1 });
		_db.PublicationRatings.Add(new PublicationRating { PublicationId = publicationId, UserId = 2 });
		_db.PublicationTags.Add(new PublicationTag { PublicationId = publicationId, TagId = 1 });
		_db.PublicationTags.Add(new PublicationTag { PublicationId = publicationId, TagId = 2 });
		_db.PublicationUrls.Add(new PublicationUrl
		{
			PublicationId = publicationId,
			Type = PublicationUrlType.Streaming,
			Url = "https://www.youtube.com/watch?v=dQw4w9WgXcQ"
		});
		await _db.SaveChangesAsync();

		var result = await _queueService.Unpublish(publicationId);

		// Result must be correct
		Assert.IsNotNull(result);
		Assert.AreEqual(UnpublishResult.UnpublishStatus.Success, result.Status);
		Assert.AreEqual(publicationTitle, result.PublicationTitle);
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
		Assert.AreEqual(sub.PublisherId, publisherId);
		Assert.AreEqual(PublicationUnderway, sub.Status);

		// Youtube url should be unlisted
		_youtubeSync.Verify(v => v.UnlistVideo(It.IsAny<string>()));

		// Submission status history added for published status
		Assert.AreEqual(1, _db.SubmissionStatusHistory.Count(sh => sh.SubmissionId == submissionId));
		var statusHistory = _db.SubmissionStatusHistory.Single(sh => sh.SubmissionId == submissionId);
		Assert.AreEqual(Published, statusHistory.Status);

		// TVA post is made
		_tva.Verify(v => v.PostSubmissionUnpublished(submissionId));
	}

	[TestMethod]
	public async Task Unpublish_ObsoletedMovies_ResetAndSync()
	{
		_youtubeSync
			.Setup(m => m.IsYoutubeUrl(It.IsAny<string>()))
			.Returns(true);

		var wikiEntry = _db.WikiPages.Add(new WikiPage { Markup = "Test" });
		var systemEntry = _db.GameSystems.Add(new GameSystem { Code = "Test" });
		var gameEntry = _db.Games.Add(new Game { SearchKey = "Test" });
		var authorEntry = _db.Users.Add(new User { UserName = "Author" });

		const int publicationId = 1;
		const string publicationTitle = "Test Publication";
		const int obsoletedPublicationId = 10;

		const int publisherId = 3;
		const int submissionId = 2;
		var submission = new Submission
		{
			Id = submissionId,
			Status = Published,
			PublisherId = publisherId
		};

		_db.Submissions.Add(submission);

		_db.Publications.Add(new Publication
		{
			Id = obsoletedPublicationId,
			ObsoletedById = publicationId,
			WikiContentId = wikiEntry.Entity.Id,
			SystemId = systemEntry.Entity.Id,
			GameId = gameEntry.Entity.Id
		});

		_db.PublicationUrls.Add(new PublicationUrl
		{
			PublicationId = obsoletedPublicationId,
			Type = PublicationUrlType.Streaming,
			Url = "https://www.youtube.com/watch?v=dQw4w9WgXcQ"
		});

		_db.PublicationAuthors.Add(new PublicationAuthor { PublicationId = obsoletedPublicationId, UserId = authorEntry.Entity.Id });

		_db.Publications.Add(new Publication
		{
			Id = publicationId,
			Title = publicationTitle,
			Submission = submission,
			SubmissionId = submissionId
		});

		await _db.SaveChangesAsync();
		var result = await _queueService.Unpublish(publicationId);

		// Result must be correct
		Assert.IsNotNull(result);
		Assert.AreEqual(UnpublishResult.UnpublishStatus.Success, result.Status);
		Assert.AreEqual(publicationTitle, result.PublicationTitle);
		Assert.IsTrue(string.IsNullOrWhiteSpace(result.ErrorMessage));

		// Obsoleted movie must no longer be obsolete
		Assert.AreEqual(1, _db.Publications.Count(p => p.Id == obsoletedPublicationId));
		var obsoletedMovie = _db.Publications.Single(p => p.Id == obsoletedPublicationId);
		Assert.IsNull(obsoletedMovie.ObsoletedById);

		// Obsoleted movie youtube url must be synced
		_youtubeSync.Verify(v => v.SyncYouTubeVideo(It.IsAny<YoutubeVideo>()));
	}

	[TestMethod]
	[ExpectedException(typeof(InvalidOperationException))]
	public async Task MapParsedResult_ThrowsIfParsingIsFailed()
	{
		await _queueService.MapParsedResult(new TestParseResult { Success = false }, new Submission());
	}

	[TestMethod]
	public async Task MapParsedResult_ErrorIfUnknownSystem()
	{
		_db.GameSystems.Add(new GameSystem { Code = "NES" });
		await _db.SaveChangesAsync();

		var actual = await _queueService.MapParsedResult(new TestParseResult { Success = true, SystemCode = "Does not exist" }, new Submission());

		Assert.IsTrue(!string.IsNullOrWhiteSpace(actual));
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

	[TestMethod]
	public async Task ObsoleteWith_NoPublication_ReturnsFalse()
	{
		var actual = await _queueService.ObsoleteWith(int.MaxValue, int.MaxValue);
		Assert.IsFalse(actual);
	}

	[TestMethod]
	public async Task ObsoleteWith_Success()
	{
		_youtubeSync.Setup(m => m.IsYoutubeUrl(It.IsAny<string>())).Returns(true);
		int pubToObsolete = 1;
		int obsoletingPub = 2;
		string youtubeUrl = "https://www.youtube.com/watch?v=dQw4w9WgXcQ";
		string wikiMarkup = "Test";
		_db.Publications.Add(new Publication
		{
			Id = pubToObsolete,
			PublicationUrls = new List<PublicationUrl>
			{
				new () { Type = PublicationUrlType.Streaming, Url = youtubeUrl }
			},
			WikiContent = new WikiPage { Markup = wikiMarkup },
			System = new GameSystem { Code = "Test" },
			Game = new Game()
		});
		await _db.SaveChangesAsync();

		var actual = await _queueService.ObsoleteWith(pubToObsolete, obsoletingPub);

		Assert.IsTrue(actual);
		Assert.AreEqual(1, _db.Publications.Count(p => p.Id == pubToObsolete));
		var actualPub = _db.Publications.Single(p => p.Id == pubToObsolete);
		Assert.AreEqual(obsoletingPub, actualPub.ObsoletedById);

		_youtubeSync.Verify(v => v.SyncYouTubeVideo(It.IsAny<YoutubeVideo>()));
	}
}
