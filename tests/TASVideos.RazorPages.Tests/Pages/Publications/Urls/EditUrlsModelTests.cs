using TASVideos.Core.Services;
using TASVideos.Core.Services.ExternalMediaPublisher;
using TASVideos.Core.Services.Wiki;
using TASVideos.Core.Services.Youtube;
using TASVideos.Data.Entity;
using TASVideos.Pages.Publications.Urls;
using TASVideos.Services;
using TASVideos.Tests.Base;
using static TASVideos.RazorPages.Tests.RazorTestHelpers;

namespace TASVideos.RazorPages.Tests.Pages.Publications.Urls;

[TestClass]
public class EditUrlsModelTests : TestDbBase
{
	private readonly IExternalMediaPublisher _publisher;
	private readonly IYoutubeSync _youtubeSync;
	private readonly IPublicationMaintenanceLogger _maintenanceLogger;
	private readonly IWikiPages _wikiPages;
	private readonly EditUrlsModel _model;

	public EditUrlsModelTests()
	{
		_publisher = Substitute.For<IExternalMediaPublisher>();
		_youtubeSync = Substitute.For<IYoutubeSync>();
		_maintenanceLogger = Substitute.For<IPublicationMaintenanceLogger>();
		_wikiPages = Substitute.For<IWikiPages>();
		_model = new EditUrlsModel(_db, _publisher, _youtubeSync, _maintenanceLogger, _wikiPages);

		_publisher.ToAbsolute(Arg.Any<string>()).Returns("https://test.com/test");
		_publisher.Send(Arg.Any<IPostable>(), Arg.Any<bool>()).Returns(Task.CompletedTask);

		var user = _db.AddUser("TestUser").Entity;
		AddAuthenticatedUser(_model, user, [PermissionTo.EditPublicationFiles]);
	}

	[TestMethod]
	public async Task OnGet_NonExistentPublication_ReturnsNotFound()
	{
		_model.PublicationId = 999;

		var result = await _model.OnGet();

		Assert.IsInstanceOfType<NotFoundResult>(result);
	}

	[TestMethod]
	public async Task OnGet_ValidPublicationWithoutUrlId_ReturnsPageWithBasicData()
	{
		var publication = _db.AddPublication().Entity;
		_model.PublicationId = publication.Id;

		var result = await _model.OnGet();

		Assert.IsInstanceOfType<PageResult>(result);
		Assert.AreEqual(publication.Title, _model.Title);
		Assert.AreEqual(0, _model.CurrentUrls.Count);
	}

	[TestMethod]
	public async Task OnGet_ValidPublicationWithUrlId_PopulatesUrlData()
	{
		var publication = _db.AddPublication().Entity;
		var url = new PublicationUrl
		{
			Publication = publication,
			Url = "https://www.youtube.com/watch?v=dQw4w9WgXcQ",
			Type = PublicationUrlType.Streaming,
			DisplayName = "Test Video"
		};
		_db.PublicationUrls.Add(url);
		await _db.SaveChangesAsync();

		_model.PublicationId = publication.Id;
		_model.UrlId = url.Id;

		var result = await _model.OnGet();

		Assert.IsInstanceOfType<PageResult>(result);
		Assert.AreEqual(publication.Title, _model.Title);
		Assert.AreEqual(url.Url, _model.CurrentUrl);
		Assert.AreEqual(url.Type, _model.Type);
		Assert.AreEqual(url.DisplayName, _model.AltTitle);
		Assert.AreEqual(1, _model.CurrentUrls.Count);
	}

	[TestMethod]
	public async Task OnGet_ValidPublicationWithNonExistentUrlId_ReturnsNotFound()
	{
		var publication = _db.AddPublication().Entity;
		_model.PublicationId = publication.Id;
		_model.UrlId = 999;

		var result = await _model.OnGet();

		Assert.IsInstanceOfType<NotFoundResult>(result);
	}

