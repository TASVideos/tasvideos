using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using TASVideos.Data.Entity;
using TASVideos.Extensions;
using TASVideos.Services;
using TASVideos.WikiEngine;
using TASVideos.WikiEngine.AST;

namespace TASVideos.TagHelpers;

public partial class WikiMarkup : TagHelper, IWriterHelper
{
	private readonly IViewComponentHelper _viewComponentHelper;

	public WikiMarkup(IViewComponentHelper viewComponentHelper)
	{
		_viewComponentHelper = viewComponentHelper;
	}

	[ViewContext]
	[HtmlAttributeNotBound]
	public ViewContext ViewContext { get; set; } = new();

	public string Markup { get; set; } = "";
	public WikiPage PageData { get; set; } = new();

	public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
	{
		((IViewContextAware)_viewComponentHelper).Contextualize(ViewContext);
		output.TagName = "article";
		output.AddCssClass("wiki");
		await Util.RenderHtmlAsync(Markup, new TagHelperTextWriter(output.Content), this);
	}

	bool IWriterHelper.CheckCondition(string condition)
	{
		return HtmlExtensions.WikiCondition(ViewContext, condition);
	}

	async Task IWriterHelper.RunViewComponentAsync(TextWriter w, string name, IReadOnlyDictionary<string, string> pp)
	{
		var componentExists = ModuleParamHelpers.ViewComponents.TryGetValue(name, out Type? viewComponent);
		if (!componentExists)
		{
			throw new InvalidOperationException($"Unknown ViewComponent: {name}");
		}

		var invokeMethod = viewComponent!.GetMethod("InvokeAsync")
			?? viewComponent.GetMethod("Invoke");

		if (invokeMethod == null)
		{
			throw new InvalidOperationException($"Could not find an Invoke method on ViewComponent {viewComponent}");
		}

		var paramObject = ModuleParamHelpers
			.GetParameterData(w, name, invokeMethod, PageData, pp);

		var content = await _viewComponentHelper.InvokeAsync(viewComponent, paramObject);
		content.WriteTo(w, HtmlEncoder.Default);
	}

	// In the website proper, we don't need to absoluteify URLs.
	string IWriterHelper.AbsoluteUrl(string url) => url;
}
