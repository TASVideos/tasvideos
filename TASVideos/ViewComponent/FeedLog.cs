using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using TASVideos.Services.RssFeedParsers;
using TASVideos.WikiEngine;

namespace TASVideos.ViewComponents
{
	[WikiModule(WikiModules.FeedLog)]
	public class FeedLog : ViewComponent
	{
		private readonly IHttpClientFactory _httpClientFactory;
		private readonly IVcsRssParser _parser;

		public FeedLog(IHttpClientFactory httpClientFactory, IVcsRssParser parser)
		{
			_httpClientFactory = httpClientFactory;
			_parser = parser;
		}

		public async Task<IViewComponentResult> InvokeAsync(string pp)
		{
			var url = ParamHelper.GetValueFor(pp, "url");
			var type = ParamHelper.GetValueFor(pp, "type");

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
			catch (Exception)
			{
				return new ContentViewComponentResult($"An error occurred attempting to retrieve data from {url}.");
			}

			return new ContentViewComponentResult($"An error occurred attempting to retrieve data from {url}.");
		}
	}
}
