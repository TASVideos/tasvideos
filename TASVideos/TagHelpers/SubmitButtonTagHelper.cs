using System;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace TASVideos.TagHelpers;

public class SubmitButtonTagHelper : TagHelper
{
	public override void Process(TagHelperContext context, TagHelperOutput output)
	{
		var guid = Guid.NewGuid();
		output.TagName = "button";
		output.Attributes.Add("type", "submit");
		output.Attributes.Add("data-submit-id", guid);
		output.PostElement.AppendHtml($@"
<script>
	document.querySelector('[data-submit-id=""{guid}""]').onclick = function () {{
		let btn = this;
		setTimeout(function () {{ btn.disabled = true }}, 0);
		setTimeout(function () {{ btn.disabled = false }}, 750);
	}}
</script>");
	}
}
