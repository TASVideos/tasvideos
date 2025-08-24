using TASVideos.Core.Services;
using TASVideos.Pages.Genres;

namespace TASVideos.RazorPages.Tests.Pages.Genres;

[TestClass]
public class IndexModelTests : BasePageModelTests
{
	private readonly IGenreService _genreService;
	private readonly IndexModel _model;

	public IndexModelTests()
	{
		_genreService = Substitute.For<IGenreService>();
		_model = new IndexModel(_genreService)
		{
			PageContext = TestPageContext()
		};
	}

	[TestMethod]
	public async Task OnGet_NoGenres_LoadsEmptyCollection()
	{
		_genreService.GetAll().Returns([]);

		await _model.OnGet();

		Assert.AreEqual(0, _model.Genres.Count);
		await _genreService.Received(1).GetAll();
	}

	[TestMethod]
	public async Task OnGet_WithGenres_LoadsAllGenres()
	{
		_genreService.GetAll().Returns(new List<GenreDto>
		{
			new(1, "Action", 10),
			new(2, "Adventure", 5),
			new(3, "RPG", 15)
		});

		await _model.OnGet();

		Assert.AreEqual(3, _model.Genres.Count);
		Assert.IsTrue(_model.Genres.Any(g => g is { DisplayName: "Action", GameCount: 10 }));
		Assert.IsTrue(_model.Genres.Any(g => g is { DisplayName: "Adventure", GameCount: 5 }));
		Assert.IsTrue(_model.Genres.Any(g => g is { DisplayName: "RPG", GameCount: 15 }));
		await _genreService.Received(1).GetAll();
	}

	[TestMethod]
	public void AllowsAnonymousAttribute() => AssertAllowsAnonymousUsers(typeof(IndexModel));
}
