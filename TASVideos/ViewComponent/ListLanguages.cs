using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewComponents;

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
			if (string.IsNullOrWhiteSpace(pageData?.PageName))
			{
				return new ContentViewComponentResult("");
			}

			var languagesMarkup = _wikiPages
				.Page("System/Languages")
				?.Markup;

			if (string.IsNullOrWhiteSpace(languagesMarkup))
			{
				return new ContentViewComponentResult("");
			}

			var languages = languagesMarkup
				.Split(',')
				.ToDictionary(
					tkey => tkey.Split(":").FirstOrDefault(),
					tvalue => tvalue.Split(":".LastOrDefault()));

			var translationPages = languages.Keys
				.Select(l => l + "/" + pageData.PageName)
				.ToList();

			var translations = _wikiPages
				.ThatAreCurrentRevisions()
				.Where(wp => translationPages.Contains(wp.PageName))
				.ToList();


			return View();
		}
	}
}
