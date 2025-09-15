using TASVideos.Core.Services;
using TASVideos.Core.Services.ExternalMediaPublisher;
using TASVideos.Core.Services.Youtube;
using TASVideos.Pages.Publications.Urls;
using TASVideos.Services;
using TASVideos.Tests.Base;

namespace TASVideos.RazorPages.Tests.Pages.Publications.Urls;

[TestClass]
public class ListUrlsModelTests : TestDbBase
{
	private readonly IPublications _publications;
	private readonly IExternalMediaPublisher _publisher;
	private readonly IYoutubeSync _youtubeSync;
	private readonly IPublicationMaintenanceLogger _publicationMaintenanceLogger;
	private readonly ListUrlsModel _page;

	public ListUrlsModelTests()
	{
		_publisher = Substitute.For<IExternalMediaPublisher>();
		_publications = Substitute.For<IPublications>();
		_youtubeSync = Substitute.For<IYoutubeSync>();
		_publicationMaintenanceLogger = Substitute.For<IPublicationMaintenanceLogger>();
		_page = new ListUrlsModel(
			_publications,
			_publisher,
			_youtubeSync,
			_publicationMaintenanceLogger);
	}

	[TestMethod]
	public async Task OnGet_NoPublication_ReturnsNotFound()
	{
		_publications.GetTitle(Arg.Any<int>()).Returns((string?)null);
		var actual = await _page.OnGet();
		Assert.IsInstanceOfType<NotFoundResult>(actual);
	}

	[TestMethod]
	public async Task OnGet_Publication_ReturnsPublication()
	{
		const int pubId = 123;
		const string pubTitle = "Publication Title";
		_publications.GetTitle(pubId).Returns(pubTitle);
		const string url1 = "Test1";
		const string url2 = "Test2";
		_publications.GetUrls(pubId).Returns([
			new PublicationUrl { PublicationId = pubId, Url = url1 },
			new PublicationUrl { PublicationId = pubId, Url = url2 }
		]);
		_page.PublicationId = pubId;

		var actual = await _page.OnGet();

		Assert.IsInstanceOfType<PageResult>(actual);
		Assert.AreEqual(pubTitle, _page.Title);
		Assert.AreEqual(2, _page.CurrentUrls.Count);
		Assert.IsTrue(_page.CurrentUrls.All(u => u.PublicationId == pubId));
		Assert.AreEqual(1, _page.CurrentUrls.Count(u => u.Url == url1));
		Assert.AreEqual(1, _page.CurrentUrls.Count(u => u.Url == url2));
	}

	[TestMethod]
	public async Task OnPostDelete_UrlNotFound_ReturnsRedirectWithoutLogging()
	{
		const int publicationId = 123;
		const int nonExistentUrlId = 999;

		_page.PublicationId = publicationId;
		_publications.RemoveUrl(nonExistentUrlId).Returns((PublicationUrl?)null);

		var result = await _page.OnPostDelete(nonExistentUrlId);

		AssertRedirect(result, "List");
		var redirect = (RedirectToPageResult)result;
		Assert.AreEqual(publicationId, redirect.RouteValues!["PublicationId"]);
		await _publications.Received(1).RemoveUrl(nonExistentUrlId);
		await _publicationMaintenanceLogger.DidNotReceive().Log(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string>());
		await _publisher.DidNotReceive().Send(Arg.Any<Post>());
		await _youtubeSync.DidNotReceive().UnlistVideo(Arg.Any<string>());
	}

	[TestMethod]
	public async Task OnPostDelete_ValidUrl_DeletesUrlAndLogs()
	{
		const int publicationId = 123;
		const int urlId = 456;
		const string urlValue = "https://www.youtube.com/watch?v=test";
		const string displayName = "Test Video";
		const string userName = "TestUser";
		var userId = 789;

		var url = new PublicationUrl
		{
			Id = urlId,
			PublicationId = publicationId,
			Url = urlValue,
			DisplayName = displayName,
			Type = PublicationUrlType.Streaming
		};

		var user = _db.AddUser(userId, userName).Entity;
		AddAuthenticatedUser(_page, user, [PermissionTo.EditPublicationFiles]);

		_page.PublicationId = publicationId;
		_publications.RemoveUrl(urlId).Returns(url);

		var result = await _page.OnPostDelete(urlId);

		Assert.IsInstanceOfType<RedirectToPageResult>(result);
		var redirect = (RedirectToPageResult)result;
		Assert.AreEqual("List", redirect.PageName);
		Assert.AreEqual(publicationId, redirect.RouteValues!["PublicationId"]);
		await _publications.Received(1).RemoveUrl(urlId);
		await _publicationMaintenanceLogger.Received(1).Log(publicationId, userId, $"Deleted {displayName} {PublicationUrlType.Streaming} URL {urlValue}");
		await _publisher.Received(1).Send(Arg.Any<Post>());
		await _youtubeSync.Received(1).UnlistVideo(urlValue);
	}

	[TestMethod]
	public async Task OnPostDelete_ValidUrlWithNullDisplayName_DeletesUrlAndLogs()
	{
		const int publicationId = 123;
		const int urlId = 456;
		const string urlValue = "https://example.com/video";
		const string userName = "TestUser";
		var userId = 789;

		var url = new PublicationUrl
		{
			Id = urlId,
			PublicationId = publicationId,
			Url = urlValue,
			DisplayName = null,
			Type = PublicationUrlType.Mirror
		};

		var user = _db.AddUser(userId, userName).Entity;
		AddAuthenticatedUser(_page, user, [PermissionTo.EditPublicationFiles]);

		_page.PublicationId = publicationId;
		_publications.RemoveUrl(urlId).Returns(url);

		var result = await _page.OnPostDelete(urlId);

		Assert.IsInstanceOfType<RedirectToPageResult>(result);
		var redirect = (RedirectToPageResult)result;
		Assert.AreEqual("List", redirect.PageName);
		Assert.AreEqual(publicationId, redirect.RouteValues!["PublicationId"]);
		await _publications.Received(1).RemoveUrl(urlId);
		await _publicationMaintenanceLogger.Received(1).Log(publicationId, userId, $"Deleted  {PublicationUrlType.Mirror} URL {urlValue}");
		await _publisher.Received(1).Send(Arg.Any<Post>());
		await _youtubeSync.Received(1).UnlistVideo(urlValue);
	}
}
