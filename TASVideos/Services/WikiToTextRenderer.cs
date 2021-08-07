using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using TASVideos.Core.Services.Youtube;
using TASVideos.Core.Settings;
using TASVideos.Data.Entity;
using TASVideos.WikiEngine;
using TASVideos.WikiEngine.AST;

namespace TASVideos.Services
{
	public class WikiToTextRenderer : IWikiToTextRenderer
	{
		private readonly AppSettings _settings;

		public WikiToTextRenderer(AppSettings settings)
		{
			_settings = settings;
		}

		public async Task<string> RenderWikiForYoutube(WikiPage page)
		{
			var sw = new StringWriter();
			await Util.RenderTextAsync(page.Markup, sw, new WriterHelper(_settings.BaseUrl));
			return sw.ToString();
		}

		private class WriterHelper : IWriterHelper
		{
			private readonly string _host;
			public WriterHelper(string host) { _host = host; }
			public bool CheckCondition(string condition)
			{
				bool result = false;

				if (condition.StartsWith('!'))
				{
					result = true;
					condition = condition.TrimStart('!');
				}

				switch (condition)
				{
					case "1":
						result ^= true;
						break;
					default:
						result ^= false;
						break;
				}

				return result;
			}

			public Task RunViewComponentAsync(TextWriter w, string name, IReadOnlyDictionary<string, string> pp)
			{
				// TODO: This needs to be handled in concert with Wiki changes
				return Task.CompletedTask;
			}

			public string AbsoluteUrl(string url)
			{
				if (!Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out var parsed))
				{
					return url;
				}

				if (!parsed.IsAbsoluteUri)
				{
					return _host.TrimEnd('/') + url;
				}

				return url;
			}
		}
	}
}
