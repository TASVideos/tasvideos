using System.Text;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace TASVideos.TagHelpers;

/// <summary>
/// TextWriter that wraps a TagHelperContent
/// </summary>
public class TagHelperTextWriter(TagHelperContent content) : TextWriter
{
	public override Encoding Encoding => Encoding.Unicode;

	public override void Write(char value)
	{
		content.AppendHtml(new string(value, 1));
	}

	public override void Write(string? value)
	{
		content.AppendHtml(value);
	}

	public override void Write(char[] buffer, int index, int count)
	{
		content.AppendHtml(new string(buffer, index, count));
	}

	public override void Write(ReadOnlySpan<char> buffer)
	{
		content.AppendHtml(new string(buffer));
	}
}
