using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using TASVideos.Data.Entity;
using TASVideos.Extensions;
using TASVideos.Models;
using TASVideos.Tasks;

namespace TASVideos.ViewComponents
{
	public class TabularMovieList : ViewComponent
	{
		private readonly PublicationTasks _publicationTasks;

		public TabularMovieList(PublicationTasks publicationTasks)
		{
			_publicationTasks = publicationTasks;
		}

		public async Task<IViewComponentResult> InvokeAsync(WikiPage pageData, string pp)
		{
			var search = new TabularMovieListSearchModel();
			var limit = WikiHelper.GetInt(WikiHelper.GetValueFor(pp, "limit"));
			if (limit.HasValue)
			{
				search.Limit = limit.Value;
			}

			var tiersStr = WikiHelper.GetValueFor(pp, "tier");
			if (!string.IsNullOrWhiteSpace(tiersStr))
			{
				search.Tiers = tiersStr.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);
			}
			
			// TODO: footer and flink

			var model = await _publicationTasks.GetTabularMovieList(search);

			return View(model);
		}
	}
}