using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace TASVideos.TagHelpers;

public class ShowMoreTagHelper : TagHelper
{
	public string MaxHeight { get; set; } = "none";
	public string ShowText { get; set; } = "Show more";
	public string HideText { get; set; } = "Hide";
	public bool Reverse { get; set; } = false;

	[HtmlAttributeNotBound]
	[ViewContext]
	public ViewContext ViewContext { get; set; } = new();

	public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
	{
		ViewContext.ViewData.UseShowMore();
		var content = (await output.GetChildContentAsync()).GetContent();
		output.TagName = "div";
		output.Content.SetHtmlContent(
			$"""
			<div style="overflow-y: scroll; max-height: {MaxHeight};" data-show-more="true" data-reverse="{Reverse.ToString().ToLower()}" id="{context.UniqueId}">{content}</div>
			""");
		output.Content.AppendHtml(
			$"""
			<div class="p-2 text-center d-none border border-primary rounded" id="show-{context.UniqueId}">
				<a href="#"><h4 class="m-0"><i class="fa fa-chevron-{(Reverse ? "up" : "down")}"></i> {ShowText}</h4></a>
			</div>
			<div class="p-2 text-center d-none border border-primary rounded" id="hide-{context.UniqueId}">
				<a href="#"><h4 class="m-0"><i class="fa fa-chevron-{(Reverse ? "down" : "up")}"></i> {HideText}</h4></a>
			</div>
			""");
	}
}
