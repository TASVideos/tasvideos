using System.Text.RegularExpressions;

namespace TASVideos.Common;

public class HtmlWriter(TextWriter w)
{
	private static readonly Regex AllowedTagNames = new("^[a-z0-9]+$");
	private static readonly Regex AllowedAttributeNames = new("^[a-z\\-]+$");
	private static readonly HashSet<string> VoidTags = new(
		new[]
		{
				"area", "base", "br", "col", "embed", "hr", "img", "input",
				"keygen", "link", "meta", "param", "source", "track", "wbr"
		},
		StringComparer.OrdinalIgnoreCase
	);

	private bool _inTagOpen;
	private readonly Stack<string> _openTags = new();

	private bool InForeignContent => _openTags.TryPeek(out var tag) && tag is "script" or "style";

	public void OpenTag(string tagName)
	{
		if (InForeignContent)
		{
			throw new InvalidOperationException("New tag not allowed at this time");
		}

		if (!AllowedTagNames.IsMatch(tagName))
		{
			throw new InvalidOperationException($"Invalid tag name {tagName}");
		}

		if (VoidTags.Contains(tagName))
		{
			throw new InvalidOperationException("Can't open a void tag");
		}

		if (_inTagOpen)
		{
			w.Write('>');
		}

		tagName = tagName.ToLowerInvariant();
		w.Write('<');
		w.Write(tagName);
		_openTags.Push(tagName);
		_inTagOpen = true;
	}

	public void CloseTag(string tagName)
	{
		if (_openTags.Count == 0)
		{
			throw new InvalidOperationException("No open tags!");
		}

		if (!tagName.Equals(_openTags.Peek(), StringComparison.OrdinalIgnoreCase))
		{
			throw new InvalidOperationException($"Opened tag {_openTags.Peek()} but closing tag {tagName}");
		}

		if (_inTagOpen)
		{
			w.Write('>');
		}

		tagName = tagName.ToLowerInvariant();
		w.Write("</");
		w.Write(tagName);
		w.Write('>');
		_openTags.Pop();
		_inTagOpen = false;
	}

	public void VoidTag(string tagName)
	{
		if (InForeignContent)
		{
			throw new InvalidOperationException("New tag not allowed at this time");
		}

		if (!AllowedTagNames.IsMatch(tagName))
		{
			throw new InvalidOperationException($"Invalid tag name {tagName}");
		}

		if (!VoidTags.Contains(tagName))
		{
			throw new InvalidOperationException("Can't void an open tag");
		}

		if (_inTagOpen)
		{
			w.Write('>');
		}

		tagName = tagName.ToLowerInvariant();
		w.Write('<');
		w.Write(tagName);
		_inTagOpen = true;
	}

	public void Attribute(string name, string value)
	{
		if (!_inTagOpen)
		{
			throw new InvalidOperationException("Not in the opening of a tag!");
		}

		if (!AllowedAttributeNames.IsMatch(name))
		{
			throw new InvalidOperationException($"Invalid attribute name {name}");
		}

		w.Write(' ');
		w.Write(name);
		w.Write("=\"");

		foreach (var c in value)
		{
			switch (c)
			{
				case '<':
					w.Write("&lt;");
					break;
				case '&':
					w.Write("&amp;");
					break;
				case '"':
					w.Write("&quot;");
					break;
				default:
					w.Write(c);
					break;
			}
		}

		w.Write('"');
	}

	public void Text(string text)
	{
		if (InForeignContent)
		{
			if (text.Contains($"</{_openTags.Peek()}", StringComparison.OrdinalIgnoreCase))
			{
				throw new InvalidOperationException("Can't unescape something that looks like closing tag here!");
			}

			if (_inTagOpen)
			{
				w.Write('>');
			}

			w.Write(text);
		}
		else
		{
			if (_inTagOpen)
			{
				w.Write('>');
			}

			foreach (var c in text)
			{
				switch (c)
				{
					case '<':
						w.Write("&lt;");
						break;
					case '&':
						w.Write("&amp;");
						break;
					default:
						w.Write(c);
						break;
				}
			}
		}

		_inTagOpen = false;
	}

	public void AssertFinished()
	{
		if (_openTags.Count > 0)
		{
			throw new InvalidOperationException("Tags still open!");
		}

		if (_inTagOpen)
		{
			w.Write('>');
		}
	}

	/// <summary>
	/// Gets the underlying writer
	/// Do not use this unless you're very careful with escaping!
	/// </summary>
	public TextWriter BaseWriter => w;
}
