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
		output.AddCssClass("card mt-4");

		output.Content.AppendHtml(
			"""
			<div class="card-header" data-bs-toggle="collapse" data-bs-target="#diff-container">
				<a class="text-body collapsed" role="button"><i class="fa fa-chevron-circle-down"></i> Diff</a>
			</div>
			<div class="card-body py-0 bg-dark-subtle">
				<div id="diff-container" class="collapse">
					<div id="diff-view" class="border border-dark-subtle mt-3"></div>
					<div id="diff-options" class="py-3">
						<label><input name="diff-type" type="radio" value="1" checked="checked" /> Inline</label>
						<label><input name="diff-type" type="radio" value="0" /> Side by Side</label>
						<label><input name="context-size" type="number" value="5" min="0" max="9999" /> Context Size</label>
					</div>
				</div>
			</div>
			""");
	}
}
