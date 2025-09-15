using TASVideos.Core.Services;
using TASVideos.Pages.Games;
using TASVideos.Tests.Base;

namespace TASVideos.RazorPages.Tests.Pages.Games;

[TestClass]
public class PublicationHistoryModelTests : TestDbBase
{
	private readonly IPublicationHistory _publicationHistory;
	private readonly PublicationHistoryModel _model;

	public PublicationHistoryModelTests()
	{
		_publicationHistory = Substitute.For<IPublicationHistory>();
		_model = new PublicationHistoryModel(_publicationHistory);
	}

	[TestMethod]
	public async Task OnGet_WithNonExistentGameId_ReturnsNotFound()
	{
		const int gameId = 999;
		_model.Id = gameId;
		_publicationHistory.ForGame(gameId).Returns((PublicationHistoryGroup?)null);

		var result = await _model.OnGet();

		Assert.IsInstanceOfType<NotFoundResult>(result);
	}

	[TestMethod]
	public async Task OnGet_WithValidGameId_ReturnsPageResult()
	{
		const int gameId = 1;
		var historyGroup = new PublicationHistoryGroup();
		_model.Id = gameId;
		_publicationHistory.ForGame(gameId).Returns(historyGroup);

		var result = await _model.OnGet();

		Assert.IsInstanceOfType<PageResult>(result);
		Assert.AreEqual(historyGroup, _model.History);
	}

	[TestMethod]
	public void AllowsAnonymousAttribute() => AssertAllowsAnonymousUsers(typeof(PublicationHistoryModel));
}
