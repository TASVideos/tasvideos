﻿using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using TASVideos.Services;
using TASVideos.WikiEngine;

namespace TASVideos.ViewComponents
{
	[WikiModule(WikiModules.Awards)]
	public class Awards : ViewComponent
	{
		private readonly IAwards _awards;

		public Awards(IAwards awards)
		{
			_awards = awards;
		}

		public async Task<IViewComponentResult> InvokeAsync(string pp)
		{
			var cache = ParamHelper.GetValueFor(pp, "clear");
			if (!string.IsNullOrWhiteSpace(cache))
			{
				return new ContentViewComponentResult("Error: clear parameter no longer supported");
			}

			int? year = ParamHelper.GetInt(pp, "year");

			if (!year.HasValue)
			{
				return new ContentViewComponentResult("Error: parameterless award module no longer supported");
			}

			var model = await _awards.ForYear(year.Value);

			return View(model);
		}
	}
}
