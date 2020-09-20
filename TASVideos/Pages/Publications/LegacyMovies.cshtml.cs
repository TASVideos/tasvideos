using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TASVideos.Data;
using TASVideos.Extensions;

namespace TASVideos.Pages.Publications
{
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

		public async Task<IActionResult> OnGet()
		{
			IEnumerable<int> ids = Id.CsvToInts();
			
			if (ids.Any())
			{
				var query = string.Join("-", ids.Select(i => i + "M"));
				return RedirectToPage("/Publications/Index", new { query });
			}

			if (!string.IsNullOrWhiteSpace(Name))
			{
				// Movies.cgi only supported a single game name
				var game = await _db.Games.FirstOrDefaultAsync(g => g.DisplayName == Name || g.GoodName == Name);
				if (game != null)
				{
					var query = game.Id + "G";
					return RedirectToPage("/Publications/Index", new { query });
				}
			}

			return NotFound();
		}
	}
}
