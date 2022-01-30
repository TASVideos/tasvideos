using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Pages.Profile.Models;

namespace TASVideos.Pages.Profile;

[RequirePermission(PermissionTo.RateMovies)]
public class UnratedModel : PageModel
{
	private readonly ApplicationDbContext _db;

	public UnratedModel(ApplicationDbContext db)
	{
		_db = db;
	}

	public IEnumerable<UnratedMovieModel> UnratedMovies { get; set; } = new List<UnratedMovieModel>();

	public async Task<IActionResult> OnGet()
	{
		var userId = User.GetUserId();
		UnratedMovies = await _db.Publications
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
