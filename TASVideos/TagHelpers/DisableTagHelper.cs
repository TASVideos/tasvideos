using Microsoft.AspNetCore.Razor.TagHelpers;

namespace TASVideos.TagHelpers
{
	[HtmlTargetElement(Attributes = nameof(Disable))]
	public class DisableTagHelper : TagHelper
	{
		public bool Disable { get; set; }

		public override void Process(TagHelperContext context, TagHelperOutput output)
		{
			if (Disable)
			{
				output.Attributes.Add("disabled", "disabled");
			}
		}
	}
}
