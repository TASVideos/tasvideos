using System.Collections.Immutable;

using Microsoft.AspNetCore.Razor.TagHelpers;

namespace TASVideos.RazorPages.Tests;

public abstract class LinkTagHelperTestsBase
{
	public static TagHelperContext GetHelperContext()
		=> new(
			[],
			ImmutableDictionary<object, object>.Empty,
			Guid.NewGuid().ToString("N"));

	public static string GetHtmlString(TagHelperOutput output)
	{
		using var writer = new StringWriter();
		output.WriteTo(writer, NullHtmlEncoder.Default);
		return writer.ToString();
	}

	public static TagHelperOutput GetOutputObj(string contentsUnencoded, string tagName = "")
	{
		TagHelperOutput output = new(
			tagName,
			[],
			(_, _) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent()));
		output.Content.SetContent(contentsUnencoded);
		return output;
	}
}
