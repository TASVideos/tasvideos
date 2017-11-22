using System.Linq;
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
				output.Attributes.Add("aria-disabled", "true");
				var tabIndex = output.Attributes.FirstOrDefault(a => a.Name == "tabindex");
				if (tabIndex != null)
				{
					output.Attributes.Remove(tabIndex);
				}

				output.Attributes.Add("tabindex", "-1");
			}
		}
	}
}
