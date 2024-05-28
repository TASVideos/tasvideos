using Microsoft.AspNetCore.Razor.TagHelpers;

namespace TASVideos.TagHelpers;

// A work-around for the fact that tools do not understand we are building RSS and not HTML, this makes errors and warnings go away
public class RssLinkTagHelper : TagHelper
{
	public override void Process(TagHelperContext context, TagHelperOutput output)
	{
		output.TagName = "link";
	}
}
