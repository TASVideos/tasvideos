using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using TASVideos.Data.Entity;

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
		if (!ViewContext.ViewData.UserHas(Permission))
		{
			output.SuppressOutput();
		}
	}
}

[HtmlTargetElement(Attributes = nameof(Permissions))]
public class PermissionsTagHelper : TagHelper
{
	public PermissionTo[] Permissions { get; set; } = Array.Empty<PermissionTo>();

	[HtmlAttributeNotBound]
	[ViewContext]
	public ViewContext ViewContext { get; set; } = new();

	public override void Process(TagHelperContext context, TagHelperOutput output)
	{
		if (!ViewContext.ViewData.UserHasAny(Permissions))
		{
			output.SuppressOutput();
		}
	}
}
