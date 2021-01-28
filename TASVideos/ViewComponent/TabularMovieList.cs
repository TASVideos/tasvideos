﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Extensions;
using TASVideos.WikiEngine;

namespace TASVideos.ViewComponents
{
	[WikiModule(WikiModules.TabularMovieList)]
	public class TabularMovieList : ViewComponent
	{
		private readonly ApplicationDbContext _db;

		public TabularMovieList(ApplicationDbContext db)
		{
			_db = db;
		}

		public async Task<IViewComponentResult> InvokeAsync(string pp)
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
				search.Tiers = tiersStr.SplitWithEmpty(",");
			}

			ViewData["flink"] = ParamHelper.GetValueFor(pp, "flink");

			var footer = ParamHelper.GetValueFor(pp, "footer");
			if (!string.IsNullOrWhiteSpace(footer))
			{
				footer = "More...";
			}

			ViewData["footer"] = footer;

			var model = await MovieList(search);

			return View(model);
		}

		private async Task<IEnumerable<TabularMovieListResultModel>> MovieList(TabularMovieListSearchModel searchCriteria)
		{
			var results = await _db.Publications
				.Where(p => searchCriteria.Tiers.Contains(p.Tier!.Name))
				.ByMostRecent()
				.Take(searchCriteria.Limit)
				.Select(p => new TabularMovieListResultModel
				{
					Id = p.Id,
					CreateTimeStamp = p.CreateTimeStamp,
					Frames = p.Frames,
					FrameRate = p.SystemFrameRate!.FrameRate,
					Game = p.Game!.DisplayName,
					Authors = string.Join(",", p.Authors.Select(pa => pa.Author!.UserName)),
					Screenshot = p.Files
						.Where(f => f.Type == FileType.Screenshot)
						.Select(f => new TabularMovieListResultModel.ScreenshotFile
						{
							Path = f.Path,
							Description = f.Description
						})
						.First(),
					ObsoletedMovie = p.ObsoletedMovies
						.Select(o => new TabularMovieListResultModel.ObsoletedPublication
						{
							Id = o.Id,
							Frames = o.Frames,
							FrameRate = o.SystemFrameRate!.FrameRate
						})
						.FirstOrDefault()
				})
				.ToListAsync();

			return results;
		}
	}
}
