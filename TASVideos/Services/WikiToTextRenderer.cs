using TASVideos.Core.Services.Wiki;
using TASVideos.Core.Services.Youtube;
using TASVideos.Core.Settings;
using TASVideos.WikiEngine;
using TASVideos.WikiEngine.AST;

namespace TASVideos.Services;

public class WikiToTextRenderer(AppSettings settings, IServiceProvider serviceProvider) : IWikiToTextRenderer
{
	public async Task<string> RenderWikiForYoutube(IWikiPage page)
	{
		var sw = new StringWriter();
		await Util.RenderTextAsync(page.Markup, sw, new WriterHelper(settings.BaseUrl, serviceProvider, page));
		return sw.ToString();
	}

	private class WriterHelper(string host, IServiceProvider serviceProvider, IWikiPage wikiPage)
		: IWriterHelper
	{
		public bool CheckCondition(string condition)
		{
			var result = false;

			if (condition.StartsWith('!'))
			{
				result = true;
				condition = condition.TrimStart('!');
			}

			result ^= condition switch
			{
				"1" => true,
				_ => false
			};

			return result;
		}

		public async Task RunViewComponentAsync(TextWriter w, string name, IReadOnlyDictionary<string, string> pp)
		{
			var componentExists = ModuleParamHelpers.TextComponents.TryGetValue(name, out var textComponent);
			if (!componentExists)
			{
				return;
			}

			var invokeMethod = textComponent!.GetMethod("RenderTextAsync");

			switch (invokeMethod)
			{
				case null when textComponent.GetMethod("RenderText") is not null:
					throw new NotImplementedException("Sync method not supported yet");
				case null:
					throw new InvalidOperationException($"Could not find an RenderText method on ViewComponent {textComponent}");
			}

			var paramObject = ModuleParamHelpers
				.GetParameterData(w, name, invokeMethod, wikiPage, pp);

			var module = serviceProvider.GetRequiredService(textComponent);
			var result = await (Task<string>)invokeMethod.Invoke(module, [.. paramObject.Values])!;
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
				return host.TrimEnd('/') + url;
			}

			return url;
		}
	}
}
