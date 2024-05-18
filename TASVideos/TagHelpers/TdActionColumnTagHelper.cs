using Microsoft.AspNetCore.Razor.TagHelpers;

namespace TASVideos.TagHelpers;

public class TdActionColumnTagHelper : TagHelper
{
	public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
	{
		output.TagName = "td";
		var content = (await output.GetChildContentAsync()).GetContent();
		output.Content.SetHtmlContent(
			$"""
			<div class='action-column'>
				{content}
			</div>
			""");
	}
}
