using Microsoft.AspNetCore.Html;
using TASVideos.Core.Services.Wiki;
using TASVideos.WikiModules;

namespace TASVideos.Extensions;

public static class ViewComponentHelperExtensions
{
	public static async Task<IHtmlContent> RenderWiki(this IViewComponentHelper component, string pageName)
	{
		return await component.InvokeAsync(nameof(RenderWikiPage), new { url = pageName });
	}

	public static async Task<IHtmlContent> ListSubPages(this IViewComponentHelper component, IWikiPage pageData, bool show)
	{
		return await component.InvokeAsync(nameof(WikiModules.ListSubPages), new { pageData, show });
	}

	public static async Task<IHtmlContent> ListLanguages(this IViewComponentHelper component, IWikiPage pageData)
	{
		return await component.InvokeAsync(nameof(WikiModules.ListLanguages), new { pageData });
	}

	public static async Task<IHtmlContent> HomePageHeader(this IViewComponentHelper component, IWikiPage pageData)
	{
		return await component.InvokeAsync(nameof(WikiModules.HomePageHeader), new { pageData });
	}

	public static async Task<IHtmlContent> GameResourcesHeader(this IViewComponentHelper component, IWikiPage pageData)
	{
		return await component.InvokeAsync(nameof(WikiModules.GameResourcesHeader), new { pageData });
	}

	public static async Task<IHtmlContent> GameResourcesFooter(this IViewComponentHelper component, IWikiPage pageData)
	{
		return await component.InvokeAsync(nameof(WikiModules.GameResourcesFooter), new { pageData });
	}

	public static async Task<IHtmlContent> SystemPageHeader(this IViewComponentHelper component, IWikiPage pageData)
	{
		return await component.InvokeAsync(nameof(WikiModules.SystemPageHeader), new { pageData });
	}

	public static async Task<IHtmlContent> SystemPageFooter(this IViewComponentHelper component, IWikiPage pageData)
	{
		return await component.InvokeAsync(nameof(WikiModules.SystemPageFooter), new { pageData });
	}
}
