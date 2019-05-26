using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewComponents;

using TASVideos.Data.Entity;
using TASVideos.Services;
using TASVideos.ViewComponents.Models;

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
			// This was originally done to put the header with a link back to the english page
			// Now we always put the parent module on the page which will handle this
			if (ParamHelper.HasParam(pp, "istranslation"))
			{
				return new ContentViewComponentResult("");
			}

			if (string.IsNullOrWhiteSpace(pageData?.PageName))
			{
				return new ContentViewComponentResult("");
			}

			var languagesMarkup = (await _wikiPages
				.SystemPage("Languages"))
				?.Markup;

			if (string.IsNullOrWhiteSpace(languagesMarkup))
			{
				return new ContentViewComponentResult("");
			}

			var languages = languagesMarkup
				.Split(',')
				.Select(l =>
				{
					var split = l.Split(":");
					return new LanguageEntry
					{
						LanguageCode = split.FirstOrDefault(),
						LanguageDisplayName = split.LastOrDefault(),
						Path = split.FirstOrDefault() + "/" + pageData.PageName
					};
				})
				.Where(l => _wikiPages.Exists(l.Path));

			return View(languages);
		}
	}
}
