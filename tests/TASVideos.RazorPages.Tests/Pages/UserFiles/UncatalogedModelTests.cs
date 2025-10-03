using TASVideos.Data.Entity.Game;
using TASVideos.Pages.UserFiles;
using TASVideos.Tests.Base;

namespace TASVideos.RazorPages.Tests.Pages.UserFiles;

[TestClass]
public class UncatalogedModelTests : TestDbBase
{
	private readonly Uncataloged _page;

	public UncatalogedModelTests()
	{
		_page = new Uncataloged(_db);
	}

	[TestMethod]
	public async Task OnGet_LoadsPublicUncatalogedFiles()
	{
		var user = _db.AddUser("TestUser").Entity;
		var system = _db.GameSystems.Add(new GameSystem { Id = 1, Code = "NES", DisplayName = "Nintendo Entertainment System" }).Entity;

		var uncatalogedFile = new UserFile
		{
			Id = 1,
			FileName = "uncataloged.bk2",
			Title = "Uncataloged File",
			Author = user,
			Hidden = false,
			System = system,
			GameId = null,
			UploadTimestamp = DateTime.UtcNow.AddDays(-1)
		};

		var game = _db.Games.Add(new Game { Id = 1, DisplayName = "Test Game" }).Entity;

		var catalogedFile = new UserFile
		{
			Id = 2,
			FileName = "cataloged.bk2",
			Title = "Cataloged File",
			Author = user,
			Hidden = false,
			System = system,
			Game = game,
			UploadTimestamp = DateTime.UtcNow.AddDays(-2)
		};

		_db.UserFiles.AddRange(uncatalogedFile, catalogedFile);
		await _db.SaveChangesAsync();

		await _page.OnGet();

		Assert.AreEqual(1, _page.Files.Count);
		var file = _page.Files.First();
		Assert.AreEqual(1, file.Id);
		Assert.AreEqual("uncataloged.bk2", file.FileName);
		Assert.AreEqual("NES", file.SystemCode);
		Assert.AreEqual("TestUser", file.Author);
	}

	[TestMethod]
	public async Task OnGet_ExcludesHiddenFiles()
	{
		var user = _db.AddUser("TestUser").Entity;
		var system = _db.GameSystems.Add(new GameSystem { Id = 1, Code = "NES", DisplayName = "Nintendo Entertainment System" }).Entity;

		var publicFile = new UserFile
		{
			Id = 1,
			FileName = "public.bk2",
			Title = "Public File",
			Author = user,
			Hidden = false,
			System = system,
			GameId = null,
			UploadTimestamp = DateTime.UtcNow
		};

		var hiddenFile = new UserFile
		{
			Id = 2,
			FileName = "hidden.bk2",
			Title = "Hidden File",
			Author = user,
			Hidden = true,
			System = system,
			GameId = null,
			UploadTimestamp = DateTime.UtcNow
		};

		_db.UserFiles.AddRange(publicFile, hiddenFile);
		await _db.SaveChangesAsync();

		await _page.OnGet();

		Assert.AreEqual(1, _page.Files.Count);
		Assert.AreEqual("public.bk2", _page.Files.First().FileName);
	}

	[TestMethod]
	public async Task OnGet_HandlesFilesWithoutSystem()
	{
		var user = _db.AddUser("TestUser").Entity;
		_db.UserFiles.Add(new UserFile
		{
			Id = 1,
			FileName = "NoSystem.lua",
			Title = "File Without System",
			Author = user,
			SystemId = null,
			GameId = null
		});
		await _db.SaveChangesAsync();

		await _page.OnGet();

		Assert.AreEqual(1, _page.Files.Count);
		var file = _page.Files.First();
		Assert.AreEqual("NoSystem.lua", file.FileName);
		Assert.IsNull(file.SystemCode);
	}

	[TestMethod]
	public async Task OnGet_ReturnsEmptyListWhenNoUncatalogedFiles()
	{
		var user = _db.AddUser("TestUser").Entity;
		var game = _db.AddGame("Test Game").Entity;
		_db.UserFiles.Add(new UserFile
		{
			Id = 1,
			FileName = "cataloged.bk2",
			Title = "Cataloged File",
			Author = user,
			Hidden = false,
			Game = game,
			UploadTimestamp = DateTime.UtcNow
		});
		await _db.SaveChangesAsync();

		await _page.OnGet();

		Assert.AreEqual(0, _page.Files.Count);
	}
}
