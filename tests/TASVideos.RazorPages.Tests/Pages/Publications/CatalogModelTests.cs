using TASVideos.Core.Services.ExternalMediaPublisher;
using TASVideos.Pages.Publications;
using TASVideos.Services;
using TASVideos.Tests.Base;

namespace TASVideos.RazorPages.Tests.Pages.Publications;

[TestClass]
public class CatalogModelTests : TestDbBase
{
	private readonly IExternalMediaPublisher _publisher;
	private readonly CatalogModel _page;

	public CatalogModelTests()
	{
		_publisher = Substitute.For<IExternalMediaPublisher>();
		_page = new CatalogModel(_db, _publisher);
	}

	[TestMethod]
	public async Task OnGet_PublicationNotFound_ReturnsNotFound()
	{
		_page.Id = 999;
		var result = await _page.OnGet();
		Assert.IsInstanceOfType<NotFoundResult>(result);
	}

	[TestMethod]
	public async Task OnGet_PublicationExists_PopulatesCatalogData()
	{
		var publication = _db.AddPublication().Entity;
		_page.Id = publication.Id;

		var result = await _page.OnGet();

		Assert.IsInstanceOfType<PageResult>(result);
		Assert.AreEqual(publication.Title, _page.Catalog.Title);
		Assert.AreEqual(publication.GameId, _page.Catalog.Game);
		Assert.AreEqual(publication.SystemId, _page.Catalog.System);
		Assert.AreEqual(publication.SystemFrameRateId, _page.Catalog.SystemFramerate);
		Assert.AreEqual(publication.GameVersionId, _page.Catalog.GameVersion);
		Assert.AreEqual(publication.GameGoalId, _page.Catalog.Goal);
		Assert.AreEqual(publication.EmulatorVersion, _page.Catalog.Emulator);
	}

	[TestMethod]
	public async Task OnGet_WithQueryParameters_OverridesCatalogData()
	{
		var publication1 = _db.AddPublication().Entity;
		var publication2 = _db.AddPublication().Entity; // Sneaky way to create new game/system records

		_page.Id = publication1.Id;
		_page.SystemId = publication2.SystemId;
		_page.GameId = publication2.GameId;
		_page.GameVersionId = publication2.GameVersionId;
		_page.GameGoalId = publication2.GameGoalId;

		var result = await _page.OnGet();

		Assert.IsInstanceOfType<PageResult>(result);
		Assert.AreEqual(publication2.SystemId, _page.Catalog.System);
		Assert.AreEqual(publication2.GameId, _page.Catalog.Game);
		Assert.AreEqual(publication2.GameVersionId, _page.Catalog.GameVersion);
		Assert.AreEqual(publication2.GameGoalId, _page.Catalog.Goal);
	}

	[TestMethod]
	public async Task OnPost_InvalidModelState_ReturnsPage()
	{
		var publication = _db.AddPublication().Entity;
		_page.Id = publication.Id;
		_page.Catalog = new CatalogModel.PublicationCatalog
		{
			System = 999,
			SystemFramerate = publication.SystemFrameRateId,
			Emulator = ""
		};

		var result = await _page.OnPost();

		Assert.IsInstanceOfType<PageResult>(result);
		Assert.IsFalse(_page.ModelState.IsValid);
	}

	[TestMethod]
	public async Task OnPost_PublicationNotFound_ReturnsNotFound()
	{
		_page.Id = 999;
		var result = await _page.OnPost();
		Assert.IsInstanceOfType<NotFoundResult>(result);
	}

	[TestMethod]
	public async Task OnPost_ValidUpdate_UpdatesPublicationAndRedirects()
	{
		var publication = _db.AddPublication().Entity;
		var publication2 = _db.AddPublication().Entity; // Sneaky way to create new game/system records

		// Sketchy asserts to ensure creating a 2nd pub will actually create different game/systems
		Assert.AreNotEqual(publication2.SystemId, publication.SystemId);
		Assert.AreNotEqual(publication2.SystemFrameRateId, publication.SystemFrameRateId);
		Assert.AreNotEqual(publication2.GameId, publication.GameId);
		Assert.AreNotEqual(publication2.GameVersionId, publication.GameVersionId);
		Assert.AreNotEqual(publication2.GameGoalId, publication.GameGoalId);

		_page.Id = publication.Id;
		_page.Catalog = new CatalogModel.PublicationCatalog
		{
			Game = publication2.GameId,
			System = publication2.SystemId,
			SystemFramerate = publication2.SystemFrameRateId,
			GameVersion = publication2.GameVersionId,
			Goal = publication2.GameGoalId!.Value,
			Emulator = "New Emulator"
		};

		var result = await _page.OnPost();

		Assert.IsInstanceOfType<RedirectToPageResult>(result);
		var redirectResult = (RedirectToPageResult)result;
		Assert.AreEqual("View", redirectResult.PageName);

		var updatedPublication = await _db.Publications.FindAsync(publication.Id);
		Assert.IsNotNull(updatedPublication);
		Assert.AreEqual("New Emulator", updatedPublication.EmulatorVersion);
		Assert.AreEqual(publication2.SystemId, updatedPublication.SystemId);
		Assert.AreEqual(publication2.SystemFrameRateId, updatedPublication.SystemFrameRateId);
		Assert.AreEqual(publication2.GameId, updatedPublication.GameId);
		Assert.AreEqual(publication2.GameVersionId, updatedPublication.GameVersionId);
		Assert.AreEqual(publication2.GameGoalId, updatedPublication.GameGoalId);
		await _publisher.Received(1).Send(Arg.Any<Post>());
	}

	[TestMethod]
	public async Task OnPost_NoChanges_RedirectsWithoutSendingNotification()
	{
		var publication = _db.AddPublication().Entity;
		_page.Id = publication.Id;
		_page.Catalog = new CatalogModel.PublicationCatalog
		{
			Game = publication.GameId,
			System = publication.SystemId,
			SystemFramerate = publication.SystemFrameRateId,
			GameVersion = publication.GameVersionId,
			Goal = publication.GameGoalId!.Value,
			Emulator = publication.EmulatorVersion
		};

		var result = await _page.OnPost();

		Assert.IsInstanceOfType<RedirectToPageResult>(result);
		await _publisher.Received(1).Send(Arg.Any<Post>());
	}
}
