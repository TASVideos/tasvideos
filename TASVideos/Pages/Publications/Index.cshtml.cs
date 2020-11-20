using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using AutoMapper.QueryableExtensions;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Pages.Publications.Models;
using TASVideos.Services;

namespace TASVideos.Pages.Publications
{
	// TODO: add paging
	[AllowAnonymous]
	public class IndexModel : BasePageModel
	{
		private readonly ApplicationDbContext _db;
		private readonly IPointsService _points;
		private readonly IMovieSearchTokens _movieTokens;

		public IndexModel(
			ApplicationDbContext db,
			IPointsService points,
			IMovieSearchTokens movieTokens)
		{
			_db = db;
			_points = points;
			_movieTokens = movieTokens;
		}

		[FromRoute]
		public string Query { get; set; } = "";

		public IEnumerable<PublicationDisplayModel> Movies { get; set; } = new List<PublicationDisplayModel>();

		public async Task<IActionResult> OnGet()
		{
			var tokenLookup = await _movieTokens.GetTokens();

			var tokens = Query.ToTokens();

			var searchModel = new PublicationSearchModel
			{
				Tiers = tokenLookup.Tiers.Where(t => tokens.Contains(t)),
				SystemCodes = tokenLookup.SystemCodes.Where(s => tokens.Contains(s)),
				ShowObsoleted = tokens.Contains("obs"),
				Years = tokenLookup.Years.Where(y => tokens.Contains("y" + y)),
				Tags = tokenLookup.Tags.Where(t => tokens.Contains(t)),
				Genres = tokenLookup.Genres.Where(g => tokens.Contains(g)),
				Flags = tokenLookup.Flags.Where(f => tokens.Contains(f)),
				MovieIds = tokens.ToIdList('m'),
				Games = tokens.ToIdList('g'),
				GameGroups = tokens.ToIdListPrefix("group"),
				Authors = tokens
					.Where(t => t.ToLower().Contains("author"))
					.Select(t => t.ToLower().Replace("author", ""))
					.Select(t => int.TryParse(t, out var temp) ? temp : (int?)null)
					.Where(t => t.HasValue)
					.Select(t => t!.Value)
					.ToList()
			};

			// If no valid filter criteria, don't attempt to generate a list (else it would be all movies for what is most likely a malformed URL)
			if (searchModel.IsEmpty)
			{
				return Redirect("Movies");
			}

			Movies = await _db.Publications
				.OrderBy(p => p.System!.Code)
				.ThenBy(p => p.Game!.DisplayName)
				.FilterByTokens(searchModel)
				.ProjectTo<PublicationDisplayModel>()
				.ToListAsync();

			var ratings = (await _points.PublicationRatings(Movies.Select(m => m.Id)))
				.ToDictionary(tkey => tkey.Key, tvalue => tvalue.Value.Overall);

			foreach (var rating in ratings)
			{
				Movies.First(m => m.Id == rating.Key).OverallRating = rating.Value;
			}

			return Page();
		}
	}
}