	[TestMethod]
	public async Task OnGet_ValidPublicationWithExistingUrls_LoadsAllUrls()
	{
		var publication = _db.AddPublication().Entity;
		var url1 = new PublicationUrl
		{
			Publication = publication,
			Url = "https://example.com/1",
			Type = PublicationUrlType.Mirror
		};
		var url2 = new PublicationUrl
		{
			Publication = publication,
			Url = "https://example.com/2",
			Type = PublicationUrlType.Streaming
		};
		_db.PublicationUrls.AddRange(url1, url2);
		await _db.SaveChangesAsync();

		_model.PublicationId = publication.Id;

		var result = await _model.OnGet();

		Assert.IsInstanceOfType<PageResult>(result);
		Assert.AreEqual(2, _model.CurrentUrls.Count);
	}

	[TestMethod]
	public async Task OnPost_NonExistentPublication_ReturnsNotFound()
	{
		_model.PublicationId = 999;
		_model.CurrentUrl = "https://www.youtube.com/watch?v=dQw4w9WgXcQ";
		_model.Type = PublicationUrlType.Mirror;

		var result = await _model.OnPost();

		Assert.IsInstanceOfType<NotFoundResult>(result);
	}

	[TestMethod]
	public async Task OnPost_DuplicateUrl_AddsModelError()
	{
		var publication = _db.AddPublication().Entity;
		var existingUrl = new PublicationUrl
		{
			Publication = publication,
			Url = "https://example.com/duplicate",
			Type = PublicationUrlType.Mirror
		};
		_db.PublicationUrls.Add(existingUrl);
		await _db.SaveChangesAsync();

		_model.PublicationId = publication.Id;
		_model.CurrentUrl = "https://example.com/duplicate";
		_model.Type = PublicationUrlType.Mirror;

		var result = await _model.OnPost();

		Assert.IsInstanceOfType<PageResult>(result);
		Assert.IsFalse(_model.ModelState.IsValid);
		Assert.IsTrue(_model.ModelState.ContainsKey("CurrentUrl"));
	}

	[TestMethod]
	public async Task OnPost_InvalidModelState_ReturnsPage()
	{
		var publication = _db.AddPublication().Entity;
		_model.PublicationId = publication.Id;
		_model.ModelState.AddModelError("CurrentUrl", "Test error");

		var result = await _model.OnPost();

		Assert.IsInstanceOfType<PageResult>(result);
		Assert.IsFalse(_model.ModelState.IsValid);
	}

	[TestMethod]
	public async Task OnPost_AddNewUrl_CreatesUrlAndRedirects()
	{
		var publication = _db.AddPublication().Entity;
		_model.PublicationId = publication.Id;
		_model.CurrentUrl = "https://example.com/new";
		_model.Type = PublicationUrlType.Mirror;
		_model.AltTitle = "New URL";

		var result = await _model.OnPost();

		Assert.IsInstanceOfType<RedirectToPageResult>(result);
		var redirect = (RedirectToPageResult)result;
		Assert.AreEqual("List", redirect.PageName);

		var addedUrl = _db.PublicationUrls.Single(u => u.PublicationId == publication.Id);
		Assert.AreEqual("https://example.com/new", addedUrl.Url);
		Assert.AreEqual(PublicationUrlType.Mirror, addedUrl.Type);
		Assert.AreEqual("New URL", addedUrl.DisplayName);

		await _maintenanceLogger.Received(1).Log(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string>());
		await _publisher.Received(1).Send(Arg.Any<IPostable>(), Arg.Any<bool>());
	}

	[TestMethod]
	public async Task OnPost_UpdateExistingUrl_UpdatesUrlAndRedirects()
	{
		var publication = _db.AddPublication().Entity;
		var existingUrl = new PublicationUrl
		{
			Publication = publication,
			Url = "https://example.com/old",
			Type = PublicationUrlType.Mirror,
			DisplayName = "Old URL"
		};
		_db.PublicationUrls.Add(existingUrl);
		await _db.SaveChangesAsync();

		_model.PublicationId = publication.Id;
		_model.UrlId = existingUrl.Id;
		_model.CurrentUrl = "https://example.com/updated";
		_model.Type = PublicationUrlType.Streaming;
		_model.AltTitle = "Updated URL";

		var result = await _model.OnPost();

		Assert.IsInstanceOfType<RedirectToPageResult>(result);

		var updatedUrl = _db.PublicationUrls.Single(u => u.Id == existingUrl.Id);
		Assert.AreEqual("https://example.com/updated", updatedUrl.Url);
		Assert.AreEqual(PublicationUrlType.Streaming, updatedUrl.Type);
		Assert.AreEqual("Updated URL", updatedUrl.DisplayName);

		await _maintenanceLogger.Received(1).Log(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string>());
		await _publisher.Received(1).Send(Arg.Any<IPostable>(), Arg.Any<bool>());
	}

