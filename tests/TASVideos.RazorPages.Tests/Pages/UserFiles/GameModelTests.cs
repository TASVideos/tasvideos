using TASVideos.Pages.UserFiles;
using TASVideos.Tests.Base;

namespace TASVideos.RazorPages.Tests.Pages.UserFiles;

[TestClass]
public class GameModelTests : TestDbBase
{
	private readonly GameModel _page;

	public GameModelTests()
	{
		_page = new GameModel(_db);
	}

	[TestMethod]
	public async Task OnGet_WithNonExistentGameId_ReturnsNotFound()
	{
		_page.Id = 999; // Non-existent game ID
		var result = await _page.OnGet();
		Assert.IsInstanceOfType<NotFoundResult>(result);
	}

	[TestMethod]
	public async Task OnGet_AnonymousUser_LoadsOnlyPublicFiles()
	{
		var game = _db.AddGame("Test Game").Entity;
		var author = _db.AddUser("TestAuthor").Entity;
		await _db.SaveChangesAsync();

		var publicFile = new UserFile
		{
			Id = 1,
			FileName = "public-movie.bk2",
			Title = "Public Movie",
			Author = author,
			Game = game,
			Hidden = false,
			Class = UserFileClass.Movie
		};

		var hiddenFile = new UserFile
		{
			Id = 2,
			FileName = "hidden-movie.bk2",
			Title = "Hidden Movie",
			Author = author,
			Game = game,
			Hidden = true,
			Class = UserFileClass.Movie
		};

		_db.UserFiles.AddRange(publicFile, hiddenFile);
		await _db.SaveChangesAsync();

		_page.Id = game.Id;

		var result = await _page.OnGet();

		Assert.IsInstanceOfType<PageResult>(result);
		Assert.AreEqual("Test Game", _page.GameName);
		Assert.AreEqual(1, _page.Files.Count);
		Assert.AreEqual("Public Movie", _page.Files.First().Title);
	}

	[TestMethod]
	public async Task OnGet_AsAuthor_LoadsAllUserFilesForGame()
	{
		var game = _db.AddGame("Test Game").Entity;
		var author = _db.AddUser("TestAuthor").Entity;
		await _db.SaveChangesAsync();

		var publicFile = new UserFile
		{
			Id = 1,
			FileName = "public-movie.bk2",
			Title = "Public Movie",
			Author = author,
			Game = game,
			Hidden = false,
			Class = UserFileClass.Movie
		};

		var hiddenFile = new UserFile
		{
			Id = 2,
			FileName = "hidden-movie.bk2",
			Title = "Hidden Movie",
			Author = author,
			Game = game,
			Hidden = true,
			Class = UserFileClass.Movie
		};

		_db.UserFiles.AddRange(publicFile, hiddenFile);
		await _db.SaveChangesAsync();

		// Author is the viewer
		AddAuthenticatedUser(_page, author, []);
		_page.Id = game.Id;

		var result = await _page.OnGet();

		Assert.IsInstanceOfType<PageResult>(result);
		Assert.AreEqual("Test Game", _page.GameName);
		Assert.AreEqual(2, _page.Files.Count);
		var titles = _page.Files.Select(f => f.Title).ToList();
		Assert.IsTrue(titles.Contains("Public Movie"));
		Assert.IsTrue(titles.Contains("Hidden Movie"));
	}

	[TestMethod]
	public async Task OnGet_WithValidGame_LoadsOnlyFilesForThatGame()
	{
		var game1 = _db.AddGame("Game 1").Entity;
		var game2 = _db.AddGame("Game 2").Entity;
		var author = _db.AddUser("TestAuthor").Entity;
		await _db.SaveChangesAsync();

		var fileForGame1 = new UserFile
		{
			Id = 1,
			FileName = "game1-movie.bk2",
			Title = "Game 1 Movie",
			Author = author,
			Game = game1,
			Class = UserFileClass.Movie
		};

		var fileForGame2 = new UserFile
		{
			Id = 2,
			FileName = "game2-movie.bk2",
			Title = "Game 2 Movie",
			Author = author,
			Game = game2,
			Class = UserFileClass.Movie
		};

		_db.UserFiles.AddRange(fileForGame1, fileForGame2);
		await _db.SaveChangesAsync();

		_page.Id = game1.Id;

		var result = await _page.OnGet();

		Assert.IsInstanceOfType<PageResult>(result);
		Assert.AreEqual("Game 1", _page.GameName);
		Assert.AreEqual(1, _page.Files.Count);
		Assert.AreEqual("Game 1 Movie", _page.Files.First().Title);
	}

