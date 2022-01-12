using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Razor.TagHelpers;
using TASVideos.Extensions;

namespace TASVideos.TagHelpers
{
	public class AuthorListTagHelper : TagHelper
	{
		public IEnumerable<string>? Authors { get; set; } = new List<string>();
		public string? AdditionalAuthors { get; set; }

		public override void Process(TagHelperContext context, TagHelperOutput output)
		{
			output.TagName = "span";
			output.Content.SetHtmlContent(GetAuthorString());
		}

		private string GetAuthorString()
		{
			var authors = Authors ?? Enumerable.Empty<string>();
			return string.Join(", ", authors.Concat((AdditionalAuthors ?? "").SplitWithEmpty(","))).LastCommaToAmpersand();
		}
	}
}
