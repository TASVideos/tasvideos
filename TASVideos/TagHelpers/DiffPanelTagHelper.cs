using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace TASVideos.TagHelpers;

public class DiffPanelTagHelper : TagHelper
{
	[HtmlAttributeNotBound]
	[ViewContext]
	public ViewContext ViewContext { get; set; } = new();

	public override void Process(TagHelperContext context, TagHelperOutput output)
	{
		ViewContext.ViewData.UseDiff();
		output.TagName = "div";

		output.Content.AppendHtml(
			"""
			<div id="diff-view" class="mt-3 border border-secondary d-none"></div>
			<div id="diff-options" class="d-none py-3">
				<label><input name="diff-type" type="radio" value="1" checked="checked" /> Inline</label>
				<label><input name="diff-type" type="radio" value="0" /> Side by Side</label>
				<label><input name="context-size" type="number" value="5" min="0" max="9999" /> Context Size</label>
			</div>
			""");
	}
}
