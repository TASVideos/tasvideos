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

namespace TASVideos.ViewComponents
{
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

		public async Task<IViewComponentResult> InvokeAsync(string pp)
		{
			var tokenLookup = await _tokens.GetTokens();

			var tiers = ParamHelper.GetValueFor(pp, "tier").CsvToStrings();
			var systemCodes = ParamHelper.GetValueFor(pp, "systemCode").CsvToStrings();
			var obs = ParamHelper.HasParam(pp, "obs");
			var years = ParamHelper.GetValueFor(pp, "year").CsvToInts().ToList();
			var tags = ParamHelper.GetValueFor(pp, "tag").CsvToStrings();
			var flags = ParamHelper.GetValueFor(pp, "flag").CsvToStrings();
			var groups = ParamHelper.GetValueFor(pp, "group").CsvToInts();
			var ids = ParamHelper.GetValueFor(pp, "id").CsvToInts();
			var games = ParamHelper.GetValueFor(pp, "game").CsvToInts();
			var authors = ParamHelper.GetValueFor(pp, "author").CsvToInts();
			
			var searchModel = new PublicationSearchModel
			{
				Tiers = tokenLookup.Tiers.Where(t => tiers.Contains(t)),
				SystemCodes = tokenLookup.SystemCodes.Where(s => systemCodes.Contains(s)),
				ShowObsoleted = obs,
				Years = tokenLookup.Years.Where(y => years.Contains(y)),
				Tags = tokenLookup.Tags.Where(t => tags.Contains(t)),
				Genres = tokenLookup.Genres.Where(g => tags.Contains(g)),
				Flags = tokenLookup.Flags.Where(f => flags.Contains(f)),
				MovieIds = ids,
				Games = games,
				GameGroups = groups,
				Authors = authors
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
