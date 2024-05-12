using Microsoft.AspNetCore.Razor.TagHelpers;

namespace TASVideos.TagHelpers;

public class StandardTableTagHelper : TagHelper
{
	public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
	{
		output.TagName = "div";
		output.AddCssClass("table-responsive");

		var content = (await output.GetChildContentAsync()).GetContent();
		output.Content.SetHtmlContent(
			$"""
			<table class='table table-sm table-bordered table-striped'>
				{content}
			</table>
			""");
	}
}
