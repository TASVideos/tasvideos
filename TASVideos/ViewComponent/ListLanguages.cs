using System.Collections.Generic;
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
		private readonly ILanguages _languages;

		public ListLanguages(ILanguages languages, IWikiPages wikiPages)
		{
			_languages = languages;
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

			if (string.IsNullOrWhiteSpace(pageData.PageName))
			{
				return new ContentViewComponentResult("");
			}


			var languages = (await _languages.AvailableLanguages())
				.Select(l => new LanguageEntry
				{
					LanguageCode = l.Code,
					LanguageDisplayName = l.DisplayName,
					Path = l.Code + "/" + pageData.PageName
				})
				.ToList();

			if (!languages.Any())
			{
				return new ContentViewComponentResult("");
			}

			var existingLanguages = new List<LanguageEntry>();
			foreach (var lang in languages)
			{
				if (await _wikiPages.Exists(lang.Path))
				{
					existingLanguages.Add(lang);
				}
			}

			return View(existingLanguages);
		}
	}
}
