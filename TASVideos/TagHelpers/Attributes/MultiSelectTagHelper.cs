using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace TASVideos.TagHelpers;

[HtmlTargetElement("select", Attributes = "multiselect")]
public class MultiselectTagHelper : TagHelper
{
	[HtmlAttributeName("multiselect")]
	public bool Multiselect { get; set; }

	[HtmlAttributeNotBound]
	[ViewContext]
	public ViewContext ViewContext { get; set; } = new();

	public override int Order => 1; // needs to be executed after other TagHelpers

	public override void Process(TagHelperContext context, TagHelperOutput output)
	{
		if (Multiselect)
		{
			TagBuilder selectTag = new("select");
			selectTag.MergeAttributes(output.Attributes.ToDictionary(a => a.Name, a => a.Value.ToString()));
			HtmlContentBuilder tempContentBuilder = new();
			output.PostContent.MoveTo(tempContentBuilder);
			selectTag.InnerHtml.AppendHtml(tempContentBuilder);

			TagBuilder noscriptTag = new("noscript");
			noscriptTag.InnerHtml.AppendHtml(selectTag);

			TagBuilder templateTag = new("template");
			templateTag.Attributes.Add("data-multiselect", "true");
			templateTag.InnerHtml.AppendHtml(selectTag);

			output.TagName = "div";
			output.Attributes.Clear();
			output.Content.SetHtmlContent(noscriptTag);
			output.Content.AppendHtml(templateTag);

			ViewContext.ViewData.UseSelectImprover();
		}
	}
}
