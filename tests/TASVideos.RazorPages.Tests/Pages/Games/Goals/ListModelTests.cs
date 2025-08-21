using Microsoft.EntityFrameworkCore;
using TASVideos.Data.Entity;
using TASVideos.Pages.Games.Goals;
using static TASVideos.RazorPages.Tests.RazorTestHelpers;

namespace TASVideos.RazorPages.Tests.Pages.Games.Goals;

[TestClass]
public class ListModelTests : BasePageModelTests
{
	private readonly ListModel _model;

	public ListModelTests()
	{
		_model = new ListModel(_db)
		{
			PageContext = TestPageContext()
		};
	}

	#region OnGet Tests

	[TestMethod]
	public async Task OnGet_GameExists_ReturnsPageWithGameAndGoals()
	{
		var game = _db.AddGame("Test Game").Entity;
		_db.AddGoalForGame(game, "baseline");
		_db.AddGoalForGame(game, "any%");
		await _db.SaveChangesAsync();

		_model.GameId = game.Id;

		var result = await _model.OnGet();

		Assert.IsInstanceOfType(result, typeof(PageResult));
		Assert.AreEqual("Test Game", _model.Game);
		Assert.AreEqual(2, _model.Goals.Count);
		Assert.IsTrue(_model.Goals.Any(g => g.Name == "baseline"));
		Assert.IsTrue(_model.Goals.Any(g => g.Name == "any%"));
	}

	[TestMethod]
	public async Task OnGet_GameDoesNotExist_ReturnsNotFound()
	{
		_model.GameId = 999;

		var result = await _model.OnGet();

		Assert.IsInstanceOfType(result, typeof(NotFoundResult));
	}

	#endregion

	#region OnPost Tests

	[TestMethod]
	public async Task OnPost_UserWithoutPermission_ReturnsAccessDenied()
	{
		var user = _db.AddUser("TestUser").Entity;
		AddAuthenticatedUser(_model, user, []);

		var result = await _model.OnPost("new goal");

		Assert.IsInstanceOfType(result, typeof(RedirectToPageResult));
		var redirect = (RedirectToPageResult)result;
		Assert.AreEqual("/Account/AccessDenied", redirect.PageName);
	}

	[TestMethod]
	public async Task OnPost_EmptyGoalName_ReturnsErrorAndBackToList()
	{
		var user = _db.AddUser("TestUser").Entity;
		AddAuthenticatedUser(_model, user, [PermissionTo.CatalogMovies]);
		var game = _db.AddGame("Test Game").Entity;
		await _db.SaveChangesAsync();
		_model.GameId = game.Id;

		var result = await _model.OnPost("");

		Assert.IsInstanceOfType(result, typeof(RedirectToPageResult));
		var redirect = (RedirectToPageResult)result;
		Assert.AreEqual("List", redirect.PageName);
		Assert.IsTrue(_model.Message!.Contains("Cannot create empty goal"));
	}

	[TestMethod]
	public async Task OnPost_NullGoalName_ReturnsErrorAndBackToList()
	{
		var user = _db.AddUser("TestUser").Entity;
		AddAuthenticatedUser(_model, user, [PermissionTo.CatalogMovies]);
		var game = _db.AddGame("Test Game").Entity;
		await _db.SaveChangesAsync(); // Save the game first
		_model.GameId = game.Id;

		var result = await _model.OnPost(null);

		Assert.IsInstanceOfType(result, typeof(RedirectToPageResult));
		var redirect = (RedirectToPageResult)result;
		Assert.AreEqual("List", redirect.PageName);
		Assert.IsTrue(_model.Message!.Contains("Cannot create empty goal"));
	}

	[TestMethod]
	public async Task OnPost_DuplicateGoalName_ReturnsErrorAndBackToList()
	{
		var user = _db.AddUser("TestUser").Entity;
		AddAuthenticatedUser(_model, user, [PermissionTo.CatalogMovies]);
		var game = _db.AddGame("Test Game").Entity;
		_db.AddGoalForGame(game, "existing goal");
		await _db.SaveChangesAsync();

		_model.GameId = game.Id;

		var result = await _model.OnPost("existing goal");

		Assert.IsInstanceOfType(result, typeof(RedirectToPageResult));
		var redirect = (RedirectToPageResult)result;
		Assert.AreEqual("List", redirect.PageName);
		Assert.IsTrue(_model.Message!.Contains("Cannot create goal existing goal because it already exists"));
	}

