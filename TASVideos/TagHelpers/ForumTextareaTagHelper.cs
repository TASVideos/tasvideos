using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace TASVideos.TagHelpers;

public class ForumTextareaTagHelper(IHtmlGenerator generator) : TextAreaTagHelper(generator)
{
	public override void Process(TagHelperContext context, TagHelperOutput output)
	{
		output.TagName = "textarea";
		output.Attributes.Add("placeholder", "Enter your [b]bbcode[/b] here...");
		output.Attributes.Add("data-id", "forum-edit");
		base.Process(context, output);
	}
}
