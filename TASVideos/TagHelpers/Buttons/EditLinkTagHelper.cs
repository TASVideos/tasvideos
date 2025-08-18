using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace TASVideos.TagHelpers;

public class EditLinkTagHelper(IHtmlGenerator generator) : AnchorTagHelper(generator)
{
	public override void Process(TagHelperContext context, TagHelperOutput output)
		=> ProcessAsync(context, output).Wait();

	public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
	{
		output.TagName = "a";
		base.Process(context, output);
		output.AddCssClass("btn");
		output.AddCssClass("btn-primary");
		var content = (await output.GetChildContentAsync()).GetContent();
		if (string.IsNullOrWhiteSpace(content))
		{
			output.Content.AppendHtml("<i class=\"fa fa-pencil\"></i> Edit");
		}
	}
}
