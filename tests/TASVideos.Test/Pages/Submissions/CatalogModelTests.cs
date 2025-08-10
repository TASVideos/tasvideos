using TASVideos.Core.Services.ExternalMediaPublisher;
using TASVideos.Data.Entity;
using TASVideos.Data.Entity.Game;
using TASVideos.Pages.Submissions;
using TASVideos.Services;
using TASVideos.Tests.Base;
using static TASVideos.RazorPages.Tests.RazorTestHelpers;

namespace TASVideos.RazorPages.Tests.Pages.Submissions;

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
	public async Task OnGet_SubmissionNotFound_ReturnsNotFound()
	{
		_page.Id = 999;

		var result = await _page.OnGet();

		Assert.IsInstanceOfType<NotFoundResult>(result);
	}

	[TestMethod]
	public async Task OnGet_SubmissionExists_PopulatesCatalogData()
	{
		var system = new GameSystem { Id = 1, Code = "NES", DisplayName = "Nintendo Entertainment System" };
		_db.GameSystems.Add(system);
		var frameRate = new GameSystemFrameRate { Id = 1, GameSystemId = system.Id, FrameRate = 60.0988 };
		_db.GameSystemFrameRates.Add(frameRate);
		var game = new Game { Id = 1, DisplayName = "Test Game" };
		_db.Games.Add(game);
		var version = new GameVersion { Id = 1, Name = "1.0", SystemId = system.Id, GameId = game.Id };
		_db.GameVersions.Add(version);
		var goal = new GameGoal { Id = 1, DisplayName = "Test Goal", GameId = game.Id };
		_db.GameGoals.Add(goal);
		var user = _db.AddUser("TestUser").Entity;
		await _db.SaveChangesAsync();

		var submission = new Submission
		{
			Title = "Test Submission",
			GameId = game.Id,
			SystemId = system.Id,
			SystemFrameRateId = frameRate.Id,
			GameVersionId = version.Id,
			GameGoalId = goal.Id,
			EmulatorVersion = "Test Emulator",
			SyncedOn = DateTime.UtcNow,
			SyncedByUserId = user.Id,
			AdditionalSyncNotes = "Test sync notes",
			Submitter = user,
			Status = SubmissionStatus.New
		};

		_db.Submissions.Add(submission);
		await _db.SaveChangesAsync();

		_page.Id = submission.Id;

		var result = await _page.OnGet();

		Assert.IsInstanceOfType<PageResult>(result);
		Assert.AreEqual("Test Submission", _page.Catalog.Title);
		Assert.AreEqual(game.Id, _page.Catalog.Game);
		Assert.AreEqual(system.Id, _page.Catalog.System);
		Assert.AreEqual(frameRate.Id, _page.Catalog.SystemFramerate);
		Assert.AreEqual(version.Id, _page.Catalog.GameVersion);
		Assert.AreEqual(goal.Id, _page.Catalog.Goal);
		Assert.AreEqual("Test Emulator", _page.Catalog.Emulator);
		Assert.IsTrue(_page.Catalog.SyncVerified);
		Assert.IsNotNull(_page.Catalog.SyncVerifiedOn);
		Assert.AreEqual(user.UserName, _page.Catalog.SyncedBy);
		Assert.AreEqual("Test sync notes", _page.Catalog.AdditionalSyncNotes);
	}

	[TestMethod]
	public async Task OnPost_InvalidModelState_ReturnsPage()
	{
		var submission = _db.AddAndSaveUnpublishedSubmission().Entity;
		var user = _db.AddUser("TestUser").Entity;

		_page.Id = submission.Id;
		_page.Catalog = new CatalogModel.SubmissionCatalog
		{
			System = 999, // Invalid system ID
			SystemFramerate = submission.SystemFrameRateId
		};

		AddAuthenticatedUser(_page, user, [PermissionTo.CatalogMovies]);

		var result = await _page.OnPost();

		Assert.IsInstanceOfType<PageResult>(result);
		Assert.IsFalse(_page.ModelState.IsValid);
	}

	[TestMethod]
	public async Task OnPost_SubmissionNotFound_ReturnsNotFound()
	{
		_page.Id = 999;

		var result = await _page.OnPost();

		Assert.IsInstanceOfType<NotFoundResult>(result);
	}

	[TestMethod]
	public async Task OnPost_ValidUpdate_UpdatesSubmissionAndRedirects()
	{
		var system = new GameSystem { Id = 1, Code = "NES", DisplayName = "Nintendo Entertainment System" };
		_db.GameSystems.Add(system);
		var frameRate = new GameSystemFrameRate { Id = 1, GameSystemId = system.Id, FrameRate = 60.0988 };
		_db.GameSystemFrameRates.Add(frameRate);
		var submitter = _db.AddUser("TestUser").Entity;
		await _db.SaveChangesAsync();

		var submission = new Submission
		{
			Title = "Test Submission",
			SystemId = system.Id,
			SystemFrameRateId = frameRate.Id,
			EmulatorVersion = "Old Emulator",
			AdditionalSyncNotes = "Old notes",
			Submitter = submitter,
			Status = SubmissionStatus.New
		};

		_db.Submissions.Add(submission);
		await _db.SaveChangesAsync();

		_page.Id = submission.Id;
		_page.Catalog = new CatalogModel.SubmissionCatalog
		{
			System = system.Id,
			SystemFramerate = frameRate.Id,
			Emulator = "New Emulator",
			AdditionalSyncNotes = "New notes"
		};

		var user = _db.AddUser("CatalogUser").Entity;
		AddAuthenticatedUser(_page, user, [PermissionTo.CatalogMovies]);

		var result = await _page.OnPost();

		Assert.IsInstanceOfType<RedirectToPageResult>(result);
		var redirectResult = (RedirectToPageResult)result;
		Assert.AreEqual("View", redirectResult.PageName);

		var updatedSubmission = await _db.Submissions.FindAsync(submission.Id);
		Assert.IsNotNull(updatedSubmission);
		Assert.AreEqual("New Emulator", updatedSubmission.EmulatorVersion);
		Assert.AreEqual("New notes", updatedSubmission.AdditionalSyncNotes);
		await _publisher.Received(1).Send(Arg.Any<Post>());
	}

	[TestMethod]
	public void CanSyncVerify_AllRequiredFieldsPresent_ReturnsTrue()
	{
		var catalog = new CatalogModel.SubmissionCatalog
		{
			Emulator = "Test Emulator",
			Game = 1,
			GameVersion = 1,
			SystemFramerate = 1,
			System = 1
		};

		Assert.IsTrue(catalog.CanSyncVerify);
	}

	[TestMethod]
	[DataRow("", 1, 1, 1, 1)]
	[DataRow(null, 1, 1, 1, 1)]
	[DataRow("Test", null, 1, 1, 1)]
	[DataRow("Test", 1, null, 1, 1)]
	[DataRow("Test", 1, 1, null, 1)]
	[DataRow("Test", 1, 1, 1, null)]
	public void CanSyncVerify_MissingRequiredFields_ReturnsFalse(string? emulator, int? game, int? gameVersion, int? systemFramerate, int? system)
	{
		var catalog = new CatalogModel.SubmissionCatalog
		{
			Emulator = emulator,
			Game = game,
			GameVersion = gameVersion,
			SystemFramerate = systemFramerate,
			System = system
		};

		Assert.IsFalse(catalog.CanSyncVerify);
	}
}
