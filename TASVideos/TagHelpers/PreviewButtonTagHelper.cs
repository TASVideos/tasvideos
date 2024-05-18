using Microsoft.AspNetCore.Razor.TagHelpers;

namespace TASVideos.TagHelpers;

public class PreviewButtonTagHelper : TagHelper
{
	public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
	{
		output.TagName = "button";
		output.Attributes.Add("type", "button");
		output.Attributes.Add("id", "preview-button");
		output.AddCssClass("btn");
		output.AddCssClass("btn-secondary");

		var content = (await output.GetChildContentAsync()).GetContent();
		if (string.IsNullOrWhiteSpace(content))
		{
			output.Content.AppendHtml("<i class=\"fa fa-eye\"></i> Preview");
		}
	}
}
