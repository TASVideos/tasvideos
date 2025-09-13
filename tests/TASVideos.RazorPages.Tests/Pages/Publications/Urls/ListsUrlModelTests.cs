using TASVideos.Core.Services;
using TASVideos.Core.Services.Youtube;
using TASVideos.Pages.Publications.Urls;
using TASVideos.Services;
using TASVideos.Tests.Base;

namespace TASVideos.RazorPages.Tests.Pages.Publications.Urls;

[TestClass]
public class ListUrlsModelTests : TestDbBase
{
	private readonly IPublications _publications;
	private readonly ListUrlsModel _page;

	public ListUrlsModelTests()
	{
		var publisher = Substitute.For<IExternalMediaPublisher>();
		_publications = Substitute.For<IPublications>();
		_page = new ListUrlsModel(
			_db,
			_publications,
			publisher,
			Substitute.For<IYoutubeSync>(),
			Substitute.For<IPublicationMaintenanceLogger>());
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
		var pub = _db.AddPublication().Entity;
		const string url1 = "Test1";
		const string url2 = "Test2";
		_db.PublicationUrls.Add(new PublicationUrl { PublicationId = pub.Id, Url = url1 });
		_db.PublicationUrls.Add(new PublicationUrl { PublicationId = pub.Id, Url = url2 });
		await _db.SaveChangesAsync();
		_publications.GetTitle(pub.Id).Returns(pub.Title);
		_page.PublicationId = pub.Id;

		var actual = await _page.OnGet();

		Assert.IsInstanceOfType<PageResult>(actual);
		Assert.AreEqual(pub.Title, _page.Title);
		Assert.AreEqual(2, _page.CurrentUrls.Count);
		Assert.IsTrue(_page.CurrentUrls.All(u => u.PublicationId == pub.Id));
		Assert.AreEqual(1, _page.CurrentUrls.Count(u => u.Url == url1));
		Assert.AreEqual(1, _page.CurrentUrls.Count(u => u.Url == url2));
	}
}
