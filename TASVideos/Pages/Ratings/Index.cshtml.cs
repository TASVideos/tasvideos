using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Pages.Ratings.Models;

namespace TASVideos.Pages.Ratings;

[AllowAnonymous]
public class IndexModel : BasePageModel
{
	private readonly ApplicationDbContext _db;

	public IndexModel(ApplicationDbContext db)
	{
		_db = db;
	}

	[FromRoute]
	public int Id { get; set; }

	public PublicationRatingsModel Publication { get; set; } = new();

	public IEnumerable<PublicationRatingsModel.RatingEntry> VisibleRatings => User.Has(PermissionTo.SeePrivateRatings)
		? Publication.Ratings
		: Publication.Ratings.Where(r => r.IsPublic || r.UserName == User.Name());

	// TODO: refactor to use pointsService for calculations
	public async Task<IActionResult> OnGet()
	{
		var publication = await _db.Publications
			.Include(p => p.PublicationRatings)
			.ThenInclude(r => r.User)
			.SingleOrDefaultAsync(p => p.Id == Id);

		if (publication is null)
		{
			return NotFound();
		}

		Publication = new PublicationRatingsModel
		{
			PublicationTitle = publication.Title,
			Ratings = publication.PublicationRatings
				.Select(pr => new PublicationRatingsModel.RatingEntry
				{
					UserName = pr.User!.UserName,
					IsPublic = pr.User!.PublicRatings,
					Rating = pr.Value
				})
				.ToList()
		};

		// Entertainment counts 2:1 over Tech
		Publication.OverallRating = Publication.Ratings.Any()
			? Math.Round(Publication.Ratings.Select(r => r.Rating).Average(), 2)
			: 0;

		return Page();
	}
}
