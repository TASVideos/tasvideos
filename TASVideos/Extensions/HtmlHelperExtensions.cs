using Microsoft.AspNetCore.Html;

namespace TASVideos.Extensions;

public static class HtmlHelperExtensions
{
	public static string ToYesNo(this bool val)
	{
		return val ? "Yes" : "No";
	}

	public static async Task<IHtmlContent> RenderWiki(this IHtmlHelper html, string pageName)
	{
		return await html.PartialAsync("_RenderWikiPage", pageName);
	}
}
