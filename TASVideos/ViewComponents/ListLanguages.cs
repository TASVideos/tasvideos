using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using TASVideos.Core.Services;
using TASVideos.Data.Entity;
using TASVideos.ViewComponents.Models;
using TASVideos.WikiEngine;

namespace TASVideos.ViewComponents;

[WikiModule(WikiModules.ListLanguages)]
public class ListLanguages : ViewComponent
{
	private readonly IWikiPages _wikiPages;
	private readonly ILanguages _languages;

	public ListLanguages(ILanguages languages, IWikiPages wikiPages)
	{
		_languages = languages;
		_wikiPages = wikiPages;
	}

	public async Task<IViewComponentResult> InvokeAsync(WikiPage? pageData, bool isTranslation)
	{
		if (string.IsNullOrWhiteSpace(pageData?.PageName))
		{
			return new ContentViewComponentResult("");
		}

		string pageName = pageData.PageName;
		var languages = new List<LanguageEntry>();

		if (isTranslation)
		{
			// Actual translation pages should be nested from the language page
			if (!pageName.Contains('/'))
			{
				return new ContentViewComponentResult("");
			}

			pageName = string.Join("", pageName.Split('/').Skip(1));

			// Translations should also include the original link to the English version
			languages.Add(new LanguageEntry
			{
				LanguageCode = "EN",
				LanguageDisplayName = "English",
				Path = pageName
			});
		}

		languages.AddRange((await _languages.AvailableLanguages())
			.Select(l => new LanguageEntry
			{
				LanguageCode = l.Code,
				LanguageDisplayName = l.DisplayName,
				Path = l.Code + "/" + pageName
			})
			.ToList());

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
