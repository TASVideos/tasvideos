using System.Net;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace TASVideos.TagHelpers;

[HtmlTargetElement("html-encode", TagStructure = TagStructure.NormalOrSelfClosing)]
public class HtmlEncodeTagHelper : TagHelper
{
	public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
	{
		var childContent = output.Content.IsModified
			? output.Content.GetContent()
			: (await output.GetChildContentAsync()).GetContent();

		string encodedChildContent = WebUtility.HtmlEncode(childContent ?? "");

		output.TagName = null;
		output.Content.SetHtmlContent(encodedChildContent);
	}
}
