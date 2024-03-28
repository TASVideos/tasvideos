namespace TASVideos.Pages.Profile;

[RequirePermission(PermissionTo.RateMovies)]
public class UnratedModel(ApplicationDbContext db) : BasePageModel
{
	public List<UnratedMovie> UnratedMovies { get; set; } = [];

	public async Task OnGet()
	{
		var userId = User.GetUserId();
		UnratedMovies = await db.Publications
			.ThatAreCurrent()
			.Where(p => p.PublicationRatings.All(pr => pr.UserId != userId))
			.Select(p => new UnratedMovie(p.Id, p.Title))
			.ToListAsync();
	}

	public record UnratedMovie(int Id, string Title);
}
