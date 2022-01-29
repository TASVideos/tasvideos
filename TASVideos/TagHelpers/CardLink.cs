using Microsoft.AspNetCore.Razor.TagHelpers;
using static TASVideos.TagHelpers.TagHelperExtensions;

namespace TASVideos.TagHelpers;

[HtmlTargetElement("card-link", TagStructure = TagStructure.WithoutEndTag)]
public class CardLinkTagHelper : TagHelper
{
	public string Href { get; set; } = "";

	public string Header { get; set; } = "";

	public string Body { get; set; } = "";

	public override void Process(TagHelperContext context, TagHelperOutput output)
	{
		output.TagMode = TagMode.StartTagAndEndTag;
		output.TagName = "a";
		output.AddCssClass("card-link");
		output.Attributes.Add("href", Href);

		output.Content.AppendHtml($@"
	<h4 class='card-link-header'>
		{Text(Header)}
	</h4>
	<span class='card-link-body'>
		{Text(Body)}
	</span>
	<span class='card-link-arrow fa fa-chevron-right'></span>
");
	}
}
