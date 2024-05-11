using Microsoft.AspNetCore.Razor.TagHelpers;

namespace TASVideos.TagHelpers;

public class StandardTableTagHelper : TagHelper
{
	public override void Process(TagHelperContext context, TagHelperOutput output)
	{
		output.TagName = "table";
		output.AddCssClass("table");
		output.AddCssClass("table-sm");
		output.AddCssClass("table-bordered");
		output.AddCssClass("table-striped");
		output.AddCssClass("table-responsive");
	}
}
