using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Extensions;
using TASVideos.Pages.Publications.Models;
using TASVideos.Services;
using TASVideos.WikiEngine;

namespace TASVideos.ViewComponents
{
	[WikiModule(WikiModules.DisplayMovies)]
	public class DisplayMovies : ViewComponent
	{
		private readonly ApplicationDbContext _db;
		private readonly IMapper _mapper;
		private readonly IMovieSearchTokens _tokens;

		public DisplayMovies(
			ApplicationDbContext db,
			IMapper mapper,
			IMovieSearchTokens tokens)
		{
			_db = db;
			_mapper = mapper;
			_tokens = tokens;
		}

		public async Task<IViewComponentResult> InvokeAsync(
			IList<string> tier,
			IList<string> systemCode,
			bool obs,
			IList<int> year,
			IList<string> tag,
			IList<string> flag,
			IList<int> group,
			IList<int> id,
			IList<int> game,
			IList<int> author
		)
		{
			var tokenLookup = await _tokens.GetTokens();

			var searchModel = new PublicationSearchModel
			{
				Tiers = tokenLookup.Tiers.Where(t => tier.Contains(t)),
				SystemCodes = tokenLookup.SystemCodes.Where(s => systemCode.Contains(s)),
				ShowObsoleted = obs,
				Years = tokenLookup.Years.Where(y => year.Contains(y)),
				Tags = tokenLookup.Tags.Where(t => tag.Contains(t)),
				Genres = tokenLookup.Genres.Where(g => tag.Contains(g)),
				Flags = tokenLookup.Flags.Where(f => flag.Contains(f)),
				MovieIds = id,
				Games = game,
				GameGroups = group,
				Authors = author
			};

			if (searchModel.IsEmpty)
			{
				return View(new List<PublicationDisplayModel>());
			}

			var results = await _mapper.ProjectTo<PublicationDisplayModel>(
				_db.Publications
					.OrderBy(p => p.System!.Code)
					.ThenBy(p => p.Game!.DisplayName)
					.FilterByTokens(searchModel))
				.ToListAsync();
			return View(results);
		}
	}
}
