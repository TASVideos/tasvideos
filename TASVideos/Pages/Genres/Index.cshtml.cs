using TASVideos.Core.Services;

namespace TASVideos.Pages.Genres;

public class IndexModel(IGenreService genreService) : BasePageModel
{
	public IEnumerable<GenreDto> Genres { get; set; } = new List<GenreDto>();

	public async Task OnGet()
	{
		Genres = (await genreService.GetAll())
			.OrderBy(g => g.DisplayName)
			.ToList();
	}
}
