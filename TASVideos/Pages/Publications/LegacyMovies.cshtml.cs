using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TASVideos.Data;

namespace TASVideos.Pages.Publications;

// Handles legacy movies.cgi links
[AllowAnonymous]
public class LegacyMoviesModel : PageModel
{
	private readonly ApplicationDbContext _db;

	public LegacyMoviesModel(ApplicationDbContext db)
	{
		_db = db;
	}

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

		List<string> tokens = new();
		if (!string.IsNullOrWhiteSpace(Name))
		{
			// Movies.cgi only supported a single game name
			var game = await _db.Games.FirstOrDefaultAsync(g => g.DisplayName == Name || g.GoodName == Name);
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
