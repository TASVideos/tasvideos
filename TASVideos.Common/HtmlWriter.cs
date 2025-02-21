using System.Text.RegularExpressions;

namespace TASVideos.Common;

public partial class HtmlWriter(TextWriter w)
{
	private struct HtmlClassList()
	{
		private List<string>? _list = null;

		private string? _single = null;

		public void Add(string item)
		{
			if (_single is not null)
			{
				_list = new() { _single, item };
				_single = null;
			}
			else if (_list is not null)
			{
				_list.Add(item);
			}
			else
			{
				_single = item;
			}
		}

		public readonly string Serialize()
		{
			HashSet<string> classes;
			if (!string.IsNullOrEmpty(_single))
			{
				classes = _single.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToHashSet();
			}
			else if (_list is not null)
			{
				classes = _list.SelectMany(s => s.Split(' ', StringSplitOptions.RemoveEmptyEntries)).ToHashSet();
			}
			else
			{
				return "";
			}

			return classes.Count is 0 ? "" : string.Join(' ', classes);
		}
	}

	private static readonly Regex AllowedTagNames = AllowedTagNamesRegex();
	private static readonly Regex AllowedAttributeNames = AllowedAttributeNamesRegex();
	private static readonly HashSet<string> VoidTags = new(
		[
			"area",
			"base",
			"br",
			"col",
			"embed",
			"hr",
			"img",
			"input",
			"keygen",
			"link",
			"meta",
			"param",
			"source",
			"track",
			"wbr"
		],
		StringComparer.OrdinalIgnoreCase
	);

	private HtmlClassList _currentElemClassAttr = default;

	private HtmlClassList _currentElemRelAttr = default;

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

		FinalizeOpenTag();
		w.Write('<');
		w.Write(tagName);
		_openTags.Push(tagName);
		_inTagOpen = true;
	}

	public void CloseTag(string tagName)
	{
		if (VoidTags.Contains(tagName))
		{
			throw new InvalidOperationException($"{tagName} cannot have children and is self-closing");
		}

		if (_openTags.Count == 0)
		{
			throw new InvalidOperationException("No open tags!");
		}

		tagName = tagName.ToLowerInvariant();
		if (!tagName.Equals(_openTags.Peek(), StringComparison.Ordinal))
		{
			throw new InvalidOperationException($"Opened tag {_openTags.Peek()} but closing tag {tagName}");
		}

		FinalizeOpenTag();
		w.Write("</");
		w.Write(tagName);
		w.Write('>');
		_openTags.Pop();
	}

	/// <summary>equivalent of <see cref="OpenTag"/> for tags like <c>&lt;img></c> which may not have children and are self-closing</summary>
	/// <remarks>where does the name "void" come from? it's the title of the relevant MDN article, but nowhere in the spec --yoshi</remarks>
	/// <seealso cref="OpenTag"/>
	/// <seealso cref="CloseTag"/>
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

		FinalizeOpenTag();
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

		if (name is "class")
		{
			_currentElemClassAttr.Add(value);
		}
		else if (name is "rel")
		{
			_currentElemRelAttr.Add(value);
		}
		else
		{
			EscapeAndWriteAttribute(name, value);
		}
	}

	private void EscapeAndWriteAttribute(ReadOnlySpan<char> name, ReadOnlySpan<char> value)
	{
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
					w.Write(c); // different overload
					break;
			}
		}

		w.Write('"');
	}

	public void Text(string text)
	{
		var inForeignContent = InForeignContent;
		if (inForeignContent && text.Contains($"</{_openTags.Peek()}", StringComparison.OrdinalIgnoreCase))
		{
			throw new InvalidOperationException("Can't unescape something that looks like closing tag here!");
		}

		FinalizeOpenTag();
		if (inForeignContent)
		{
			w.Write(text);
			return;
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
					w.Write(c); // different overload
					break;
			}
		}
	}

	public void AssertFinished()
	{
		if (_openTags.Count > 0)
		{
			throw new InvalidOperationException($"Tags still open! {string.Join(" > ", _openTags)}");
		}

		FinalizeOpenTag();
	}

	private void FinalizeOpenTag()
	{
		if (!_inTagOpen)
		{
			return;
		}

		_inTagOpen = false;

		var classListStr = _currentElemClassAttr.Serialize();
		_currentElemClassAttr = default;
		if (classListStr.Length is not 0)
		{
			EscapeAndWriteAttribute("class", classListStr);
		}

		var relationListStr = _currentElemRelAttr.Serialize();
		_currentElemRelAttr = default;
		if (relationListStr.Length is not 0)
		{
			EscapeAndWriteAttribute("rel", relationListStr);
		}

		w.Write('>');
	}

	/// <summary>
	/// Gets the underlying writer
	/// Do not use this unless you're very careful with escaping!
	/// </summary>
	public TextWriter BaseWriter => w;

	[GeneratedRegex("^[a-z0-9]+$")]
	private static partial Regex AllowedTagNamesRegex();
	[GeneratedRegex("^[a-z\\-]+$")]
	private static partial Regex AllowedAttributeNamesRegex();
}
