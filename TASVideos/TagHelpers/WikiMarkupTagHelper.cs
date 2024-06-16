using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using TASVideos.Core.Services.Wiki;
using TASVideos.WikiEngine;
using TASVideos.WikiEngine.AST;

namespace TASVideos.TagHelpers;

public partial class WikiMarkup(IViewComponentHelper viewComponentHelper) : TagHelper, IWriterHelper
{
	[ViewContext]
	[HtmlAttributeNotBound]
	public ViewContext ViewContext { get; set; } = new();

	public string? Markup { get; set; }
	public IWikiPage? PageData { get; set; }

	public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
	{
		((IViewContextAware)viewComponentHelper).Contextualize(ViewContext);
		output.TagName = "article";
		output.AddCssClass("wiki");
		await Util.RenderHtmlAsync(Markup ?? "", new TagHelperTextWriter(output.Content), this);
	}

	bool IWriterHelper.CheckCondition(string condition)
	{
		return ViewContext.WikiCondition(condition);
	}

	async Task IWriterHelper.RunViewComponentAsync(TextWriter w, string name, IReadOnlyDictionary<string, string> pp)
	{
		var componentExists = ModuleParamHelpers.ViewComponents.TryGetValue(name, out Type? viewComponent);
		if (!componentExists)
		{
			throw new InvalidOperationException($"Unknown ViewComponent: {name}");
		}

		var invokeMethod = (viewComponent!.GetMethod("InvokeAsync")
			?? viewComponent.GetMethod("Invoke"))
			?? throw new InvalidOperationException($"Could not find an Invoke method on ViewComponent {viewComponent}");

		var paramObject = ModuleParamHelpers
			.GetParameterData(w, name, invokeMethod, PageData, pp);

		var content = await viewComponentHelper.InvokeAsync(viewComponent, paramObject);
		content.WriteTo(w, HtmlEncoder.Default);
	}

	// In the website proper, we don't need to absoluteify URLs.
	string IWriterHelper.AbsoluteUrl(string url) => url;
}
