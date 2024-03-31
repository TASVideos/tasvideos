using Microsoft.AspNetCore.Mvc.ViewComponents;
using TASVideos.Core.Services.Wiki;

namespace TASVideos.WikiModules;

public class RenderWikiPage(IWikiPages wikiPages) : WikiViewComponent
{
	public RenderWikiPageModel Page { get; set; } = new();

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

		Page = new RenderWikiPageModel
		{
			Markup = existingPage.Markup,
			PageData = existingPage
		};
		ViewData.SetWikiPage(existingPage);
		ViewData.SetTitle(existingPage.PageName);
		ViewData["Layout"] = null;
		return View();
	}

	public class RenderWikiPageModel
	{
		public string Markup { get; init; } = "";

		public IWikiPage PageData { get; init; } = null!;
	}
}
