using Microsoft.AspNetCore.Razor.TagHelpers;

namespace TASVideos.TagHelpers
{
	[HtmlTargetElement(Attributes = nameof(Select))]
	public class SelectTagHelper : TagHelper
	{
		public bool Select { get; set; }

		public override void Process(TagHelperContext context, TagHelperOutput output)
		{
			if (Select)
			{
				output.Attributes.Add("selected", "selected");
			}
		}
	}
}