	[TestMethod]
	public async Task OnGet_OrdersByUploadTimestampDescending()
	{
		var game = _db.AddGame("Test Game").Entity;
		var author = _db.AddUser("TestAuthor").Entity;
		await _db.SaveChangesAsync();

		var olderFile = new UserFile
		{
			Id = 1,
			FileName = "older-movie.bk2",
			Title = "Older Movie",
			Author = author,
			GameId = game.Id,
			Hidden = false,
			Class = UserFileClass.Movie,
			UploadTimestamp = DateTime.UtcNow.AddDays(-2)
		};

		var newerFile = new UserFile
		{
			Id = 2,
			FileName = "newer-movie.bk2",
			Title = "Newer Movie",
			Author = author,
			GameId = game.Id,
			Hidden = false,
			Class = UserFileClass.Movie,
			UploadTimestamp = DateTime.UtcNow.AddDays(-1)
		};

		_db.UserFiles.AddRange(olderFile, newerFile);
		await _db.SaveChangesAsync();

		_page.Id = game.Id;

		var result = await _page.OnGet();

		Assert.IsInstanceOfType<PageResult>(result);
		Assert.AreEqual(2, _page.Files.Count);
		Assert.AreEqual("Newer Movie", _page.Files.First().Title);
		Assert.AreEqual("Older Movie", _page.Files.Last().Title);
	}

	[TestMethod]
	public async Task OnGet_WithGameButNoFiles_ReturnsEmptyList()
	{
		var game = _db.AddGame("Test Game").Entity;
		await _db.SaveChangesAsync();

		_page.Id = game.Id;

		var result = await _page.OnGet();

		Assert.IsInstanceOfType<PageResult>(result);
		Assert.AreEqual("Test Game", _page.GameName);
		Assert.AreEqual(0, _page.Files.Count);
	}

	[TestMethod]
	public async Task OnGet_HidesFilesFromOtherUsers()
	{
		var game = _db.AddGame("Test Game").Entity;
		var author1 = _db.AddUser("Author1").Entity;
		var author2 = _db.AddUser("Author2").Entity;
		var viewer = _db.AddUser("Viewer").Entity;
		await _db.SaveChangesAsync();

		var author1PublicFile = new UserFile
		{
			Id = 1,
			FileName = "author1-public.bk2",
			Title = "Author1 Public Movie",
			Author = author1,
			Game = game,
			Hidden = false,
			Class = UserFileClass.Movie
		};

		var author1HiddenFile = new UserFile
		{
			Id = 2,
			FileName = "author1-hidden.bk2",
			Title = "Author1 Hidden Movie",
			Author = author1,
			Game = game,
			Hidden = true,
			Class = UserFileClass.Movie
		};

		var author2PublicFile = new UserFile
		{
			Id = 3,
			FileName = "author2-public.bk2",
			Title = "Author2 Public Movie",
			Author = author2,
			Game = game,
			Hidden = false,
			Class = UserFileClass.Movie
		};

		var author2HiddenFile = new UserFile
		{
			Id = 4,
			FileName = "author2-hidden.bk2",
			Title = "Author2 Hidden Movie",
			Author = author2,
			Game = game,
			Hidden = true,
			Class = UserFileClass.Movie
		};

		_db.UserFiles.AddRange(author1PublicFile, author1HiddenFile, author2PublicFile, author2HiddenFile);
		await _db.SaveChangesAsync();

		// Viewer should only see public files from all authors
		AddAuthenticatedUser(_page, viewer, []);
		_page.Id = game.Id;

		var result = await _page.OnGet();

		Assert.IsInstanceOfType<PageResult>(result);
		Assert.AreEqual(2, _page.Files.Count); // Only public files
		var titles = _page.Files.Select(f => f.Title).ToList();
		Assert.IsTrue(titles.Contains("Author1 Public Movie"));
		Assert.IsTrue(titles.Contains("Author2 Public Movie"));
		Assert.IsFalse(titles.Contains("Author1 Hidden Movie"));
		Assert.IsFalse(titles.Contains("Author2 Hidden Movie"));
	}

	[TestMethod]
	public void AllowsAnonymousAttribute() => AssertAllowsAnonymousUsers(typeof(GameModel));
}