	[TestMethod]
	public async Task OnPost_ValidGoalName_CreatesGoalAndRedirectsToList()
	{
		var user = _db.AddUser("TestUser").Entity;
		AddAuthenticatedUser(_model, user, [PermissionTo.CatalogMovies]);
		var game = _db.AddGame("Test Game").Entity;
		await _db.SaveChangesAsync();
		_model.GameId = game.Id;

		var result = await _model.OnPost("new goal");

		Assert.IsInstanceOfType(result, typeof(RedirectToPageResult));
		var redirect = (RedirectToPageResult)result;
		Assert.AreEqual("List", redirect.PageName);
		Assert.IsNotNull(redirect.RouteValues);
		Assert.AreEqual(game.Id, redirect.RouteValues["GameId"]);

		var createdGoal = await _db.GameGoals.SingleOrDefaultAsync(gg => gg.DisplayName == "new goal");
		Assert.IsNotNull(createdGoal);
		Assert.AreEqual(game.Id, createdGoal.GameId);
		Assert.IsTrue(_model.Message!.Contains("Goal new goal created successfully"));
	}

	#endregion

	#region OnPostEdit Tests

	[TestMethod]
	public async Task OnPostEdit_UserWithoutPermission_ReturnsAccessDenied()
	{
		var user = _db.AddUser("TestUser").Entity;
		AddAuthenticatedUser(_model, user, []);

		var result = await _model.OnPostEdit(1, "new name");

		Assert.IsInstanceOfType(result, typeof(RedirectToPageResult));
		var redirect = (RedirectToPageResult)result;
		Assert.AreEqual("/Account/AccessDenied", redirect.PageName);
	}

	[TestMethod]
	public async Task OnPostEdit_EmptyNewGoalName_ReturnsErrorAndBackToList()
	{
		var user = _db.AddUser("TestUser").Entity;
		AddAuthenticatedUser(_model, user, [PermissionTo.CatalogMovies]);
		var game = _db.AddGame("Test Game").Entity;
		await _db.SaveChangesAsync();
		_model.GameId = game.Id;

		var result = await _model.OnPostEdit(1, "");

		Assert.IsInstanceOfType(result, typeof(RedirectToPageResult));
		Assert.IsTrue(_model.Message!.Contains("empty goal"));
	}

	[TestMethod]
	public async Task OnPostEdit_GameGoalNotFound_ReturnsNotFound()
	{
		var user = _db.AddUser("TestUser").Entity;
		AddAuthenticatedUser(_model, user, [PermissionTo.CatalogMovies]);

		var result = await _model.OnPostEdit(999, "new name");

		Assert.IsInstanceOfType(result, typeof(NotFoundResult));
	}

	[TestMethod]
	public async Task OnPostEdit_BaselineGoal_ReturnsErrorAndBackToList()
	{
		var user = _db.AddUser("TestUser").Entity;
		AddAuthenticatedUser(_model, user, [PermissionTo.CatalogMovies]);
		var game = _db.AddGame("Test Game").Entity;
		var baselineGoal = _db.AddGoalForGame(game).Entity;
		await _db.SaveChangesAsync();

		_model.GameId = game.Id;

		var result = await _model.OnPostEdit(baselineGoal.Id, "new name");

		Assert.IsInstanceOfType(result, typeof(RedirectToPageResult));
		Assert.AreEqual("danger", _model.MessageType);
	}

	[TestMethod]
	public async Task OnPostEdit_DuplicateNewGoalName_ReturnsErrorAndBackToList()
	{
		var user = _db.AddUser("TestUser").Entity;
		AddAuthenticatedUser(_model, user, [PermissionTo.CatalogMovies]);
		var game = _db.AddGame("Test Game").Entity;
		var goal1 = _db.AddGoalForGame(game, "goal1").Entity;
		_db.AddGoalForGame(game, "goal2");
		await _db.SaveChangesAsync();

		_model.GameId = game.Id;

		var result = await _model.OnPostEdit(goal1.Id, "goal2");

		Assert.IsInstanceOfType(result, typeof(RedirectToPageResult));
		Assert.AreEqual("danger", _model.MessageType);
	}

	[TestMethod]
	public async Task OnPostEdit_ValidNewGoalName_UpdatesGoalAndRedirectsToList()
	{
		var user = _db.AddUser("TestUser").Entity;
		AddAuthenticatedUser(_model, user, [PermissionTo.CatalogMovies]);
		var game = _db.AddGame("Test Game").Entity;
		var goal = _db.AddGoalForGame(game, "old name").Entity;
		await _db.SaveChangesAsync();

		_model.GameId = game.Id;

		var result = await _model.OnPostEdit(goal.Id, "new name");

		Assert.IsInstanceOfType(result, typeof(RedirectToPageResult));
		var redirect = (RedirectToPageResult)result;
		Assert.AreEqual("List", redirect.PageName);

		var updatedGoal = await _db.GameGoals.FindAsync(goal.Id);
		Assert.AreEqual("new name", updatedGoal!.DisplayName);
		Assert.AreEqual("success", _model.MessageType);
	}

