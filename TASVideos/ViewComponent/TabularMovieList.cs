using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using TASVideos.Data;
using TASVideos.Data.Entity;

namespace TASVideos.ViewComponents
{
	public class TabularMovieList : ViewComponent
	{
		private readonly ApplicationDbContext _db;

		public TabularMovieList(ApplicationDbContext db)
		{
			_db = db;
		}

		public async Task<IViewComponentResult> InvokeAsync(WikiPage pageData, string pp)
		{
			var search = new TabularMovieListSearchModel();
			var limit = ParamHelper.GetInt(pp, "limit");
			if (limit.HasValue)
			{
				search.Limit = limit.Value;
			}

			var tiersStr = ParamHelper.GetValueFor(pp, "tier");
			if (!string.IsNullOrWhiteSpace(tiersStr))
			{
				search.Tiers = tiersStr.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);
			}
			
			ViewData["flink"] = ParamHelper.GetValueFor(pp, "flink");
			ViewData["footer"] = ParamHelper.GetValueFor(pp, "footer") ?? "More...";

			var model = await MovieList(search);

			return View(model);
		}

		private async Task<IEnumerable<TabularMovieListResultModel>> MovieList(TabularMovieListSearchModel searchCriteria)
		{
			// It is important to actually query for an Entity object here instead of a ViewModel
			// Because we need the title property which is a derived property that can't be done in Linq to Sql
			// And needs a variety of information from sub-tables, hence all the includes
			var movies = await _db.Publications
				.Include(p => p.Tier)
				.Include(p => p.Game)
				.Include(p => p.System)
				.Include(p => p.SystemFrameRate)
				.Include(p => p.Files)
				.Include(p => p.Authors)
				.ThenInclude(pa => pa.Author)
				.Where(p => searchCriteria.Tiers.Contains(p.Tier.Name))
				.ByMostRecent()
				.Take(searchCriteria.Limit)
				.ToListAsync();

			var results = movies
				.Select(m => new TabularMovieListResultModel
				{
					Id = m.Id,
					CreateTimeStamp = m.CreateTimeStamp,
					Time = m.Time,
					Game = m.Game.DisplayName,
					Authors = string.Join(", ", m.Authors.Select(pa => pa.Author)),
					ObsoletedBy = null, // TODO: previous logic
					Screenshot = m.Files.First(f => f.Type == FileType.Screenshot).Path
				})
				.ToList();

			return results;
		}
	}
}