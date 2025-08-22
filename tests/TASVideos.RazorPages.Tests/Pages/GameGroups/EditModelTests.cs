using Microsoft.EntityFrameworkCore;
using TASVideos.Core.Services.ExternalMediaPublisher;
using TASVideos.Data.Entity;
using TASVideos.Data.Entity.Game;
using TASVideos.Pages;
using TASVideos.Pages.GameGroups;
using TASVideos.Services;

namespace TASVideos.RazorPages.Tests.Pages.GameGroups;

[TestClass]
public class EditModelTests : BasePageModelTests
{
	private readonly IExternalMediaPublisher _publisher;
	private readonly EditModel _model;

	public EditModelTests()
	{
		_publisher = Substitute.For<IExternalMediaPublisher>();
		_model = new EditModel(_db, _publisher)
		{
			PageContext = TestPageContext()
		};
	}

	[TestMethod]
	public async Task OnGet_NonExistentGameGroup_ReturnsNotFound()
	{
		_model.Id = 999;

		var result = await _model.OnGet();

		Assert.IsInstanceOfType(result, typeof(NotFoundResult));
	}

	[TestMethod]
	public async Task OnGet_ExistingGameGroup_LoadsGameGroupData()
	{
		var gameGroup = _db.GameGroups.Add(new GameGroup
		{
			Name = "Test Series",
			Abbreviation = "TS",
			Description = "A test game series"
		}).Entity;
		await _db.SaveChangesAsync();

		_model.Id = gameGroup.Id;

		var result = await _model.OnGet();

		Assert.IsInstanceOfType(result, typeof(PageResult));
		Assert.AreEqual("Test Series", _model.Name);
		Assert.AreEqual("TS", _model.Abbreviation);
		Assert.AreEqual("A test game series", _model.Description);
		Assert.IsTrue(_model.CanDelete);
	}

	[TestMethod]
	public async Task OnGet_GameGroupWithGames_CannotDelete()
	{
		var gameGroup = _db.AddGameGroup("Group With Games").Entity;
		var game = _db.AddGame("Test Game").Entity;
		_db.GameGameGroups.Add(new GameGameGroup { Game = game, GameGroup = gameGroup });
		await _db.SaveChangesAsync();

		_model.Id = gameGroup.Id;

		await _model.OnGet();

		Assert.IsFalse(_model.CanDelete);
	}

	[TestMethod]
	public async Task OnPost_InvalidModelState_ReturnsPage()
	{
		_model.ModelState.AddModelError("Name", "Name is required");

		var result = await _model.OnPost();

		Assert.IsInstanceOfType(result, typeof(PageResult));
	}

	[TestMethod]
	public async Task OnPost_DuplicateAbbreviation_AddsModelError()
	{
		_db.GameGroups.Add(new GameGroup { Name = "Existing Group", Abbreviation = "EG" });
		await _db.SaveChangesAsync();

		_model.Name = "New Group";
		_model.Abbreviation = "EG"; // Same as existing

		var result = await _model.OnPost();

		Assert.IsInstanceOfType(result, typeof(PageResult));
		Assert.IsTrue(_model.ModelState.ContainsKey(nameof(_model.Abbreviation)));
	}

	[TestMethod]
	public async Task OnPost_CreateNewGameGroup_CreatesGameGroup()
	{
		_model.Name = "New Game Series";
		_model.Abbreviation = "NGS";
		_model.Description = "A new game series";

		var result = await _model.OnPost();

		Assert.IsInstanceOfType(result, typeof(RedirectToPageResult));
		var redirectResult = (RedirectToPageResult)result;
		Assert.AreEqual("Index", redirectResult.PageName);

		var createdGameGroup = await _db.GameGroups.SingleOrDefaultAsync(gg => gg.Name == "New Game Series");
		Assert.IsNotNull(createdGameGroup);
		Assert.AreEqual("New Game Series", createdGameGroup.Name);
		Assert.AreEqual("NGS", createdGameGroup.Abbreviation);
		Assert.AreEqual("A new game series", createdGameGroup.Description);
	}

