using Microsoft.AspNetCore.Razor.TagHelpers;

namespace TASVideos.TagHelpers
{
	[HtmlTargetElement(Attributes = nameof(Show))]
	public class ShowTagHelper : TagHelper
	{
		public bool Show { get; set; }

		public override void Process(TagHelperContext context, TagHelperOutput output)
		{
			if (!Show)
			{
				output.AddCssClass("d-none");
			}
		}
	}
}
