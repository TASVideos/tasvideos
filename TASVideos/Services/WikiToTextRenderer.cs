using TASVideos.Core.Services.Youtube;
using TASVideos.Core.Settings;
using TASVideos.Data.Entity;
using TASVideos.WikiEngine;
using TASVideos.WikiEngine.AST;

namespace TASVideos.Services;

public class WikiToTextRenderer : IWikiToTextRenderer
{
	private readonly AppSettings _settings;
	private readonly IServiceProvider _serviceProvider;

	public WikiToTextRenderer(AppSettings settings, IServiceProvider serviceProvider)
	{
		_settings = settings;
		_serviceProvider = serviceProvider;
	}

	public async Task<string> RenderWikiForYoutube(WikiPage page)
	{
		var sw = new StringWriter();
		await Util.RenderTextAsync(page.Markup, sw, new WriterHelper(_settings.BaseUrl, _serviceProvider, page));
		return sw.ToString();
	}

	private class WriterHelper : IWriterHelper
	{
		private readonly string _host;
		private readonly IServiceProvider _serviceProvider;
		private readonly WikiPage _wikiPage;

		public WriterHelper(string host, IServiceProvider serviceProvider, WikiPage wikiPage)
		{
			_host = host;
			_serviceProvider = serviceProvider;
			_wikiPage = wikiPage;
		}

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

		public async Task RunViewComponentAsync(TextWriter w, string name, IReadOnlyDictionary<string, string> pp)
		{
			var componentExists = ModuleParamHelpers.TextComponents.TryGetValue(name, out Type? textComponent);
			if (!componentExists)
			{
				return;
			}

			var invokeMethod = textComponent!.GetMethod("RenderTextAsync");

			if (invokeMethod == null && textComponent.GetMethod("RenderText") != null)
			{
				throw new NotImplementedException("Sync method not supported yet");
			}

			if (invokeMethod == null)
			{
				throw new InvalidOperationException($"Could not find an RenderText method on ViewComponent {textComponent}");
			}

			var paramObject = ModuleParamHelpers
				.GetParameterData(w, name, invokeMethod, _wikiPage, pp);

			var module = _serviceProvider.GetRequiredService(textComponent);
			var result = await (Task<string>)invokeMethod.Invoke(module, paramObject.Values.ToArray())!;
			await w.WriteAsync(result);
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
