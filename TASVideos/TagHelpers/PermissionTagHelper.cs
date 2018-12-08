using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using TASVideos.Data.Entity;
using TASVideos.Extensions;

namespace TASVideos.TagHelpers
{
	[HtmlTargetElement(Attributes = nameof(Permission))]
	public class PermissionTagHelper : TagHelper
	{
		public PermissionTo Permission { get; set; }

		[HtmlAttributeNotBound]
		[ViewContext]
		public ViewContext ViewContext { get; set; }

		public override void Process(TagHelperContext context, TagHelperOutput output)
		{
			if (!ViewContext.ViewData.UserHas(Permission))
			{
				output.SuppressOutput();
			}
		}
	}
}
