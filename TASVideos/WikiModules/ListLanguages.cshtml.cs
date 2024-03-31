using Microsoft.AspNetCore.Mvc.ViewComponents;
using TASVideos.Core.Services.Wiki;

namespace TASVideos.WikiModules;

public class ListLanguages(ILanguages languages) : WikiViewComponent
{
	public IEnumerable<LanguagePage> Languages { get; set; } = null!;

	public async Task<IViewComponentResult> InvokeAsync(IWikiPage? pageData)
	{
		if (string.IsNullOrWhiteSpace(pageData?.PageName))
		{
			return new ContentViewComponentResult("");
		}

		Languages = await languages.GetTranslations(pageData.PageName);
		return View();
	}
}