	[TestMethod]
	public async Task OnPost_UpdateExistingGameGroup_UpdatesAllFields()
	{
		var gameGroup = _db.GameGroups.Add(new GameGroup
		{
			Name = "Original Series",
			Abbreviation = "OS",
			Description = "Original description"
		}).Entity;
		await _db.SaveChangesAsync();

		_model.Id = gameGroup.Id;
		_model.Name = "Updated Series";
		_model.Abbreviation = "US";
		_model.Description = "Updated description";

		var result = await _model.OnPost();

		Assert.IsInstanceOfType(result, typeof(RedirectToPageResult));

		var updatedGameGroup = await _db.GameGroups.SingleAsync(gg => gg.Id == gameGroup.Id);
		Assert.AreEqual("Updated Series", updatedGameGroup.Name);
		Assert.AreEqual("US", updatedGameGroup.Abbreviation);
		Assert.AreEqual("Updated description", updatedGameGroup.Description);

		await _publisher.Received(1).Send(Arg.Any<Post>());
	}

	[TestMethod]
	public async Task OnPost_GameGroupNotFound_ReturnsNotFound()
	{
		_model.Id = 999;
		_model.Name = "Test Name";

		var result = await _model.OnPost();

		Assert.IsInstanceOfType(result, typeof(NotFoundResult));
	}

	[TestMethod]
	public async Task OnPost_DatabaseSaveFailure_HandlesGracefully()
	{
		_model.Name = "Test Group";

		_db.CreateUpdateConflict();

		var result = await _model.OnPost();

		Assert.IsInstanceOfType(result, typeof(RedirectToPageResult));
	}

	[TestMethod]
	public async Task OnPostDelete_NoId_ReturnsNotFound()
	{
		var result = await _model.OnPostDelete();

		Assert.IsInstanceOfType(result, typeof(NotFoundResult));
	}

	[TestMethod]
	public async Task OnPostDelete_GameGroupWithGames_ShowsErrorAndRedirects()
	{
		var gameGroup = _db.AddGameGroup("Group With Games").Entity;
		var game = _db.AddGame("Test Game").Entity;
		_db.GameGameGroups.Add(new GameGameGroup { Game = game, GameGroup = gameGroup });
		await _db.SaveChangesAsync();

		_model.Id = gameGroup.Id;

		var result = await _model.OnPostDelete();

		Assert.IsInstanceOfType(result, typeof(RedirectToPageResult));
		var redirectResult = (RedirectToPageResult)result;
		Assert.AreEqual("List", redirectResult.PageName);

		// Game group should still exist
		var existingGameGroup = await _db.GameGroups.SingleOrDefaultAsync(gg => gg.Id == gameGroup.Id);
		Assert.IsNotNull(existingGameGroup);
	}

	[TestMethod]
	public async Task OnPostDelete_ValidGameGroup_DeletesSuccessfully()
	{
		var gameGroup = _db.AddGameGroup("Deletable Group").Entity;
		await _db.SaveChangesAsync();
		_db.ChangeTracker.Clear();

		_model.Id = gameGroup.Id;

		var result = await _model.OnPostDelete();

		Assert.IsInstanceOfType(result, typeof(RedirectToPageResult));
		var redirectResult = (RedirectToPageResult)result;
		Assert.AreEqual("List", redirectResult.PageName);

		var deletedGameGroup = await _db.GameGroups.SingleOrDefaultAsync(gg => gg.Id == gameGroup.Id);
		Assert.IsNull(deletedGameGroup);
	}

	[TestMethod]
	public void EditModel_HasRequirePermissionAttribute()
	{
		var type = typeof(EditModel);
		var attribute = type.GetCustomAttributes(typeof(RequirePermissionAttribute), false).FirstOrDefault() as RequirePermissionAttribute;

		Assert.IsNotNull(attribute);
		Assert.IsTrue(attribute.RequiredPermissions.Contains(PermissionTo.CatalogMovies));
	}
}