	[TestMethod]
	public async Task OnPost_YouTubeStreamingUrl_TriggersYouTubeSyncWhenIsYouTube()
	{
		var publication = _db.AddPublication().Entity;
		_model.PublicationId = publication.Id;
		_model.CurrentUrl = "https://www.youtube.com/watch?v=test123";
		_model.Type = PublicationUrlType.Streaming;
		_model.AltTitle = "YouTube Video";

		_youtubeSync.IsYoutubeUrl("https://www.youtube.com/watch?v=test123").Returns(true);
		var mockWikiPage = Substitute.For<IWikiPage>();
		mockWikiPage.Markup.Returns("Test wiki content");
		_wikiPages.Page(Arg.Any<string>()).Returns(mockWikiPage);

		var result = await _model.OnPost();

		Assert.IsInstanceOfType<RedirectToPageResult>(result);
		await _youtubeSync.Received(1).SyncYouTubeVideo(Arg.Any<YoutubeVideo>());
	}

	[TestMethod]
	public async Task OnPost_YouTubeStreamingUrl_SkipsYouTubeSyncWhenNotYouTube()
	{
		var publication = _db.AddPublication().Entity;
		_model.PublicationId = publication.Id;
		_model.CurrentUrl = "https://example.com/video";
		_model.Type = PublicationUrlType.Streaming;

		_youtubeSync.IsYoutubeUrl("https://example.com/video").Returns(false);

		var result = await _model.OnPost();

		Assert.IsInstanceOfType<RedirectToPageResult>(result);
		await _youtubeSync.DidNotReceive().SyncYouTubeVideo(Arg.Any<YoutubeVideo>());
	}

	[TestMethod]
	public async Task OnPost_NonStreamingUrl_SkipsYouTubeSyncEvenIfYouTube()
	{
		var publication = _db.AddPublication().Entity;
		_model.PublicationId = publication.Id;
		_model.CurrentUrl = "https://www.youtube.com/watch?v=test123";
		_model.Type = PublicationUrlType.Mirror;

		_youtubeSync.IsYoutubeUrl("https://www.youtube.com/watch?v=test123").Returns(true);

		var result = await _model.OnPost();

		Assert.IsInstanceOfType<RedirectToPageResult>(result);
		await _youtubeSync.DidNotReceive().SyncYouTubeVideo(Arg.Any<YoutubeVideo>());
	}

	[TestMethod]
	public async Task OnPost_AllowsEditingSameUrlOnSameRecord()
	{
		var publication = _db.AddPublication().Entity;
		var existingUrl = new PublicationUrl
		{
			Publication = publication,
			Url = "https://example.com/same",
			Type = PublicationUrlType.Mirror
		};
		_db.PublicationUrls.Add(existingUrl);
		await _db.SaveChangesAsync();

		_model.PublicationId = publication.Id;
		_model.UrlId = existingUrl.Id;
		_model.CurrentUrl = "https://example.com/same";
		_model.Type = PublicationUrlType.Mirror;
		_model.AltTitle = "Updated Display Name";

		var result = await _model.OnPost();

		Assert.IsInstanceOfType<RedirectToPageResult>(result);
		Assert.IsTrue(_model.ModelState.IsValid);

		var updatedUrl = _db.PublicationUrls.Single(u => u.Id == existingUrl.Id);
		Assert.AreEqual("Updated Display Name", updatedUrl.DisplayName);
	}

	[TestMethod]
	public void AvailableTypes_ReturnsAllPublicationUrlTypes()
	{
		var availableTypes = EditUrlsModel.AvailableTypes;

		Assert.IsTrue(availableTypes.Count > 0);
		var enumValues = Enum.GetValues<PublicationUrlType>();
		Assert.AreEqual(enumValues.Length, availableTypes.Count);
	}
}
