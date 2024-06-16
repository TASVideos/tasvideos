using Microsoft.AspNetCore.Razor.TagHelpers;

namespace TASVideos.TagHelpers;

public class MinorEditTagHelper : TagHelper
{
	public bool Checked { get; set; }

	public override void Process(TagHelperContext context, TagHelperOutput output)
	{
		output.TagName = "label";
		var checkedStr = Checked ? "checked" : "";
		output.Content.SetHtmlContent(
			$"""
			<input name="MinorEdit" type="checkbox" class="form-check-input" {checkedStr}/> Minor Edit
			""");
	}
}
