using System.Text.RegularExpressions;

namespace TASVideos.Common;

public class HtmlWriter
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

	private readonly TextWriter _w;
	private bool _inTagOpen;
	private readonly Stack<string> _openTags = new();

	private bool InForeignContent => _openTags.TryPeek(out var tag) && tag is "script" or "style";

	public HtmlWriter(TextWriter w)
	{
		_w = w;
	}

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
			_w.Write('>');
		}

		tagName = tagName.ToLowerInvariant();
		_w.Write('<');
		_w.Write(tagName);
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
			_w.Write('>');
		}

		tagName = tagName.ToLowerInvariant();
		_w.Write("</");
		_w.Write(tagName);
		_w.Write('>');
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
			_w.Write('>');
		}

		tagName = tagName.ToLowerInvariant();
		_w.Write('<');
		_w.Write(tagName);
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

		_w.Write(' ');
		_w.Write(name);
		_w.Write("=\"");

		foreach (var c in value)
		{
			switch (c)
			{
				case '<':
					_w.Write("&lt;");
					break;
				case '&':
					_w.Write("&amp;");
					break;
				case '"':
					_w.Write("&quot;");
					break;
				default:
					_w.Write(c);
					break;
			}
		}

		_w.Write('"');
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
				_w.Write('>');
			}

			_w.Write(text);
		}
		else
		{
			if (_inTagOpen)
			{
				_w.Write('>');
			}

			foreach (var c in text)
			{
				switch (c)
				{
					case '<':
						_w.Write("&lt;");
						break;
					case '&':
						_w.Write("&amp;");
						break;
					default:
						_w.Write(c);
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
			_w.Write('>');
		}
	}

	/// <summary>
	/// Gets the underlying writer
	/// Do not use this unless you're very careful with escaping!
	/// </summary>
	public TextWriter BaseWriter => _w;
}