	[TestMethod]
	public async Task OnPostEdit_NonBaselineGoalWithPublicationsAndSubmissions_UpdatesTitles()
	{
		var user = _db.AddUser("TestUser").Entity;
		AddAuthenticatedUser(_model, user, [PermissionTo.CatalogMovies]);
		var game = _db.AddGame("Test Game").Entity;
		await _db.SaveChangesAsync(); // Save the game first
		var goal = _db.AddGoalForGame(game, "old name").Entity;
		await _db.SaveChangesAsync();

		var pub = _db.AddPublication().Entity;
		pub.Title = "Old Title";
		pub.GameGoalId = goal.Id;

		var sub = _db.AddSubmission().Entity;
		sub.Title = "Old Submission Title";
		sub.GameGoalId = goal.Id;
		await _db.SaveChangesAsync();

		_model.GameId = game.Id;

		var result = await _model.OnPostEdit(goal.Id, "new goal");

		Assert.IsInstanceOfType(result, typeof(RedirectToPageResult));

		var updatedGoal = await _db.GameGoals.FindAsync(goal.Id);
		Assert.IsNotNull(updatedGoal);
		Assert.AreEqual("new goal", updatedGoal.DisplayName);
		Assert.IsTrue(sub.Title.Contains(goal.DisplayName));
		Assert.IsTrue(pub.Title.Contains(goal.DisplayName));
		Assert.AreEqual("success", _model.MessageType);
	}

	#endregion

	#region OnGetDelete Tests

	[TestMethod]
	public async Task OnGetDelete_UserWithoutPermission_ReturnsAccessDenied()
	{
		var user = _db.AddUser("TestUser").Entity;
		AddAuthenticatedUser(_model, user, []);

		var result = await _model.OnGetDelete(1);

		Assert.IsInstanceOfType(result, typeof(RedirectToPageResult));
		var redirect = (RedirectToPageResult)result;
		Assert.AreEqual("/Account/AccessDenied", redirect.PageName);
	}

	[TestMethod]
	public async Task OnGetDelete_GameGoalNotFound_ReturnsNotFound()
	{
		var user = _db.AddUser("TestUser").Entity;
		AddAuthenticatedUser(_model, user, [PermissionTo.CatalogMovies]);

		var result = await _model.OnGetDelete(999);

		Assert.IsInstanceOfType(result, typeof(NotFoundResult));
	}

	[TestMethod]
	public async Task OnGetDelete_BaselineGoal_ReturnsErrorAndBackToList()
	{
		var user = _db.AddUser("TestUser").Entity;
		AddAuthenticatedUser(_model, user, [PermissionTo.CatalogMovies]);
		var game = _db.AddGame("Test Game").Entity;
		var baselineGoal = _db.AddGoalForGame(game).Entity;
		await _db.SaveChangesAsync();

		_model.GameId = game.Id;

		var result = await _model.OnGetDelete(baselineGoal.Id);

		Assert.IsInstanceOfType(result, typeof(RedirectToPageResult));
		Assert.IsTrue(_model.Message!.Contains("Cannot delete baseline goal"));
	}

	[TestMethod]
	public async Task OnGetDelete_ValidGoalWithoutAssociations_DeletesGoalAndRedirectsToList()
	{
		var user = _db.AddUser("TestUser").Entity;
		AddAuthenticatedUser(_model, user, [PermissionTo.CatalogMovies]);
		var game = _db.AddGame("Test Game").Entity;
		var goal = _db.AddGoalForGame(game, "test goal").Entity;
		await _db.SaveChangesAsync();

		_model.GameId = game.Id;
		var goalId = goal.Id;

		var result = await _model.OnGetDelete(goalId);

		Assert.IsInstanceOfType(result, typeof(RedirectToPageResult));
		var redirect = (RedirectToPageResult)result;
		Assert.AreEqual("List", redirect.PageName);

		var deletedGoal = await _db.GameGoals.FindAsync(goalId);
		Assert.IsNull(deletedGoal);
		Assert.AreEqual("success", _model.MessageType);
	}

	#endregion
}
