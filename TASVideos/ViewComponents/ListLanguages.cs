using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using TASVideos.Core.Services;
using TASVideos.Data.Entity;

namespace TASVideos.ViewComponents;

public class ListLanguages : ViewComponent
{
	private readonly ILanguages _languages;

	public ListLanguages(ILanguages languages)
	{
		_languages = languages;
	}

	public async Task<IViewComponentResult> InvokeAsync(WikiPage? pageData)
	{
		if (string.IsNullOrWhiteSpace(pageData?.PageName))
		{
			return new ContentViewComponentResult("");
		}

		var languages = await _languages.GetTranslations(pageData.PageName);
		return View(languages);
	}
}
