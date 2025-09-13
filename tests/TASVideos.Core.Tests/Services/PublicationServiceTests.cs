using TASVideos.Core.Services.Wiki;
using TASVideos.Core.Services.Youtube;
using TASVideos.Data.Entity;
using TASVideos.Data.Entity.Awards;
using static TASVideos.Data.Entity.SubmissionStatus;

namespace TASVideos.Core.Tests.Services;

[TestClass]
public class PublicationsTests : TestDbBase
{
	private readonly IYoutubeSync _youtubeSync;
	private readonly ITASVideoAgent _tva;

	private readonly Publications _publications;

	public PublicationsTests()
	{
		_youtubeSync = Substitute.For<IYoutubeSync>();
		_tva = Substitute.For<ITASVideoAgent>();
		var wikiPages = Substitute.For<IWikiPages>();
		var tagService = Substitute.For<ITagService>();
		var flagService = Substitute.For<IFlagService>();
		_publications = new Publications(_db, _youtubeSync, _tva, wikiPages, tagService, flagService);
	}

	#region GetTitle

	[TestMethod]
	public async Task GetTitle_NotFound_ReturnsNull()
	{
		var actual = await _publications.GetTitle(int.MaxValue);
		Assert.IsNull(actual);
	}

	[TestMethod]
	public async Task GetTitle_Found_ReturnsTitle()
	{
		var pub = _db.AddPublication().Entity;
		var actual = await _publications.GetTitle(pub.Id);
		Assert.IsNotNull(actual);
		Assert.AreEqual(pub.Title, actual);
	}

	#endregion

	#region GetUrls

	[TestMethod]
	public async Task GetUrls_NotFound_ReturnsEmptyList()
	{
		var actual = await _publications.GetUrls(int.MaxValue);
		Assert.AreEqual(0, actual.Count);
	}

	[TestMethod]
	public async Task GetUrls_FoundWithNoUrls_ReturnsEmptyList()
	{
		var pub = _db.AddPublication().Entity;
		var actual = await _publications.GetUrls(pub.Id);
		Assert.AreEqual(0, actual.Count);
	}

	[TestMethod]
	public async Task GetUrls_FoundWithUrls_ReturnsUrls()
	{
		var pub = _db.AddPublication().Entity;
		_db.AddStreamingUrl(pub, "url1");
		_db.AddStreamingUrl(pub, "url2");
		await _db.SaveChangesAsync();

		var urls = await _publications.GetUrls(pub.Id);

		Assert.AreEqual(2,  urls.Count);
	}

	#endregion

	#region CanUnpublish

	[TestMethod]
	public async Task CanUnpublish_NotFound()
	{
		var result = await _publications.CanUnpublish(int.MaxValue);
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

		var result = await _publications.CanUnpublish(pub.Id);
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

		var result = await _publications.CanUnpublish(pub.Id);
		Assert.IsNotNull(result);
		Assert.AreEqual(UnpublishResult.UnpublishStatus.Success, result.Status);
		Assert.IsTrue(string.IsNullOrWhiteSpace(result.ErrorMessage));
		Assert.AreEqual(publicationTitle, result.PublicationTitle);
	}

	#endregion

	#region Unpublish

	[TestMethod]
	public async Task Unpublish_NotFound()
	{
		var result = await _publications.Unpublish(int.MaxValue);
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

		var result = await _publications.Unpublish(pub.Id);
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
		_db.AddStreamingUrl(pub, "https://www.youtube.com/watch?v=dQw4w9WgXcQ");
		await _db.SaveChangesAsync();
		int publicationId = pub.Id;
		int submissionId = pub.Submission!.Id;

		var result = await _publications.Unpublish(publicationId);

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
		_db.AddStreamingUrl(obsoletedPub, "https://www.youtube.com/watch?v=dQw4w9WgXcQ");
		var pub = _db.AddPublication().Entity;

		obsoletedPub.ObsoletedBy = pub;

		await _db.SaveChangesAsync();
		var result = await _publications.Unpublish(pub.Id);

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

	#region RemoveUrl

	[TestMethod]
	public async Task RemoveUrl_NotFound_ReturnsNull()
	{
		var result = await _publications.RemoveUrl(int.MaxValue);
		Assert.IsNull(result);
	}

	[TestMethod]
	public async Task RemoveUrl_Found_RemovesUrlAndReturnsSuccess()
	{
		var pub = _db.AddPublication().Entity;
		var url = _db.AddStreamingUrl(pub, "https://example.com/video").Entity;
		await _db.SaveChangesAsync();

		var result = await _publications.RemoveUrl(url.Id);

		Assert.IsNotNull(result);
		Assert.AreEqual(url.Id, result.Id);
		Assert.AreEqual(url.Url, result.Url);
		Assert.AreEqual(url.Type, result.Type);
		Assert.AreEqual(0, _db.PublicationUrls.Count(u => u.Id == url.Id));
	}

	[TestMethod]
	public async Task RemoveUrl_DatabaseFailure_ReturnsNull()
	{
		var pub = _db.AddPublication().Entity;
		var url = _db.AddStreamingUrl(pub, "https://example.com/video").Entity;
		await _db.SaveChangesAsync();

		_db.CreateUpdateConflict();
		var result = await _publications.RemoveUrl(url.Id);

		Assert.IsNull(result);
		Assert.AreEqual(1, _db.PublicationUrls.Count(u => u.Id == url.Id));
	}

	[TestMethod]
	public async Task RemoveUrl_ConcurrencyFailure_ReturnsConcurrencyFailure()
	{
		var pub = _db.AddPublication().Entity;
		var url = _db.AddStreamingUrl(pub, "https://example.com/video").Entity;
		await _db.SaveChangesAsync();

		_db.CreateConcurrentUpdateConflict();
		var result = await _publications.RemoveUrl(url.Id);

		Assert.IsNull(result);
		Assert.AreEqual(1, _db.PublicationUrls.Count(u => u.Id == url.Id));
	}

	#endregion
}
