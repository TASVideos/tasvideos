using Microsoft.AspNetCore.Razor.TagHelpers;

namespace TASVideos.TagHelpers;

public class TableHeadTagHelper : TagHelper
{
	public string Columns { get; set; } = "";

	public override void Process(TagHelperContext context, TagHelperOutput output)
	{
		output.TagName = "thead";

		var columns = Columns.Split(",");
		foreach (var column in columns)
		{
			output.Content.AppendHtml($"<th>{column}</th>");
		}
	}
}
