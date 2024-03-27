using Microsoft.AspNetCore.Mvc.ViewComponents;
using TASVideos.Core.Services.Wiki;

namespace TASVideos.ViewComponents;

public class RenderWikiPage(IWikiPages wikiPages) : ViewComponent
{
	public async Task<IViewComponentResult> InvokeAsync(string? url, int? revision = null)
	{
		url = url?.Trim('/') ?? "";
		if (!WikiHelper.IsValidWikiPageName(url))
		{
			return new ContentViewComponentResult("");
		}

		var existingPage = await wikiPages.Page(url, revision);
		if (existingPage is null)
		{
			return new ContentViewComponentResult("");
		}

		var model = new RenderWikiPageModel
		{
			Markup = existingPage.Markup,
			PageData = existingPage
		};
		ViewData.SetWikiPage(existingPage);
		ViewData.SetTitle(existingPage.PageName);
		ViewData["Layout"] = null;
		return View(model);
	}
}
