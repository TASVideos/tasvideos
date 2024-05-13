using Microsoft.AspNetCore.Razor.TagHelpers;

namespace TASVideos.TagHelpers;

[HtmlTargetElement(Attributes = nameof(Readonly))]
public class ReadonlyTagHelper : TagHelper
{
	public bool Readonly { get; set; }

	public override void Process(TagHelperContext context, TagHelperOutput output)
	{
		if (!Readonly)
		{
			return;
		}

		output.Attributes.Add("readonly", "readonly");
		output.Attributes.Add("aria-readonly", "true");
		var tabIndex = output.Attributes.FirstOrDefault(a => a.Name == "tabindex");
		if (tabIndex is not null)
		{
			output.Attributes.Remove(tabIndex);
		}

		output.Attributes.Add("tabindex", "-1");
		output.AddCssClass("disabled");
	}
}
