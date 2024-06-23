using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace TASVideos.TagHelpers;

public class WikiTextareaTagHelper(IHtmlGenerator generator) : TextAreaTagHelper(generator)
{
	public override void Process(TagHelperContext context, TagHelperOutput output)
	{
		output.TagName = "textarea";
		output.Attributes.Add("placeholder", "Enter your __wiki markup__ here...");
		output.Attributes.Add("data-id", "wiki-edit");
		base.Process(context, output);
	}
}
