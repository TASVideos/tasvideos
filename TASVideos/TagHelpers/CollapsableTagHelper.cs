using System;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Razor.TagHelpers;
using static TASVideos.TagHelpers.TagHelperExtensions;

namespace TASVideos.TagHelpers;

[HtmlTargetElement("collapsablecontent-header")]
public class CollapsableHeaderTagHelper : TagHelper
{
	public string BodyId { get; set; } = "";

	public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
	{
		var content = (await output.GetChildContentAsync()).GetContent();

		output.TagName = "div";

		// TODO: sr and aria tags could use something more informative than BodyId
		output.Content.AppendHtml($@"
				<a class='collapsed' data-bs-toggle='collapse' {Attr("href", "#" + BodyId)} aria-expanded='false' aria-controls='collapse1' role='button'>
					{content}
				</a>
				<a
					data-bs-toggle='collapse'
					class='collapsed btn btn-default btn-xs text-end'
					{Attr("href", "#" + BodyId)}
					{Attr("aria-label", $"Expand/Collapse {BodyId}")}
					aria-expanded='false'
					role='button'
				>
					<i class='fa' aria-hidden='true'></i>
					<span class='sr-only'>Expand/Collapse {Text(BodyId)}</span>
				</a>
			");
	}
}

[HtmlTargetElement("collapsablecontent-body")]
public class CollapsableBodyTagHelper : TagHelper
{
	public bool StartShown { get; set; }

	public override void Process(TagHelperContext context, TagHelperOutput output)
	{
		output.TagName = "div";
		var id = output.Attributes.FirstOrDefault(a => a.Name.ToLower() == "id");
		if (id == null)
		{
			throw new InvalidOperationException("collapsablecontent-body requires an id attribute");
		}

		output.AddCssClass("collapse");
		if (StartShown)
		{
			output.AddCssClass("show");
		}
	}
}
