using TASVideos.Core.Services;
using TASVideos.Core.Services.ExternalMediaPublisher;
using TASVideos.Core.Services.Wiki;
using TASVideos.Core.Services.Youtube;
using TASVideos.Pages.Publications.Urls;
using TASVideos.Services;
using TASVideos.Tests.Base;

namespace TASVideos.RazorPages.Tests.Pages.Publications.Urls;

[TestClass]
public class EditUrlsModelTests : TestDbBase
{
	private readonly IPublications _publications;
	private readonly IExternalMediaPublisher _publisher;
	private readonly IYoutubeSync _youtubeSync;
	private readonly IPublicationMaintenanceLogger _maintenanceLogger;
	private readonly IWikiPages _wikiPages;
	private readonly EditUrlsModel _model;

	public EditUrlsModelTests()
	{
		_publications = Substitute.For<IPublications>();
		_publisher = Substitute.For<IExternalMediaPublisher>();
		_youtubeSync = Substitute.For<IYoutubeSync>();
		_maintenanceLogger = Substitute.For<IPublicationMaintenanceLogger>();
		_wikiPages = Substitute.For<IWikiPages>();
		_model = new EditUrlsModel(_db, _publications, _publisher, _youtubeSync, _maintenanceLogger, _wikiPages);

		_publisher.ToAbsolute(Arg.Any<string>()).Returns("https://test.com/test");
		_publisher.Send(Arg.Any<IPostable>(), Arg.Any<bool>()).Returns(Task.CompletedTask);

		var user = _db.AddUser("TestUser").Entity;
		AddAuthenticatedUser(_model, user, [PermissionTo.EditPublicationFiles]);
	}

	[TestMethod]
	public async Task OnGet_NonExistentPublication_ReturnsNotFound()
	{
		_model.PublicationId = 999;
		_publications.GetTitle(Arg.Any<int>()).Returns((string?)null);
		var result = await _model.OnGet();
		Assert.IsInstanceOfType<NotFoundResult>(result);
	}

	[TestMethod]
	public async Task OnGet_ValidPublicationWithoutUrlId_ReturnsPageWithBasicData()
	{
		var pub = _db.AddPublication().Entity;
		_publications.GetTitle(pub.Id).Returns(pub.Title);
		_model.PublicationId = pub.Id;

		var result = await _model.OnGet();

		Assert.IsInstanceOfType<PageResult>(result);
		Assert.AreEqual(pub.Title, _model.Title);
		Assert.AreEqual(0, _model.CurrentUrls.Count);
	}

	[TestMethod]
	public async Task OnGet_ValidPublicationWithUrlId_PopulatesUrlData()
	{
		const int pubId = 123;
		const string pubTitle = "Test TItle";
		var url = new PublicationUrl
		{
			Id = 456,
			PublicationId = pubId,
			Url = "https://example.com/video",
			Type = PublicationUrlType.Streaming,
			DisplayName = "Test Video"
		};

		_publications.GetTitle(pubId).Returns(pubTitle);
		_publications.GetUrls(pubId).Returns([url]);

		_model.PublicationId = pubId;
		_model.UrlId = url.Id;

		var result = await _model.OnGet();

		Assert.IsInstanceOfType<PageResult>(result);
		Assert.AreEqual(pubTitle, _model.Title);
		Assert.AreEqual(url.Url, _model.CurrentUrl);
		Assert.AreEqual(url.Type, _model.Type);
		Assert.AreEqual(url.DisplayName, _model.AltTitle);
		Assert.AreEqual(1, _model.CurrentUrls.Count);
	}

	[TestMethod]
	public async Task OnGet_ValidPublicationWithNonExistentUrlId_ReturnsNotFound()
	{
		var pub = _db.AddPublication().Entity;
		_model.PublicationId = pub.Id;
		_model.UrlId = 999;

		var result = await _model.OnGet();

		Assert.IsInstanceOfType<NotFoundResult>(result);
	}

	[TestMethod]
	public async Task OnGet_ValidPublicationWithExistingUrls_LoadsAllUrls()
	{
		const int pubId = 123;
		_publications.GetUrls(pubId).Returns([
			new PublicationUrl { Url = "https://example.com/1" },
			new PublicationUrl { Url = "https://example.com/2" }
		]);
		_model.PublicationId = pubId;

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
		var pub = _db.AddPublication().Entity;
		_db.AddMirrorUrl(pub, "https://example.com/duplicate");
		await _db.SaveChangesAsync();

		_model.PublicationId = pub.Id;
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
		var pub = _db.AddPublication().Entity;
		_model.PublicationId = pub.Id;
		_model.ModelState.AddModelError("CurrentUrl", "Test error");

		var result = await _model.OnPost();

		Assert.IsInstanceOfType<PageResult>(result);
		Assert.IsFalse(_model.ModelState.IsValid);
	}

	[TestMethod]
	public async Task OnPost_AddNewUrl_CreatesUrlAndRedirects()
	{
		var pub = _db.AddPublication().Entity;
		_model.PublicationId = pub.Id;
		_model.CurrentUrl = "https://example.com/new";
		_model.Type = PublicationUrlType.Mirror;
		_model.AltTitle = "New URL";

		var result = await _model.OnPost();

		Assert.IsInstanceOfType<RedirectToPageResult>(result);
		var redirect = (RedirectToPageResult)result;
		Assert.AreEqual("List", redirect.PageName);

		var addedUrl = _db.PublicationUrls.Single(u => u.PublicationId == pub.Id);
		Assert.AreEqual("https://example.com/new", addedUrl.Url);
		Assert.AreEqual(PublicationUrlType.Mirror, addedUrl.Type);
		Assert.AreEqual("New URL", addedUrl.DisplayName);

		await _maintenanceLogger.Received(1).Log(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string>());
		await _publisher.Received(1).Send(Arg.Any<IPostable>(), Arg.Any<bool>());
	}

	[TestMethod]
	public async Task OnPost_UpdateExistingUrl_UpdatesUrlAndRedirects()
	{
		var pub = _db.AddPublication().Entity;
		var existingUrl = _db.AddMirrorUrl(pub, "https://example.com/old").Entity;
		existingUrl.DisplayName = "Old URL";
		await _db.SaveChangesAsync();

		_model.PublicationId = pub.Id;
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
		var pub = _db.AddPublication().Entity;
		_model.PublicationId = pub.Id;
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
		var pub = _db.AddPublication().Entity;
		_model.PublicationId = pub.Id;
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
		var pub = _db.AddPublication().Entity;
		_model.PublicationId = pub.Id;
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
		var pub = _db.AddPublication().Entity;
		var existingUrl = _db.AddMirrorUrl(pub, "https://example.com/same").Entity;
		await _db.SaveChangesAsync();

		_model.PublicationId = pub.Id;
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
