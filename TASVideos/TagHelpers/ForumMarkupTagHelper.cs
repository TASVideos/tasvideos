using Microsoft.AspNetCore.Razor.TagHelpers;
using TASVideos.ForumEngine;

namespace TASVideos.TagHelpers
{
	public class ForumMarkupTagHelper : TagHelper
	{
		public string Markup { get; set; } = "";
		public bool EnableHtml { get; set; }
		public bool EnableBbCode { get; set; }

		public override void Process(TagHelperContext context, TagHelperOutput output)
		{
			var parsed = PostParser.Parse(Markup, EnableBbCode, EnableHtml);
			output.TagName = "div";
			output.AddCssClass("postbody");
			parsed.WriteHtml(new TagHelperTextWriter(output.Content));
		}
	}
}
