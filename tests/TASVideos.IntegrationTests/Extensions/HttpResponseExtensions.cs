using AngleSharp;
using AngleSharp.Html.Dom;

namespace TASVideos.IntegrationTests.Extensions;

public static class HttpResponseExtensions
{
	/// <summary>
	/// Asserts that the response was successful (2xx status code)
	/// </summary>
	public static void EnsureSuccessStatusCode(this HttpResponseMessage response, string? context = null)
	{
		if (!response.IsSuccessStatusCode)
		{
			var message = context != null
				? $"{context}: Expected success status code but got {response.StatusCode}"
				: $"Expected success status code but got {response.StatusCode}";
			throw new HttpRequestException(message);
		}
	}

	/// <summary>
	/// Gets the page title from the HTML response
	/// </summary>
	public static async Task<string> GetPageTitleAsync(this HttpResponseMessage response)
	{
		var document = await response.GetHtmlDocumentAsync();
		return document.Title ?? "";
	}

	/// <summary>
	/// Checks if the response contains specific text
	/// </summary>
	public static async Task<bool> ContainsTextAsync(this HttpResponseMessage response, string text)
	{
		var content = await response.Content.ReadAsStringAsync();
		return content.Contains(text, StringComparison.OrdinalIgnoreCase);
	}

	/// <summary>
	/// Gets all elements matching a CSS selector
	/// </summary>
	public static async Task<IEnumerable<IElement>> QuerySelectorAllAsync(this HttpResponseMessage response, string selector)
	{
		var document = await response.GetHtmlDocumentAsync();
		return document.QuerySelectorAll(selector);
	}

	/// <summary>
	/// Gets the first element matching a CSS selector
	/// </summary>
	public static async Task<IElement?> QuerySelectorAsync(this HttpResponseMessage response, string selector)
	{
		var document = await response.GetHtmlDocumentAsync();
		return document.QuerySelector(selector);
	}

	private static async Task<IHtmlDocument> GetHtmlDocumentAsync(this HttpResponseMessage response)
	{
		var content = await response.Content.ReadAsStringAsync();
		var config = Configuration.Default;
		var context = BrowsingContext.New(config);
		return await context.OpenAsync(req => req.Content(content)) as IHtmlDocument
			?? throw new InvalidOperationException("Failed to parse HTML document");
	}
}
