using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace TASVideos.TagHelpers;

public class AddLinkTagHelper(IHtmlGenerator generator) : AnchorTagHelper(generator)
{
	public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
	{
		output.TagName = "a";
		await base.ProcessAsync(context, output);
		output.AddCssClass("btn");
		output.AddCssClass("btn-primary");
		var content = (await output.GetChildContentAsync()).GetContent();
		if (string.IsNullOrWhiteSpace(content))
		{
			output.Content.AppendHtml("<i class=\"fa fa-plus\"></i> Add");
		}
	}
}
