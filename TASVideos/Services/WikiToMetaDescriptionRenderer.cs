using System.Text;
using TASVideos.Core.Services.Wiki;
using TASVideos.WikiEngine;
using TASVideos.WikiEngine.AST;

namespace TASVideos.Services;

public class WikiToMetaDescriptionRenderer(IServiceProvider serviceProvider) : IWikiToMetaDescriptionRenderer
{
	public async Task<string> RenderWikiForMetaDescription(IWikiPage page)
	{
		var sb = new StringBuilder();
		await Util.RenderMetaDescriptionAsync(page.Markup, sb, new WriterHelper(serviceProvider, page));
		return sb.ToString().Trim();
	}

	private class WriterHelper(IServiceProvider serviceProvider, IWikiPage wikiPage)
		: IWriterHelper
	{
		public string AbsoluteUrl(string url)
		{
			return url;
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
			var componentExists = ModuleParamHelpers.MetaDescriptionComponents.TryGetValue(name, out Type? metaDescriptionComponent);
			if (!componentExists)
			{
				return;
			}

			var invokeMethod = metaDescriptionComponent!.GetMethod("RenderMetaDescriptionAsync");

			switch (invokeMethod)
			{
				case null when metaDescriptionComponent.GetMethod("RenderMetaDescription") is not null:
					throw new NotImplementedException("Sync method not supported yet");
				case null:
					throw new InvalidOperationException($"Could not find an RenderMetaDescription method on ViewComponent {metaDescriptionComponent}");
			}

			var paramObject = ModuleParamHelpers
				.GetParameterData(w, name, invokeMethod, wikiPage, pp);

			var module = serviceProvider.GetRequiredService(metaDescriptionComponent);
			var result = await (Task<string>)invokeMethod.Invoke(module, [.. paramObject.Values])!;
			await w.WriteAsync(result);
		}
	}
}
