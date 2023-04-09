using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using TASVideos.Core.Services.Wiki;
using TASVideos.ViewComponents;

namespace TASVideos.Extensions;

public static class ViewComponentHelperExtensions
{
	public static async Task<IHtmlContent> RenderWiki(this IViewComponentHelper component, string pageName)
	{
		return await component.InvokeAsync(nameof(RenderWikiPage), new { url = pageName });
	}

	public static async Task<IHtmlContent> ListSubPages(this IViewComponentHelper component, IWikiPage pageData, bool show)
	{
		return await component.InvokeAsync(nameof(ViewComponents.ListSubPages), new { pageData, show });
	}

	public static async Task<IHtmlContent> ListLanguages(this IViewComponentHelper component, IWikiPage pageData)
	{
		return await component.InvokeAsync(nameof(ViewComponents.ListLanguages), new { pageData });
	}

	public static async Task<IHtmlContent> HomePageHeader(this IViewComponentHelper component, IWikiPage pageData)
	{
		return await component.InvokeAsync(nameof(ViewComponents.HomePageHeader), new { pageData });
	}

	public static async Task<IHtmlContent> GameResourcesHeader(this IViewComponentHelper component, IWikiPage pageData)
	{
		return await component.InvokeAsync(nameof(ViewComponents.GameResourcesHeader), new { pageData });
	}

	public static async Task<IHtmlContent> GameResourcesFooter(this IViewComponentHelper component, IWikiPage pageData)
	{
		return await component.InvokeAsync(nameof(ViewComponents.GameResourcesFooter), new { pageData });
	}

	public static async Task<IHtmlContent> SystemPageHeader(this IViewComponentHelper component, IWikiPage pageData)
	{
		return await component.InvokeAsync(nameof(ViewComponents.SystemPageHeader), new { pageData });
	}

	public static async Task<IHtmlContent> SystemPageFooter(this IViewComponentHelper component, IWikiPage pageData)
	{
		return await component.InvokeAsync(nameof(ViewComponents.SystemPageFooter), new { pageData });
	}
}
