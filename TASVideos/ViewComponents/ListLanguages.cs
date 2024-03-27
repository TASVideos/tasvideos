using Microsoft.AspNetCore.Mvc.ViewComponents;
using TASVideos.Core.Services.Wiki;

namespace TASVideos.ViewComponents;

public class ListLanguages(ILanguages languages) : ViewComponent
{
	public async Task<IViewComponentResult> InvokeAsync(IWikiPage? pageData)
	{
		if (string.IsNullOrWhiteSpace(pageData?.PageName))
		{
			return new ContentViewComponentResult("");
		}

		var languages1 = await languages.GetTranslations(pageData.PageName);
		return View(languages1);
	}
}
