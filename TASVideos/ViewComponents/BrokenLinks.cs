using Microsoft.AspNetCore.Mvc;
using TASVideos.Core.Services;
using TASVideos.WikiEngine;

namespace TASVideos.ViewComponents;

[WikiModule(WikiModules.BrokenLinks)]
public class BrokenLinks : ViewComponent
{
	private readonly IWikiPages _wikiPages;

	public BrokenLinks(IWikiPages wikiPages)
	{
		_wikiPages = wikiPages;
	}

	public async Task<IViewComponentResult> InvokeAsync()
	{
		var brokenLinks = await _wikiPages.BrokenLinks();
		return View(brokenLinks);
	}
}
