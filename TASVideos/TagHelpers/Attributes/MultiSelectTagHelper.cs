using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace TASVideos.TagHelpers;

[HtmlTargetElement("select", Attributes = "multiselect")]
public class MultiselectTagHelper : TagHelper
{
	[HtmlAttributeName("multiselect")]
	public bool Multiselect { get; set; }

	[HtmlAttributeNotBound]
	[ViewContext]
	public ViewContext ViewContext { get; set; } = new();

	public override void Process(TagHelperContext context, TagHelperOutput output)
	{
		if (Multiselect)
		{
			output.Attributes.Add("data-multiselect", "true");
			output.AddCssClass("d-none-except-noscript");
			ViewContext.ViewData.UseSelectImprover();
		}
	}
}
