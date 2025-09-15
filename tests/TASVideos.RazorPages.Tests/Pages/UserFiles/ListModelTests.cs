using TASVideos.Pages.UserFiles;
using TASVideos.Tests.Base;

namespace TASVideos.RazorPages.Tests.Pages.UserFiles;

[TestClass]
public class ListModelTests : TestDbBase
{
	private readonly ListModel _page;

	public ListModelTests()
	{
		_page = new ListModel(_db);
	}

	[TestMethod]
	public async Task OnGet_LoadsPublicUserFiles()
	{
		var author = _db.AddUser("TestAuthor").Entity;
		var publicFile = new UserFile
		{
			Id = 1,
			FileName = "public-movie.bk2",
			Title = "Public Movie",
			Author = author,
			Hidden = false,
			Class = UserFileClass.Movie,
			UploadTimestamp = DateTime.UtcNow.AddDays(-1)
		};

		var hiddenFile = new UserFile
		{
			Id = 2,
			FileName = "hidden-movie.bk2",
			Title = "Hidden Movie",
			Author = author,
			Hidden = true,
			Class = UserFileClass.Movie,
			UploadTimestamp = DateTime.UtcNow.AddDays(-2)
		};

		_db.UserFiles.AddRange(publicFile, hiddenFile);
		await _db.SaveChangesAsync();

		await _page.OnGet();

		Assert.AreEqual(1, _page.UserFiles.Count());
		Assert.AreEqual("Public Movie", _page.UserFiles.First().Title);
		Assert.AreEqual("TestAuthor", _page.UserFiles.First().Author);
	}

	[TestMethod]
	public async Task OnGet_OrdersByUploadTimestampDescending()
	{
		var author1 = _db.AddUser("Author1").Entity;
		var author2 = _db.AddUser("Author2").Entity;
		var olderFile = new UserFile
		{
			Id = 1,
			FileName = "older-movie.bk2",
			Title = "Older Movie",
			Author = author1,
			Hidden = false,
			Class = UserFileClass.Movie,
			UploadTimestamp = DateTime.UtcNow.AddDays(-3)
		};

		var newerFile = new UserFile
		{
			Id = 2,
			FileName = "newer-movie.bk2",
			Title = "Newer Movie",
			Author = author2,
			Hidden = false,
			Class = UserFileClass.Movie,
			UploadTimestamp = DateTime.UtcNow.AddDays(-1),
			Frames = 12345,
			Rerecords = 6789
		};

		_db.UserFiles.AddRange(olderFile, newerFile);
		await _db.SaveChangesAsync();

		await _page.OnGet();

		Assert.AreEqual(2, _page.UserFiles.Count());
		var newerMovie = _page.UserFiles.First();
		Assert.AreEqual("Newer Movie", newerMovie.Title);
		Assert.AreEqual(12345, newerMovie.Frames);
		Assert.AreEqual(6789, newerMovie.Rerecords);
		Assert.AreEqual("Older Movie", _page.UserFiles.Last().Title);
	}

	[TestMethod]
	public async Task OnGet_WithPaging_RespectsPageSize()
	{
		var author = _db.AddUser("TestAuthor").Entity;
		await _db.SaveChangesAsync();

		// Create 10 files
		for (int i = 1; i <= 10; i++)
		{
			_db.UserFiles.Add(new UserFile
			{
				Id = i,
				FileName = $"movie-{i}.bk2",
				Title = $"Movie {i}",
				Author = author,
				Hidden = false,
				Class = UserFileClass.Movie,
				UploadTimestamp = DateTime.UtcNow.AddDays(-i)
			});
		}

		await _db.SaveChangesAsync();

		_page.Search = new ListModel.UserFileListRequest { CurrentPage = 1, PageSize = 5 };

		await _page.OnGet();

		Assert.AreEqual(5, _page.UserFiles.Count()); // Should respect page size
		Assert.AreEqual(10, _page.UserFiles.RowCount); // Should show total count
	}

	[TestMethod]
	public async Task OnGet_EmptyDatabase_ReturnsEmptyResult()
	{
		await _page.OnGet();

		Assert.AreEqual(0, _page.UserFiles.Count());
		Assert.AreEqual(0, _page.UserFiles.RowCount);
	}

	[TestMethod]
	public void AllowsAnonymousUsers() => AssertAllowsAnonymousUsers(typeof(ListModel));
}
