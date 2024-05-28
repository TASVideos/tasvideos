using Microsoft.AspNetCore.Razor.TagHelpers;

namespace TASVideos.TagHelpers;

public class Fullrow : TagHelper
{
	public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
	{
		output.TagName = "div";
		output.AddCssClass("row");

		var content = (await output.GetChildContentAsync()).GetContent();
		output.Content.SetHtmlContent($"<div class='col-12'>{content}</div>");
	}
}
