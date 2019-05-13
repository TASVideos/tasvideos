using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Razor.TagHelpers;
namespace TASVideos.TagHelpers
{
	[HtmlTargetElement("card-link", TagStructure = TagStructure.WithoutEndTag)]
	public class CardLinkTagHelper : TagHelper
	{
		public string Href { get; set; }

		public string Header { get; set; }

		public string Body { get; set; }

		public override void Process(TagHelperContext context, TagHelperOutput output)
		{
			output.TagMode = TagMode.StartTagAndEndTag;
			output.TagName = "div";
			output.AddCssClass("card-link");

			output.Content.AppendHtml($@"
<a class='card-link-anchor' href='{Href}'>
	<h4 class='card-link-header'>
		{Header}
	</h4>
	<span class='card-link-body'>
		{Body}
	</span>
	<span class='card-link-arrow fa fa-chevron-right'></span>
</a>
");
		}
	}
}
