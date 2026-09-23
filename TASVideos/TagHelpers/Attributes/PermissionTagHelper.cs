using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace TASVideos.TagHelpers;

[HtmlTargetElement(Attributes = nameof(Permission))]
public class PermissionTagHelper : TagHelper
{
	public override int Order => 10; // output suppression must happen last

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
	}
}

[HtmlTargetElement(Attributes = nameof(Permission))]
public class PermissionResultSetterTagHelper : PermissionTagHelper
{
	public override int Order => -10; // this makes sure this runs before other tag helpers. they can avoid creating antiforgery tokens that don't get used.

	public override void Process(TagHelperContext context, TagHelperOutput output)
	{
		context.SetPermissionResult(ViewContext.HttpContext.User.Has(Permission));
	}
}

[HtmlTargetElement(Attributes = nameof(Permissions))]
public class PermissionsTagHelper : TagHelper
{
	public override int Order => 10; // output suppression must happen last

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

[HtmlTargetElement(Attributes = nameof(Permissions))]
public class PermissionsResultSetterTagHelper : PermissionsTagHelper
{
	public override int Order => -10; // this makes sure this runs before other tag helpers. they can avoid creating antiforgery tokens that don't get used.

	public override void Process(TagHelperContext context, TagHelperOutput output)
	{
		context.SetPermissionResult(ViewContext.HttpContext.User.HasAny(Permissions));
	}
}
