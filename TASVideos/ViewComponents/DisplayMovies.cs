using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using TASVideos.Core.Services;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Pages.Publications.Models;
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
			bool obsonly,
			IList<int> year,
			IList<string> tag,
			IList<string> flag,
			IList<int> group,
			IList<int> id,
			IList<int> game,
			IList<int> author,
			string sort,
			int? limit)
		{
			var tokenLookup = await _tokens.GetTokens();

			var searchModel = new PublicationSearchModel
			{
				Classes = tokenLookup.Classes.Where(c => tier.Select(tt => tt.ToLower()).Contains(c)),
				SystemCodes = tokenLookup.SystemCodes.Where(s => systemCode.Select(c => c.ToLower()).Contains(s)),
				ShowObsoleted = obs,
				OnlyObsoleted = obsonly,
				SortBy = sort,
				Limit = limit,
				Years = tokenLookup.Years.Where(year.Contains),
				Tags = tokenLookup.Tags.Where(t => tag.Select(tt => tt.ToLower()).Contains(t)),
				Genres = tokenLookup.Genres.Where(g => tag.Select(tt => tt.ToLower()).Contains(g)),
				Flags = tokenLookup.Flags.Where(f => flag.Select(ff => ff.ToLower()).Contains(f)),
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
					.FilterByTokens(searchModel))
				.ToListAsync();
			return View(results);
		}
	}
}
