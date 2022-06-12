using TASVideos.Core.Services;
using TASVideos.Data.Entity.Game;

namespace TASVideos.Pages.Genres;

public class IndexModel : BasePageModel
{
	private readonly IGenreService _genreService;

	public IndexModel(IGenreService genreService)
	{
		_genreService = genreService;
	}

	public IEnumerable<GenreDto> Genres { get; set; } = new List<GenreDto>();

	public async Task OnGet()
	{
		Genres = (await _genreService.GetAll())
			.OrderBy(g => g.DisplayName)
			.ToList();
	}
}
