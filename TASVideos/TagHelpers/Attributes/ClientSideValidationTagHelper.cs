using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace TASVideos.TagHelpers;

[HtmlTargetElement("form", Attributes = "client-side-validation")]
public class ClientSideValidationTagHelper : TagHelper
{
	[HtmlAttributeName("client-side-validation")]
	public bool ClientSideValidation { get; set; }

	[HtmlAttributeNotBound]
	[ViewContext]
	public ViewContext ViewContext { get; set; } = new();

	public override void Process(TagHelperContext context, TagHelperOutput output)
	{
		if (ClientSideValidation)
		{
			ViewContext.ViewData.EnableClientSideValidation();
		}
	}
}
