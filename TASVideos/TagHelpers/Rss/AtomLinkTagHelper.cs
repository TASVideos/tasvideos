using Microsoft.AspNetCore.Razor.TagHelpers;

namespace TASVideos.TagHelpers;

[HtmlTargetElement("atom-link", TagStructure = TagStructure.WithoutEndTag)]
public class AtomLinkTagHelper : TagHelper
{
	public override void Process(TagHelperContext context, TagHelperOutput output)
	{
		output.TagName = "atom:link";
		output.Attributes.Add("rel", "self");
		output.Attributes.Add("type", "application/rss+xml");
		output.Attributes.Add("xmlns", "atom");
	}
}
