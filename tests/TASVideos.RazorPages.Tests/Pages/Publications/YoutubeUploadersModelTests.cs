using TASVideos.Core.Services;
using TASVideos.Core.Services.Youtube;
using TASVideos.Core.Services.Youtube.Dtos;
using TASVideos.Pages.Publications;
using TASVideos.Tests.Base;

namespace TASVideos.RazorPages.Tests.Pages.Publications;

[TestClass]
public class YoutubeUploadersModelTests : TestDbBase
{
	private readonly IYoutubeSync _youtubeSync;
	private readonly ICacheService _cache;
	private readonly YoutubeUploadersModel _model;

	public YoutubeUploadersModelTests()
	{
		_youtubeSync = Substitute.For<IYoutubeSync>();
		_cache = Substitute.For<ICacheService>();
		_model = new YoutubeUploadersModel(_db, _youtubeSync, _cache);
	}

	[TestMethod]
	public async Task OnGet_NoPublications_ReturnsEmptyVideosList()
	{
		await _model.OnGet();
		Assert.AreEqual(0, _model.Videos.Count);
	}

	[TestMethod]
	public async Task OnGet_NonYouTubeUrls_ExcludesFromResults()
	{
		var pub = _db.AddPublication().Entity;
		_db.AddStreamingUrl(pub, "https://example.com/video");
		await _db.SaveChangesAsync();
		_youtubeSync.IsYoutubeUrl("https://example.com/video").Returns(false);

		await _model.OnGet();

		Assert.AreEqual(0, _model.Videos.Count);
	}

	[TestMethod]
	public async Task OnGet_YouTubeUrlsWithCachedChannelTitles_UsesCache()
	{
		var pub = _db.AddPublication().Entity;
		_db.AddStreamingUrl(pub);
		await _db.SaveChangesAsync();

		_youtubeSync.IsYoutubeUrl("https://www.youtube.com/watch?v=dQw4w9WgXcQ").Returns(true);
		_youtubeSync.VideoId("https://www.youtube.com/watch?v=dQw4w9WgXcQ").Returns("dQw4w9WgXcQ");
		_cache.TryGetValue("YoutubeUploaders-dQw4w9WgXcQ", out Arg.Any<string>()).Returns(x =>
		{
			x[1] = "Cached Channel";
			return true;
		});
		_youtubeSync.GetPublicInfo(Arg.Any<IEnumerable<string>>()).Returns([]);

		await _model.OnGet();

		Assert.AreEqual(1, _model.Videos.Count);
		Assert.AreEqual("Cached Channel", _model.Videos[0].ChannelTitle);
	}

	[TestMethod]
	public async Task OnGet_YouTubeUrlsWithoutCache_CallsYouTubeApi()
	{
		var pub = _db.AddPublication().Entity;
		_db.AddStreamingUrl(pub);
		await _db.SaveChangesAsync();

		_youtubeSync.IsYoutubeUrl("https://www.youtube.com/watch?v=dQw4w9WgXcQ").Returns(true);
		_youtubeSync.VideoId("https://www.youtube.com/watch?v=dQw4w9WgXcQ").Returns("dQw4w9WgXcQ");
		_cache.TryGetValue("YoutubeUploaders-dQw4w9WgXcQ", out Arg.Any<string>()).Returns(false);

		var youtubeResponse = new YoutubeVideoResponseItem
		{
			Id = "dQw4w9WgXcQ",
			Snippet = new YoutubeVideoResponseItem.SnippetData
			{
				ChannelTitle = "Definitely a Legit TAS Video"
			}
		};
		_youtubeSync.GetPublicInfo(Arg.Is<IEnumerable<string>>(ids => ids.Contains("dQw4w9WgXcQ")))
			.Returns([youtubeResponse]);

		await _model.OnGet();

		Assert.AreEqual(1, _model.Videos.Count);
		Assert.AreEqual("Definitely a Legit TAS Video", _model.Videos[0].ChannelTitle);
		_cache.Received(1).Set("YoutubeUploaders-dQw4w9WgXcQ", "Definitely a Legit TAS Video", Durations.OneDay);
	}

	[TestMethod]
	public async Task OnGet_MixOfCachedAndUncachedVideos_HandlesBothCorrectly()
	{
		var pub1 = _db.AddPublication().Entity;
		var pub2 = _db.AddPublication().Entity;
		_db.AddStreamingUrl(pub1);
		_db.AddStreamingUrl(pub2, "https://www.youtube.com/watch?v=sVR32jXj68w");
		await _db.SaveChangesAsync();

		_youtubeSync.IsYoutubeUrl(Arg.Any<string>()).Returns(true);
		_youtubeSync.VideoId("https://www.youtube.com/watch?v=dQw4w9WgXcQ").Returns("dQw4w9WgXcQ");
		_youtubeSync.VideoId("https://www.youtube.com/watch?v=sVR32jXj68w").Returns("sVR32jXj68w");

		_cache.TryGetValue("YoutubeUploaders-dQw4w9WgXcQ", out Arg.Any<string>()).Returns(x =>
		{
			x[1] = "Cached Channel";
			return true;
		});
		_cache.TryGetValue("YoutubeUploaders-sVR32jXj68w", out Arg.Any<string>()).Returns(false);

		var youtubeResponse = new YoutubeVideoResponseItem
		{
			Id = "sVR32jXj68w",
			Snippet = new YoutubeVideoResponseItem.SnippetData
			{
				ChannelTitle = "API Channel"
			}
		};

		// ReSharper disable PossibleMultipleEnumeration
		_youtubeSync.GetPublicInfo(Arg.Is<IEnumerable<string>>(ids => ids.Contains("sVR32jXj68w") && !ids.Contains("dQw4w9WgXcQ")))
			.Returns([youtubeResponse]);

		await _model.OnGet();

		Assert.AreEqual(2, _model.Videos.Count);
		var cachedVideo = _model.Videos.First(v => v.VideoId == "dQw4w9WgXcQ");
		var apiVideo = _model.Videos.First(v => v.VideoId == "sVR32jXj68w");

		Assert.AreEqual("Cached Channel", cachedVideo.ChannelTitle);
		Assert.AreEqual("API Channel", apiVideo.ChannelTitle);
		_cache.Received(1).Set("YoutubeUploaders-sVR32jXj68w", "API Channel", Durations.OneDay);
	}

