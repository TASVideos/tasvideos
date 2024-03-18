using Microsoft.AspNetCore.Razor.TagHelpers;

namespace TASVideos.TagHelpers;

public class AuthorListTagHelper : TagHelper
{
	public IEnumerable<string>? Authors { get; set; } = [];
	public string? AdditionalAuthors { get; set; }

	public override void Process(TagHelperContext context, TagHelperOutput output)
	{
		output.TagName = "span";
		output.Content.SetHtmlContent(GetAuthorString());
	}

	private string GetAuthorString()
	{
		var authors = Authors ?? [];
		return string.Join(", ", authors.Concat((AdditionalAuthors ?? "").SplitWithEmpty(","))).LastCommaToAmpersand();
	}
}
