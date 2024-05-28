using Microsoft.AspNetCore.Razor.TagHelpers;

namespace TASVideos.TagHelpers;

// A work-around for the fact that tools do not understand we are building RSS and not HTML, this makes errors and warnings go away
public class RssTagHelper : TagHelper
{
	public override void Process(TagHelperContext context, TagHelperOutput output)
	{
		output.TagName = "rss";
		output.Attributes.Add("version", "2.0");
		output.Attributes.Add("xmlns:atom", "http://www.w3.org/2005/Atom");
		output.Attributes.Add("xmlns:media", "http://search.yahoo.com/mrss/");
	}
}
