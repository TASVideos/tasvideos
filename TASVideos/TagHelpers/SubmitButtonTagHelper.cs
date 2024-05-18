using Microsoft.AspNetCore.Razor.TagHelpers;

namespace TASVideos.TagHelpers;

public class SubmitButtonTagHelper : TagHelper
{
	public string? BtnClassOverride { get; set; }

	public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
	{
		var guid = Guid.NewGuid();
		output.TagName = "button";
		output.Attributes.Add("type", "submit");
		output.Attributes.Add("data-submit-id", guid);
		output.PostElement.AppendHtml(
			$$"""
				<script>
					document.querySelector('[data-submit-id="{{guid}}"]').onclick = function () {
						let btn = this;
						setTimeout(function () { btn.disabled = true }, 0);
						setTimeout(function () { btn.disabled = false }, 750);
					}
				</script>
				""");

		output.AddCssClass("btn");

		output.AddCssClass(string.IsNullOrEmpty(BtnClassOverride)
			? "btn-primary"
			: BtnClassOverride);

		var content = (await output.GetChildContentAsync()).GetContent();
		if (string.IsNullOrWhiteSpace(content))
		{
			output.Content.AppendHtml("<i class=\"fa fa-save\"></i> Save");
		}
	}
}
