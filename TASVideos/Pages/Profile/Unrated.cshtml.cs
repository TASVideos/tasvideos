namespace TASVideos.Pages.Profile;

[RequirePermission(PermissionTo.RateMovies)]
public class UnratedModel(ApplicationDbContext db) : BasePageModel
{
	public List<UnratedMovieModel> UnratedMovies { get; set; } = [];

	public async Task<IActionResult> OnGet()
	{
		var userId = User.GetUserId();
		UnratedMovies = await db.Publications
			.ThatAreCurrent()
			.Where(p => p.PublicationRatings.All(pr => pr.UserId != userId))
			.Select(p => new UnratedMovieModel(p.Id, p.Title))
			.ToListAsync();

		return Page();
	}

	public record UnratedMovieModel(int Id, string Title);
}
