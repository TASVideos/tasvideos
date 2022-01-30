using System.Text;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace TASVideos.TagHelpers;

/// <summary>
/// TextWriter that wraps a TagHelperContent
/// </summary>
public class TagHelperTextWriter : TextWriter
{
	private readonly TagHelperContent _content;

	public TagHelperTextWriter(TagHelperContent content)
	{
		_content = content;
	}

	public override Encoding Encoding => Encoding.Unicode;

	public override void Write(char value)
	{
		_content.AppendHtml(new string(value, 1));
	}

	public override void Write(string? value)
	{
		_content.AppendHtml(value);
	}

	public override void Write(char[] buffer, int index, int count)
	{
		_content.AppendHtml(new string(buffer, index, count));
	}

	public override void Write(ReadOnlySpan<char> buffer)
	{
		_content.AppendHtml(new string(buffer));
	}
}
