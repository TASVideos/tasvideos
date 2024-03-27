using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TASVideos.Pages.Publications;

// Handles legacy movies.cgi links
[AllowAnonymous]
public class LegacyMoviesModel(ApplicationDbContext db) : PageModel
{
	[FromQuery]
	public string? Name { get; set; }

	[FromQuery]
	public string? Id { get; set; }

	[FromQuery]
	public string? Rec { get; set; }

	public async Task<IActionResult> OnGet()
	{
		var ids = Id.CsvToInts().ToList();
		string query;

		// Id filtering supercedes any other filters, so we can short circuit here
		if (ids.Any())
		{
			query = string.Join("-", ids.Select(i => i + "M"));
			return RedirectToPage("/Publications/Index", new { query });
		}

		List<string> tokens = [];
		if (!string.IsNullOrWhiteSpace(Name))
		{
			// Movies.cgi only supported a single game name
			var game = await db.Games.FirstOrDefaultAsync(g => g.DisplayName == Name || g.Abbreviation == Name);
			if (game is not null)
			{
				tokens.Add(game.Id + "G");
			}
		}

		// rec=N is the same as rec=Y, it simply checks for existence of the param
		if (!string.IsNullOrWhiteSpace(Rec))
		{
			tokens.Add("NewcomerRec");
		}

		if (tokens.Any())
		{
			query = string.Join("-", tokens);
			return RedirectToPage("/Publications/Index", new { query });
		}

		return Redirect("Movies");
	}
}
