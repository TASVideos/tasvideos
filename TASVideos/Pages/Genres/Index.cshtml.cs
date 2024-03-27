namespace TASVideos.Pages.Genres;

public class IndexModel(IGenreService genreService) : BasePageModel
{
	public IReadOnlyCollection<GenreDto> Genres { get; set; } = [];

	public async Task OnGet()
	{
		Genres = await genreService.GetAll();
	}
}
