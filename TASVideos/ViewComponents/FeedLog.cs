using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using TASVideos.Core.Services.RssFeedParsers;
using TASVideos.WikiEngine;

namespace TASVideos.ViewComponents;

[WikiModule(WikiModules.FeedLog)]
public class FeedLog : ViewComponent
{
	private readonly IHttpClientFactory _httpClientFactory;
	private readonly IVcsRssParser _parser;
	private readonly ILogger<FeedLog> _logger;

	public FeedLog(
		IHttpClientFactory httpClientFactory,
		IVcsRssParser parser,
		ILogger<FeedLog> logger)
	{
		_httpClientFactory = httpClientFactory;
		_parser = parser;
		_logger = logger;
	}

	public async Task<IViewComponentResult> InvokeAsync(string url, string type)
	{
		if (string.IsNullOrWhiteSpace(url))
		{
			return new ContentViewComponentResult("url parameter is required.");
		}

		if (!_parser.IsSupportedType(type))
		{
			return new ContentViewComponentResult($"Error: {type} is not a supported type.");
		}

		var client = _httpClientFactory.CreateClient();
		var request = new HttpRequestMessage(HttpMethod.Get, url);

		try
		{
			var response = await client.SendAsync(request);
			if (response.IsSuccessStatusCode)
			{
				var xml = await response.Content.ReadAsStringAsync();
				var entries = _parser.Parse(type, xml);
				return View(entries);
			}
		}
		catch (Exception ex)
		{
			_logger.LogError("Unable to process feed log for {url} ex: {ex}", url, ex.ToString());
			return new ContentViewComponentResult($"An error occurred attempting to retrieve data from {url}.");
		}

		return new ContentViewComponentResult($"An error occurred attempting to retrieve data from {url}.");
	}
}
