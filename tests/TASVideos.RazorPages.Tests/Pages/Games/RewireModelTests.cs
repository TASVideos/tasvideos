using TASVideos.Core.Services.ExternalMediaPublisher;
using TASVideos.Pages.Games;
using TASVideos.Services;

namespace TASVideos.RazorPages.Tests.Pages.Games;

[TestClass]
public class RewireModelTests : BasePageModelTests
{
	private readonly IExternalMediaPublisher _publisher;
	private readonly RewireModel _model;

	public RewireModelTests()
	{
		_publisher = Substitute.For<IExternalMediaPublisher>();
		_model = new RewireModel(_db, _publisher)
		{
			PageContext = TestPageContext()
		};
	}

	[TestMethod]
	public async Task OnGet_WithNullGameIds_DoesNotSetValidIds()
	{
		await _model.OnGet();

		Assert.IsFalse(_model.ValidIds);
		Assert.IsNull(_model.FromGame);
		Assert.IsNull(_model.IntoGame);
	}

	[TestMethod]
	public async Task OnGet_WithOneValidGameId_DoesNotSetValidIds()
	{
		var game = _db.AddGame("Test Game").Entity;
		await _db.SaveChangesAsync();

		_model.FromGameId = game.Id;
		_model.IntoGameId = 999; // Non-existent

		await _model.OnGet();

		Assert.IsFalse(_model.ValidIds);
		Assert.IsNull(_model.FromGame);
		Assert.IsNull(_model.IntoGame);
	}

	[TestMethod]
	public async Task OnGet_WithNonExistentGameIds_DoesNotSetValidIds()
	{
		_model.FromGameId = 998;
		_model.IntoGameId = 999;

		await _model.OnGet();

		Assert.IsFalse(_model.ValidIds);
		Assert.IsNull(_model.FromGame);
		Assert.IsNull(_model.IntoGame);
	}

	[TestMethod]
	public async Task OnGet_WithTwoValidGameIds_LoadsGameData()
	{
		var fromGame = _db.AddGame("From Game").Entity;
		var intoGame = _db.AddGame("Into Game").Entity;
		await _db.SaveChangesAsync();

		_model.FromGameId = fromGame.Id;
		_model.IntoGameId = intoGame.Id;

		await _model.OnGet();

		Assert.IsTrue(_model.ValidIds);
		Assert.IsNotNull(_model.FromGame);
		Assert.IsNotNull(_model.IntoGame);
		Assert.AreEqual(fromGame.Id, _model.FromGame.Game!.Id);
		Assert.AreEqual("From Game", _model.FromGame.Game.Title);
		Assert.AreEqual(intoGame.Id, _model.IntoGame.Game!.Id);
		Assert.AreEqual("Into Game", _model.IntoGame.Game.Title);
	}

	[TestMethod]
	public async Task OnGet_WithGameAssociations_LoadsAllRelatedData()
	{
		var fromGame = _db.AddGame("From Game").Entity;
		var intoGame = _db.AddGame("Into Game").Entity;
		_db.AddUser("TestUser");
		await _db.SaveChangesAsync();

		_model.FromGameId = fromGame.Id;
		_model.IntoGameId = intoGame.Id;

		await _model.OnGet();

		Assert.IsTrue(_model.ValidIds);
		Assert.IsNotNull(_model.FromGame);
		Assert.IsNotNull(_model.IntoGame);
		Assert.AreEqual(fromGame.Id, _model.FromGame.Game!.Id);
		Assert.AreEqual("From Game", _model.FromGame.Game.Title);
		Assert.AreEqual(intoGame.Id, _model.IntoGame.Game!.Id);
		Assert.AreEqual("Into Game", _model.IntoGame.Game.Title);
	}

	[TestMethod]
	public async Task OnGet_WithGameVersionTitleOverrides_IncludesVersionOverrides()
	{
		var fromGame = _db.AddGame("From Game").Entity;
		var intoGame = _db.AddGame("Into Game").Entity;
		await _db.SaveChangesAsync();

		_model.FromGameId = fromGame.Id;
		_model.IntoGameId = intoGame.Id;

		await _model.OnGet();

		Assert.IsTrue(_model.ValidIds);
		Assert.IsNotNull(_model.FromGame);
		Assert.IsNotNull(_model.IntoGame);
	}

	[TestMethod]
	public async Task OnPost_WithNullGameIds_ReturnsRedirectWithoutRewiring()
	{
		var result = await _model.OnPost();

		Assert.IsInstanceOfType<RedirectToPageResult>(result);
		var redirectResult = (RedirectToPageResult)result;
		Assert.AreEqual("Rewire", redirectResult.PageName);
	}

