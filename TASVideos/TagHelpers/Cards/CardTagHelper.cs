using Microsoft.AspNetCore.Razor.TagHelpers;

namespace TASVideos.TagHelpers;

public class CardTagHelper : TagHelper
{
	public bool UseArticle { get; set; } = false;

	public override void Process(TagHelperContext context, TagHelperOutput output)
	{
		output.TagName = UseArticle ? "article" : "div";
		output.AddCssClass("card");
	}
}
