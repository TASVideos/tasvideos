﻿using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using TASVideos.Core.Services;
using TASVideos.Extensions;

namespace TASVideos.ViewComponents;

public class RenderWikiPage : ViewComponent
{
	private readonly IWikiPages _wikiPages;

	public RenderWikiPage(
		IWikiPages wikiPages)
	{
		_wikiPages = wikiPages;
	}

	public async Task<IViewComponentResult> InvokeAsync(string? url, int? revision = null)
	{
		url = url?.Trim('/') ?? "";
		if (!WikiHelper.IsValidWikiPageName(url))
		{
			return new ContentViewComponentResult("");
		}

		var existingPage = await _wikiPages.Page(url, revision);

		if (existingPage is not null)
		{
			var model = new RenderWikiPageModel
			{
				Markup = existingPage.Markup,
				PageData = existingPage
			};
			ViewData["WikiPage"] = existingPage;
			ViewData["Title"] = existingPage.PageName;
			ViewData["Layout"] = null;
			return View(model);
		}

		return new ContentViewComponentResult("");
	}
}