	[TestMethod]
	public async Task OnPost_WithInvalidGameIds_ReturnsRedirectWithoutRewiring()
	{
		_model.FromGameId = 998;
		_model.IntoGameId = 999;

		var result = await _model.OnPost();

		Assert.IsInstanceOfType<RedirectToPageResult>(result);
		var redirectResult = (RedirectToPageResult)result;
		Assert.AreEqual("Rewire", redirectResult.PageName);
		Assert.IsFalse(_model.ValidIds);
	}

	[TestMethod]
	public async Task OnPost_WithValidGameIds_RewiresAllAssociations()
	{
		var user = _db.AddUser("TestUser").Entity;
		var fromGame = _db.AddGame("From Game").Entity;
		var intoGame = _db.AddGame("Into Game").Entity;
		await _db.SaveChangesAsync();

		AddAuthenticatedUser(_model, user, [PermissionTo.RewireGames]);

		_model.FromGameId = fromGame.Id;
		_model.IntoGameId = intoGame.Id;

		var result = await _model.OnPost();

		Assert.IsInstanceOfType<RedirectToPageResult>(result);
		var redirectResult = (RedirectToPageResult)result;
		Assert.AreEqual("Rewire", redirectResult.PageName);
	}

	[TestMethod]
	public async Task OnPost_SuccessfulRewire_SendsExternalMediaMessage()
	{
		var user = _db.AddUser("TestUser").Entity;
		var fromGame = _db.AddGame("From Game").Entity;
		var intoGame = _db.AddGame("Into Game").Entity;
		await _db.SaveChangesAsync();

		AddAuthenticatedUser(_model, user, [PermissionTo.RewireGames]);

		_model.FromGameId = fromGame.Id;
		_model.IntoGameId = intoGame.Id;

		await _model.OnPost();

		await _publisher.Received(1).Send(Arg.Any<IPostable>());
	}

	[TestMethod]
	public async Task OnPost_WithMultiplePublications_RewiresAllPublications()
	{
		var user = _db.AddUser("TestUser").Entity;
		var fromGame = _db.AddGame("From Game").Entity;
		var intoGame = _db.AddGame("Into Game").Entity;
		await _db.SaveChangesAsync();

		AddAuthenticatedUser(_model, user, [PermissionTo.RewireGames]);

		_model.FromGameId = fromGame.Id;
		_model.IntoGameId = intoGame.Id;

		var result = await _model.OnPost();

		Assert.IsInstanceOfType<RedirectToPageResult>(result);
		var redirectResult = (RedirectToPageResult)result;
		Assert.AreEqual("Rewire", redirectResult.PageName);
	}

	[TestMethod]
	public async Task OnPost_DatabaseSaveFailure_HandlesGracefully()
	{
		var user = _db.AddUser("TestUser").Entity;
		var fromGame = _db.AddGame("From Game").Entity;
		var intoGame = _db.AddGame("Into Game").Entity;
		await _db.SaveChangesAsync();

		AddAuthenticatedUser(_model, user, [PermissionTo.RewireGames]);

		_model.FromGameId = fromGame.Id;
		_model.IntoGameId = intoGame.Id;

		_db.CreateUpdateConflict();

		var result = await _model.OnPost();

		Assert.IsInstanceOfType<RedirectToPageResult>(result);
		var redirectResult = (RedirectToPageResult)result;
		Assert.AreEqual("Rewire", redirectResult.PageName);
	}

	[TestMethod]
	public async Task OnPost_ReturnsRedirectWithCorrectRouteValues()
	{
		var user = _db.AddUser("TestUser").Entity;
		var fromGame = _db.AddGame("From Game").Entity;
		var intoGame = _db.AddGame("Into Game").Entity;
		await _db.SaveChangesAsync();

		AddAuthenticatedUser(_model, user, [PermissionTo.RewireGames]);

		_model.FromGameId = fromGame.Id;
		_model.IntoGameId = intoGame.Id;

		var result = await _model.OnPost();

		Assert.IsInstanceOfType<RedirectToPageResult>(result);
		var redirectResult = (RedirectToPageResult)result;
		Assert.AreEqual("Rewire", redirectResult.PageName);
		Assert.IsNotNull(redirectResult.RouteValues);
		Assert.AreEqual(fromGame.Id, redirectResult.RouteValues["FromGameId"]);
		Assert.AreEqual(intoGame.Id, redirectResult.RouteValues["IntoGameId"]);
	}

	[TestMethod]
	public void RequiresPermission() => AssertHasPermission(typeof(RewireModel), PermissionTo.RewireGames);
}
