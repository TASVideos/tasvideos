using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;

using TASVideos.Data.Entity;
using TASVideos.Services;

namespace TASVideos.ViewComponents
{
	public class ListLanguages : ViewComponent
	{
		private readonly IWikiPages _wikiPages;

		public ListLanguages(IWikiPages wikiPages)
		{
			_wikiPages = wikiPages;
		}

		public async Task<IViewComponentResult> InvokeAsync(WikiPage pageData, string pp)
		{
			return View();
		}
	}
}
