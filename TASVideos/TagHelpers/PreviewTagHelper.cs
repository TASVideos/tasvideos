using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace TASVideos.TagHelpers;

public class PreviewTagHelper : TagHelper
{
	public virtual string PreviewPath { get; set; } = "";

	[HtmlAttributeNotBound]
	[ViewContext]
	public ViewContext ViewContext { get; set; } = new();

	public override void Process(TagHelperContext context, TagHelperOutput output)
	{
		output.TagName = "div";
		output.Attributes.Add("id", "preview-container");
		output.Attributes.Add("data-path", PreviewPath);
		output.AddCssClass("d-none");
		output.Content.AppendHtml(
			"""
			<br/>
			<div class="card">
				<div class="card-header">Preview:</div>
				<div id="preview-contents" class="card-body"></div>
			</div>
			""");

		ViewContext.ViewData.UsePreview();
	}
}

public sealed class WikiPreviewTagHelper : PreviewTagHelper
{
	public override string PreviewPath { get; set; } = "/Wiki/Preview";
}

public sealed class ForumPreviewTagHelper : PreviewTagHelper
{
	public override string PreviewPath { get; set; } = "/Forum/Preview";
}
