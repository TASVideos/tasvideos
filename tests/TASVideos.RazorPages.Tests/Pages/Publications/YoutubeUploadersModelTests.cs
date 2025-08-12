using TASVideos.Core.Services;
using TASVideos.Core.Services.Youtube;
using TASVideos.Core.Services.Youtube.Dtos;
using TASVideos.Data.Entity;
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
		var publication = _db.AddPublication().Entity;
		var publicationUrl = new PublicationUrl
		{
			Publication = publication,
			Url = "https://example.com/video",
			Type = PublicationUrlType.Streaming
		};
		_db.PublicationUrls.Add(publicationUrl);
		await _db.SaveChangesAsync();
		_youtubeSync.IsYoutubeUrl("https://example.com/video").Returns(false);

		await _model.OnGet();

		Assert.AreEqual(0, _model.Videos.Count);
	}

	[TestMethod]
	public async Task OnGet_YouTubeUrlsWithCachedChannelTitles_UsesCache()
	{
		var publication = _db.AddPublication().Entity;

		var publicationUrl = new PublicationUrl
		{
			Publication = publication,
			Url = "https://www.youtube.com/watch?v=dQw4w9WgXcQ",
			Type = PublicationUrlType.Streaming
		};
		_db.PublicationUrls.Add(publicationUrl);
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
		var publication = _db.AddPublication().Entity;

		var publicationUrl = new PublicationUrl
		{
			Publication = publication,
			Url = "https://www.youtube.com/watch?v=dQw4w9WgXcQ",
			Type = PublicationUrlType.Streaming
		};
		_db.PublicationUrls.Add(publicationUrl);
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
		var publication1 = _db.AddPublication().Entity;
		var publication2 = _db.AddPublication().Entity;

		var publicationUrl1 = new PublicationUrl
		{
			Publication = publication1,
			Url = "https://www.youtube.com/watch?v=dQw4w9WgXcQ",
			Type = PublicationUrlType.Streaming
		};
		var publicationUrl2 = new PublicationUrl
		{
			Publication = publication2,
			Url = "https://www.youtube.com/watch?v=sVR32jXj68w",
			Type = PublicationUrlType.Streaming
		};
		_db.PublicationUrls.AddRange(publicationUrl1, publicationUrl2);
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
		var obsoletePublication = _db.AddPublication().Entity;
		var currentPublication = _db.AddPublication().Entity;
		obsoletePublication.ObsoletedById = currentPublication.Id;

		var publicationUrl1 = new PublicationUrl
		{
			Publication = obsoletePublication,
			Url = "https://www.youtube.com/watch?v=obsolete123",
			Type = PublicationUrlType.Streaming
		};
		var publicationUrl2 = new PublicationUrl
		{
			Publication = currentPublication,
			Url = "https://www.youtube.com/watch?v=current456",
			Type = PublicationUrlType.Streaming
		};
		_db.PublicationUrls.AddRange(publicationUrl1, publicationUrl2);
		await _db.SaveChangesAsync();

		_youtubeSync.IsYoutubeUrl(Arg.Any<string>()).Returns(true);
		_youtubeSync.VideoId("https://www.youtube.com/watch?v=obsolete123").Returns("obsolete123");
		_youtubeSync.VideoId("https://www.youtube.com/watch?v=current456").Returns("current456");
		_cache.TryGetValue(Arg.Any<string>(), out Arg.Any<string>()).Returns(false);
		_youtubeSync.GetPublicInfo(Arg.Any<IEnumerable<string>>()).Returns([]);

		await _model.OnGet();

		Assert.AreEqual(2, _model.Videos.Count);
		var obsoleteVideo = _model.Videos.First(v => v.PublicationId == obsoletePublication.Id);
		var currentVideo = _model.Videos.First(v => v.PublicationId == currentPublication.Id);

		Assert.IsTrue(obsoleteVideo.IsObsolete);
		Assert.IsFalse(currentVideo.IsObsolete);
	}

	[TestMethod]
	public async Task OnGet_DuplicateVideoIds_ReturnsDistinctResults()
	{
		var publication = _db.AddPublication().Entity;

		var publicationUrl1 = new PublicationUrl
		{
			Publication = publication,
			Url = "https://www.youtube.com/watch?v=dQw4w9WgXcQ",
			Type = PublicationUrlType.Streaming
		};
		var publicationUrl2 = new PublicationUrl
		{
			Publication = publication,
			Url = "https://youtu.be/dQw4w9WgXcQ",
			Type = PublicationUrlType.Streaming
		};
		_db.PublicationUrls.AddRange(publicationUrl1, publicationUrl2);
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
		var publication = _db.AddPublication().Entity;

		var publicationUrl = new PublicationUrl
		{
			Publication = publication,
			Url = "https://www.youtube.com/watch?v=dQw4w9WgXcQ",
			Type = PublicationUrlType.Streaming
		};
		_db.PublicationUrls.Add(publicationUrl);
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
		var publication = _db.AddPublication().Entity;

		var publicationUrl = new PublicationUrl
		{
			Publication = publication,
			Url = "https://www.youtube.com/watch?v=dQw4w9WgXcQ",
			Type = PublicationUrlType.Mirror
		};
		_db.PublicationUrls.Add(publicationUrl);
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
}
