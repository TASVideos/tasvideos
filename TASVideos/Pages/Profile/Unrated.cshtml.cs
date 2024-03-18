using Microsoft.AspNetCore.Mvc;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Pages.Profile.Models;

namespace TASVideos.Pages.Profile;

[RequirePermission(PermissionTo.RateMovies)]
public class UnratedModel(ApplicationDbContext db) : BasePageModel
{
	public IEnumerable<UnratedMovieModel> UnratedMovies { get; set; } = [];

	public async Task<IActionResult> OnGet()
	{
		var userId = User.GetUserId();
		UnratedMovies = await db.Publications
			.ThatAreCurrent()
			.Where(p => p.PublicationRatings.All(pr => pr.UserId != userId))
			.Select(p => new UnratedMovieModel
			{
				Id = p.Id,
				Title = p.Title,
			})
			.ToListAsync();

		return Page();
	}
}
