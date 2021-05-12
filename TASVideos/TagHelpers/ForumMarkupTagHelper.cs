using System.Threading.Tasks;
using Microsoft.AspNetCore.Razor.TagHelpers;
using TASVideos.ForumEngine;

namespace TASVideos.RazorPages.TagHelpers
{
	public class ForumMarkupTagHelper : TagHelper
	{
		private readonly IWriterHelper _helper;

		public string Markup { get; set; } = "";
		public bool EnableHtml { get; set; }
		public bool EnableBbCode { get; set; }

		public ForumMarkupTagHelper(IWriterHelper helper)
		{
			_helper = helper;
		}

		public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
		{
			var parsed = PostParser.Parse(Markup, EnableBbCode, EnableHtml);
			output.TagName = "div";
			output.AddCssClass("postbody");
			await parsed.WriteHtml(new TagHelperTextWriter(output.Content), _helper);
		}
	}
}
