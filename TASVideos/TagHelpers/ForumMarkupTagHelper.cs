using Microsoft.AspNetCore.Razor.TagHelpers;
using TASVideos.Common;
using TASVideos.ForumEngine;

namespace TASVideos.TagHelpers;

public class ForumMarkupTagHelper(IWriterHelper helper) : TagHelper
{
	public string? Markup { get; set; }
	public bool EnableHtml { get; set; }
	public bool EnableBbCode { get; set; }

	public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
	{
		var parsed = PostParser.Parse(Markup ?? "", EnableBbCode, EnableHtml);
		output.TagName = "div";
		output.AddCssClass("postbody");
		var htmlWriter = new HtmlWriter(new TagHelperTextWriter(output.Content));
		await parsed.WriteHtml(htmlWriter, helper);
		htmlWriter.AssertFinished();
	}
}
