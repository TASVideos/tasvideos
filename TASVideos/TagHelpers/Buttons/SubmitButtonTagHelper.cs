using Microsoft.AspNetCore.Razor.TagHelpers;

namespace TASVideos.TagHelpers;

public class SubmitButtonTagHelper : TagHelper
{
	public string? BtnClassOverride { get; set; }

	public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
	{
		output.TagName = "button";
		output.Attributes.Add("type", "submit");
		output.AddCssClass("btn");

		output.AddCssClass(string.IsNullOrEmpty(BtnClassOverride)
			? "btn-primary"
			: BtnClassOverride);

		var content = (await output.GetChildContentAsync()).GetContent();
		if (string.IsNullOrWhiteSpace(content))
		{
			output.Content.AppendHtml("<i class=\"fa fa-save\"></i> Save");
		}
	}
}