	[TestMethod]
	public async Task OnGet_ObsoletedPublications_IncludesObsoleteFlag()
	{
		var obsoletePub = _db.AddPublication().Entity;
		var currentPub = _db.AddPublication().Entity;
		obsoletePub.ObsoletedById = currentPub.Id;
		_db.AddStreamingUrl(obsoletePub, "https://www.youtube.com/watch?v=obsolete123");
		_db.AddStreamingUrl(currentPub, "https://www.youtube.com/watch?v=current456");
		await _db.SaveChangesAsync();

		_youtubeSync.IsYoutubeUrl(Arg.Any<string>()).Returns(true);
		_youtubeSync.VideoId("https://www.youtube.com/watch?v=obsolete123").Returns("obsolete123");
		_youtubeSync.VideoId("https://www.youtube.com/watch?v=current456").Returns("current456");
		_cache.TryGetValue(Arg.Any<string>(), out Arg.Any<string>()).Returns(false);
		_youtubeSync.GetPublicInfo(Arg.Any<IEnumerable<string>>()).Returns([]);

		await _model.OnGet();

		Assert.AreEqual(2, _model.Videos.Count);
		var obsoleteVideo = _model.Videos.First(v => v.PublicationId == obsoletePub.Id);
		var currentVideo = _model.Videos.First(v => v.PublicationId == currentPub.Id);

		Assert.IsTrue(obsoleteVideo.IsObsolete);
		Assert.IsFalse(currentVideo.IsObsolete);
	}

	[TestMethod]
	public async Task OnGet_DuplicateVideoIds_ReturnsDistinctResults()
	{
		var pub = _db.AddPublication().Entity;
		_db.AddStreamingUrl(pub);
		_db.AddStreamingUrl(pub, "https://youtu.be/dQw4w9WgXcQ");
		await _db.SaveChangesAsync();

		_youtubeSync.IsYoutubeUrl(Arg.Any<string>()).Returns(true);
		_youtubeSync.VideoId(Arg.Any<string>()).Returns("dQw4w9WgXcQ");
		_cache.TryGetValue(Arg.Any<string>(), out Arg.Any<string>()).Returns(false);
		_youtubeSync.GetPublicInfo(Arg.Any<IEnumerable<string>>()).Returns([]);

		await _model.OnGet();

		Assert.AreEqual(1, _model.Videos.Count);
	}

	[TestMethod]
	public async Task OnGet_YouTubeApiReturnsNoResults_LeavesChannelTitleEmpty()
	{
		var pub = _db.AddPublication().Entity;
		_db.AddStreamingUrl(pub);
		await _db.SaveChangesAsync();

		_youtubeSync.IsYoutubeUrl("https://www.youtube.com/watch?v=dQw4w9WgXcQ").Returns(true);
		_youtubeSync.VideoId("https://www.youtube.com/watch?v=dQw4w9WgXcQ").Returns("dQw4w9WgXcQ");
		_cache.TryGetValue("YoutubeUploaders-dQw4w9WgXcQ", out Arg.Any<string>()).Returns(false);
		_youtubeSync.GetPublicInfo(Arg.Any<IEnumerable<string>>()).Returns([]);

		await _model.OnGet();

		Assert.AreEqual(1, _model.Videos.Count);
		Assert.AreEqual("", _model.Videos[0].ChannelTitle);
		_cache.DidNotReceive().Set(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<TimeSpan>());
	}

	[TestMethod]
	public async Task OnGet_OnlyNonStreamingUrls_ReturnsEmptyList()
	{
		var pub = _db.AddPublication().Entity;
		_db.AddMirrorUrl(pub, "https://www.youtube.com/watch?v=dQw4w9WgXcQ");
		await _db.SaveChangesAsync();

		await _model.OnGet();

		Assert.AreEqual(0, _model.Videos.Count);
	}

	[TestMethod]
	public void YoutubeEntry_Properties_SetCorrectly()
	{
		var entry = new YoutubeUploadersModel.YoutubeEntry(123, "Test Title", "abc123", true);

		Assert.AreEqual(123, entry.PublicationId);
		Assert.AreEqual("Test Title", entry.PublicationTitle);
		Assert.AreEqual("abc123", entry.VideoId);
		Assert.IsTrue(entry.IsObsolete);
		Assert.AreEqual("", entry.ChannelTitle);

		entry.ChannelTitle = "Updated Channel";
		Assert.AreEqual("Updated Channel", entry.ChannelTitle);
	}

	[TestMethod]
	public void RequiresPermission() => AssertHasPermission(typeof(YoutubeUploadersModel), PermissionTo.EditPublicationMetaData);
}
