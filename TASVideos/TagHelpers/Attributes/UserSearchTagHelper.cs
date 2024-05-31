using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace TASVideos.TagHelpers;

[HtmlTargetElement("input", Attributes = "user-search")]
public class UserSearchTagHelper : TagHelper
{
	[HtmlAttributeName("user-search")]
	public bool UserSearch { get; set; }

	[HtmlAttributeNotBound]
	[ViewContext]
	public ViewContext ViewContext { get; set; } = new();

	public override void Process(TagHelperContext context, TagHelperOutput output)
	{
		if (UserSearch)
		{
			output.Attributes.Add("data-user-search", "true");
			output.Attributes.Add("autocomplete", "off");
			output.Attributes.Add("spellcheck", "false");

			ViewContext.ViewData.UseUserSearch();
		}
	}
}
