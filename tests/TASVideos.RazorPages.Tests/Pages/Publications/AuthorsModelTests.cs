using TASVideos.Pages.Publications;
using TASVideos.Tests.Base;

namespace TASVideos.RazorPages.Tests.Pages.Publications;

[TestClass]
public class AuthorsModelTests : TestDbBase
{
	private readonly AuthorsModel _page;

	public AuthorsModelTests()
	{
		_page = new AuthorsModel(_db);
	}

	[TestMethod]
	public async Task OnGet_NoAuthors_ReturnsEmptyList()
	{
		await _page.OnGet();
		Assert.AreEqual(0, _page.Authors.Count);
	}

	[TestMethod]
	public async Task OnGet_WithAuthors_ReturnsAuthors()
	{
		var author = _db.AddUser(1, "TestAuthor").Entity;
		_db.AddPublication(author);

		await _page.OnGet();

		Assert.AreEqual(1, _page.Authors.Count);
		Assert.AreEqual("TestAuthor", _page.Authors[0].Author);
		Assert.AreEqual(1, _page.Authors[0].Id);
		Assert.AreEqual(1, _page.Authors[0].ActivePubCount);
		Assert.AreEqual(0, _page.Authors[0].ObsoletePubCount);
	}

	[TestMethod]
	public async Task OnGet_WithObsoletedPublication_CountsCorrectly()
	{
		var author = _db.AddUser(1, "TestAuthor").Entity;
		var activePub = _db.AddPublication(author).Entity;
		var obsoletePub = _db.AddPublication(author).Entity;
		obsoletePub.ObsoletedById = activePub.Id;
		await _db.SaveChangesAsync();

		await _page.OnGet();

		Assert.AreEqual(1, _page.Authors.Count);
		Assert.AreEqual(1, _page.Authors[0].ActivePubCount);
		Assert.AreEqual(1, _page.Authors[0].ObsoletePubCount);
	}
}
