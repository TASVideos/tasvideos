using TASVideos.Core.Services.ExternalMediaPublisher;
using TASVideos.Data.Entity.Game;
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
		var gameGroup = _db.AddGameGroup("Test Series", "TS").Entity;
		gameGroup.Description = "A test game series";
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
		_db.AttachToGroup(game, gameGroup);
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
	public async Task OnPost_GameGroupNotFound_ReturnsNotFound()
	{
		_model.Id = 999;
		var result = await _model.OnPost();
		Assert.IsInstanceOfType(result, typeof(NotFoundResult));
	}

	[TestMethod]
	public async Task OnPost_DuplicateAbbreviation_AddsModelError()
	{
		_db.AddGameGroup("Existing Group", "EG");
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

		AssertRedirect(result, "Index");
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

		AssertRedirect(result, "Index");

		var updatedGameGroup = await _db.GameGroups.SingleOrDefaultAsync(gg => gg.Id == gameGroup.Id);
		Assert.IsNotNull(updatedGameGroup);
		Assert.AreEqual("Updated Series", updatedGameGroup.Name);
		Assert.AreEqual("US", updatedGameGroup.Abbreviation);
		Assert.AreEqual("Updated description", updatedGameGroup.Description);

		await _publisher.Received(1).Send(Arg.Any<Post>());
	}

	[TestMethod]
	public async Task OnPost_DatabaseSaveFailure_HandlesGracefully()
	{
		_db.CreateUpdateConflict();
		var result = await _model.OnPost();
		AssertRedirect(result, "Index");
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
		_db.AttachToGroup(game, gameGroup);
		await _db.SaveChangesAsync();
		_model.Id = gameGroup.Id;

		var result = await _model.OnPostDelete();

		AssertRedirect(result, "List");
		Assert.IsTrue(_db.GameGroups.Any(gg => gg.Id == gameGroup.Id)); // Game group should still exist
	}

	[TestMethod]
	public async Task OnPostDelete_ValidGameGroup_DeletesSuccessfully()
	{
		var gameGroup = _db.AddGameGroup("Deletable Group").Entity;
		await _db.SaveChangesAsync();
		_db.ChangeTracker.Clear();
		_model.Id = gameGroup.Id;

		var result = await _model.OnPostDelete();

		AssertRedirect(result, "List");
		Assert.IsFalse(_db.GameGroups.Any(gg => gg.Id == gameGroup.Id));
	}

	[TestMethod]
	public void RequiresPermission() => AssertHasPermission(typeof(EditModel), PermissionTo.CatalogMovies);
}
