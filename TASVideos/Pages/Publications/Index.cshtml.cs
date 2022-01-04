using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TASVideos.Core.Services;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Pages.Publications.Models;

namespace TASVideos.Pages.Publications
{
	// TODO: add paging
	[AllowAnonymous]
	public class IndexModel : BasePageModel
	{
		private readonly ApplicationDbContext _db;
		private readonly IMapper _mapper;
		private readonly IPointsService _points;
		private readonly IMovieSearchTokens _movieTokens;

		public IndexModel(
			ApplicationDbContext db,
			IMapper mapper,
			IPointsService points,
			IMovieSearchTokens movieTokens)
		{
			_db = db;
			_mapper = mapper;
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
				Classes = tokenLookup.Classes.Where(t => tokens.Contains(t)),
				SystemCodes = tokenLookup.SystemCodes.Where(s => tokens.Contains(s)),
				ShowObsoleted = tokens.Contains("obs"),
				OnlyObsoleted = tokens.Contains("obsonly"),
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
				return BaseRedirect("Movies");
			}

			Movies = await _mapper.ProjectTo<PublicationDisplayModel>(
				_db.Publications
					.OrderBy(p => p.System!.Code)
					.ThenBy(p => p.Game!.DisplayName)
					.FilterByTokens(searchModel))
				.ToListAsync();

			var ratings = (await _points.PublicationRatings(Movies.Select(m => m.Id)))
				.ToDictionary(tkey => tkey.Key, tvalue => tvalue.Value.Overall);

			foreach ((int key, double? value) in ratings)
			{
				Movies.First(m => m.Id == key).OverallRating = value;
			}

			return Page();
		}
	}
}
