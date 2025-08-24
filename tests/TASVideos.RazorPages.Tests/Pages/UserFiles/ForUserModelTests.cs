using TASVideos.Core;
using TASVideos.Pages.UserFiles;
using TASVideos.Tests.Base;

namespace TASVideos.RazorPages.Tests.Pages.UserFiles;

[TestClass]
public class ForUserModelTests : TestDbBase
{
	private readonly ForUserModel _page;

	public ForUserModelTests()
	{
		_page = new ForUserModel(_db);
	}

	[TestMethod]
	public async Task OnGet_AnonymousUser_LoadsOnlyPublicFiles()
	{
		var author = _db.AddUser("TestAuthor").Entity;
		var publicFile = new UserFile
		{
			Id = 1,
			FileName = "public-movie.bk2",
			Title = "Public Movie",
			Author = author,
			Hidden = false,
			Class = UserFileClass.Movie
		};

		var hiddenFile = new UserFile
		{
			Id = 2,
			FileName = "hidden-movie.bk2",
			Title = "Hidden Movie",
			Author = author,
			Hidden = true,
			Class = UserFileClass.Movie
		};

		_db.UserFiles.AddRange(publicFile, hiddenFile);
		await _db.SaveChangesAsync();

		_page.UserName = "TestAuthor";

		await _page.OnGet();

		Assert.AreEqual(1, _page.Files.Count());
		Assert.AreEqual("Public Movie", _page.Files.First().Title);
	}

	[TestMethod]
	public async Task OnGet_AsAuthor_LoadsAllUserFiles()
	{
		var author = _db.AddUser("TestAuthor").Entity;

		var publicFile = new UserFile
		{
			Id = 1,
			FileName = "public-movie2.bk2",
			Title = "Public Movie 2",
			Author = author,
			Hidden = false,
			Class = UserFileClass.Movie
		};

		var hiddenFile = new UserFile
		{
			Id = 2,
			FileName = "hidden-movie2.bk2",
			Title = "Hidden Movie 2",
			Author = author,
			Hidden = true,
			Class = UserFileClass.Movie
		};

		_db.UserFiles.AddRange(publicFile, hiddenFile);
		await _db.SaveChangesAsync();

		// Author is the viewer
		AddAuthenticatedUser(_page, author, []);
		_page.UserName = "TestAuthor";

		await _page.OnGet();

		// Should show all files when viewed by the author
		Assert.AreEqual(2, _page.Files.Count());
		var titles = _page.Files.Select(f => f.Title).ToList();
		Assert.IsTrue(titles.Contains("Public Movie 2"));
		Assert.IsTrue(titles.Contains("Hidden Movie 2"));
	}

	[TestMethod]
	public async Task OnGet_WithNonExistentUser_ReturnsEmptyList()
	{
		_page.UserName = "NonExistentUser";
		await _page.OnGet();
		Assert.AreEqual(0, _page.Files.Count());
	}

	[TestMethod]
	public async Task OnGet_OrdersByUploadTimestampDescending()
	{
		var author = _db.AddUser("TestAuthor3").Entity;
		var olderFile = new UserFile
		{
			Id = 1,
			FileName = "older-movie.bk2",
			Title = "Older Movie",
			Author = author,
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
			Hidden = false,
			Class = UserFileClass.Movie,
			UploadTimestamp = DateTime.UtcNow.AddDays(-1)
		};

		_db.UserFiles.AddRange(olderFile, newerFile);
		await _db.SaveChangesAsync();

		_page.UserName = "TestAuthor3";

		await _page.OnGet();

		Assert.AreEqual(2, _page.Files.Count());
		Assert.AreEqual("Newer Movie", _page.Files.First().Title);
		Assert.AreEqual("Older Movie", _page.Files.Last().Title);
	}

	[TestMethod]
	public async Task OnGet_WithPaging_RespectsPageSize()
	{
		var author = _db.AddUser("TestAuthor").Entity;

		for (int i = 1; i <= 5; i++)
		{
			_db.UserFiles.Add(new UserFile
			{
				Id = 1 + i,
				FileName = $"movie-{i}-test.bk2",
				Author = author,
				Class = UserFileClass.Movie
			});
		}

		await _db.SaveChangesAsync();

		_page.UserName = "TestAuthor";
		_page.Search = new PagingModel { CurrentPage = 1, PageSize = 3 };

		await _page.OnGet();

		Assert.AreEqual(3, _page.Files.Count()); // Should respect page size
		Assert.AreEqual(5, _page.Files.RowCount); // Should show total count
	}

	[TestMethod]
	public void AllowsAnonymousAttribute() => AssertAllowsAnonymousUsers(typeof(ForUserModel));
}
