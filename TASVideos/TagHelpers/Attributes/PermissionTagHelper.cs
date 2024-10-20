using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace TASVideos.TagHelpers;

[HtmlTargetElement(Attributes = nameof(Permission))]
public class PermissionTagHelper : TagHelper
{
	public PermissionTo Permission { get; set; }

	[HtmlAttributeNotBound]
	[ViewContext]
	public ViewContext ViewContext { get; set; } = new();

	public override void Process(TagHelperContext context, TagHelperOutput output)
	{
		if (!ViewContext.HttpContext.User.Has(Permission))
		{
			output.SuppressOutput();
		}
		else if (context.TagName is "a")
		{
			// this hyperlink is gated behind a permission (it may lead to a form, for example), so bots needn't bother loading it
			// presumably crawlers aren't authenticated, but maybe a user has some sort of "eager preload" extension
			output.Attributes.Add("rel", "nofollow");
		}
	}
}

[HtmlTargetElement(Attributes = nameof(Permissions))]
public class PermissionsTagHelper : TagHelper
{
	public PermissionTo[] Permissions { get; set; } = [];

	[HtmlAttributeNotBound]
	[ViewContext]
	public ViewContext ViewContext { get; set; } = new();

	public override void Process(TagHelperContext context, TagHelperOutput output)
	{
		if (!ViewContext.HttpContext.User.HasAny(Permissions))
		{
			output.SuppressOutput();
		}
	}
}
