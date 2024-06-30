using TASVideos.Common;
using TASVideos.Extensions;
using TASVideos.WikiEngine.AST;

namespace TASVideos.WikiEngine;

public static partial class Builtins
{
	private static readonly Regex Footnote = FootnoteRegex();
	private static readonly Regex FootnoteLink = FootnoteLinkRegex();
	private static readonly Regex RealModule = RealModuleRegex();

	/// <summary>
	/// Turns text inside [square brackets] into the appropriate thing, usually module or link.  Does not handle [if:].
	/// </summary>
	public static IEnumerable<INode> MakeBracketed(ReadOnlySpan<char> text, StringIndices range)
	{
		if (text.StartsWith("if:"))
		{
			throw new InvalidOperationException("Internal parser error!  `if:` should not come to MakeBracketed");
		}

		switch (text)
		{
			// literal | escape
			case "|":
				return [new Text(range, "|")];

			// literal : escape (for inside dd/dt)
			case ":":
				return [new Text(range, ":")];
			case "expr:UserGetWikiName":
				return MakeModuleInternal(range, "UserGetWikiName");
		}

		RegexMatchShim match;
		if ((match = Footnote.Match(text)).Success)
		{
			return MakeFootnote(range, match.Groups[1].Value);
		}

		if ((match = FootnoteLink.Match(text)).Success)
		{
			return MakeFootnoteLink(range, match.Groups[1].Value);
		}

		if ((match = RealModule.Match(text)).Success)
		{
			return MakeModuleInternal(range, match.Groups[1].Value);
		}

		return MakeLinkOrImage(range, text);
	}

	private static Module[] MakeModuleInternal(StringIndices range, ReadOnlySpan<char> module)
	{
		return [new Module(range, module.ToString())];
	}

	private static IEnumerable<INode> MakeFootnote(StringIndices range, ReadOnlySpan<char> n)
	{
		return
		[
			new Text(range, "["),
			new Element(range, "a", attributes: [Attr("id", n)]),
			new Element(range, "a", attributes: [Attr("href", $"#r{n}")], new Text(range, n)),
			new Text(range, "]"),
		];
	}

	private static Element[] MakeFootnoteLink(StringIndices range, ReadOnlySpan<char> n)
	{
		return
		[
			new Element(range, "a", attributes: [Attr("id", $"r{n}")]),
			new Element(
				range,
				"sup",
				attributes: [],
				new Text(range, "["),
				new Element(range, "a", attributes: [Attr("href", $"#{n}")], new Text(range, n)),
				new Text(range, "]")),
		];
	}

	private static readonly string[] ImageSuffixes = [".svg", ".png", ".gif", ".jpg", ".jpeg", ".webp"];
	private static readonly string[] LinkPrefixes = ["=", "http://", "https://", "ftp://", "//", "irc://", "user:", "#"];

	// You can always make a wikilink by starting with "[=", and that will accept a wide range of characters
	// This regex is just for things that we'll make implicit wiki links out of; contents of brackets that don't match any other known pattern
	private static readonly Regex ImplicitWikiLink = ImplicitWikiLinkRegex();
	private static bool IsLink(ReadOnlySpan<char> text)
	{
		return text.StartsWithAny(LinkPrefixes);
	}

	private static bool IsImage(ReadOnlySpan<char> text)
	{
		return IsLink(text) && text.EndsWithAny(ImageSuffixes);
	}

	private static ReadOnlySpan<char> NormalizeImageUrl(ReadOnlySpan<char> text)
	{
		return text[0] == '='
			? text[1] is '/' ? text[1..] : $"/{text[1..]}"
			: text;
	}

	private static ReadOnlySpan<char> NormalizeUrl(ReadOnlySpan<char> text)
	{
		if (text[0] == '=')
		{
			if (text is "=" or "=/")
			{
				return "/";
			}

			return NormalizeInternalLink(text[1] is '/' ? text[1..] : $"/{text[1..]}");
		}

		if (text.StartsWith("user:"))
		{
			return NormalizeInternalLink($"/Users/Profile/{text[5..]}");
		}

		return text;
	}

	public static string NormalizeInternalLink(ReadOnlySpan<char> input)
	{
		var iAnchorSeparator = input.IndexOf('#');
		var pathAndQuery = iAnchorSeparator < 0 ? input : input[..iAnchorSeparator];
		var anchor = iAnchorSeparator < 0 ? "" : input[(iAnchorSeparator + 1)..];
		var ss = pathAndQuery.TrimEnd('/').ToString().Split('/');

		int skip = -1;
		if (ss.Length >= 4 && ss[1].Equals("users", StringComparison.OrdinalIgnoreCase) && ss[2].Equals("profile", StringComparison.OrdinalIgnoreCase))
		{
			skip = 3; // "/Users/Profile/{user}"
		}
		else if (ss.Length >= 3 && ss[1].Equals("homepages", StringComparison.OrdinalIgnoreCase))
		{
			skip = 2; // "/HomePages/{user}"
		}

		for (var i = 0; i < ss.Length; i++)
		{
			var s = ss[i];
			if (i != skip)
			{
				s = s.Replace(" ", "");
				if (s.Length > 0)
				{
					s = char.ToUpperInvariant(s[0]) + s[1..];
				}
			}

			// TODO: What should be done if a username actually ends in .html?
			if (i == ss.Length - 1 && s.EndsWith(".html", StringComparison.OrdinalIgnoreCase))
			{
				s = s[..^5];
			}

			// Ditto TODO
			if (i == ss.Length - 1 && s.EndsWith(".cgi", StringComparison.OrdinalIgnoreCase))
			{
				s = s[..^4];
			}

			ss[i] = s;
		}

		var newText = string.Join('/', ss);
		return anchor.Length is 0 ? newText : $"{newText}#{anchor}";
	}

	private static ReadOnlySpan<char> DisplayTextForUrl(ReadOnlySpan<char> text)
	{
		// If users don't like this, they should use links with explicit display text
		if (text.StartsWith("user:"))
		{
			text = text[5..];
		}

		text = text.Trim('/').Trim('=').Trim('#');
		return text;
	}

	private static IEnumerable<INode> MakeLinkOrImage(StringIndices range, ReadOnlySpan<char> text)
	{
		var pp = text.ToString().Split('|');
		if (pp.Length >= 2 && IsLink(pp[0]) && IsImage(pp[1]))
		{
			return [MakeLink(range, pp[0], MakeImage(range, pp.Skip(1), out _))];
		}

		if (IsImage(pp[0]))
		{
			var node = MakeImage(range, pp, out var unusedParams);
			if (!unusedParams)
			{
				return [node];
			}
		}

		if (IsLink(pp[0]))
		{
			var node = MakeLink(
				range,
				pp[0],
				new Text(range, pp.Length > 1 ? pp[1] : DisplayTextForUrl(pp[0])));
			return [node];
		}

		// at this point, we have text between [] that doesn't look like a module, doesn't look like a link, and doesn't look like
		// any of the other predetermined things we scan for
		// it could be an internal wiki link, but it could also be a lot of other not-allowed garbage
		if (ImplicitWikiLink.IsMatch(text))
		{
			if (pp.Length >= 2)
			{
				// These need DB lookup for title attributes in some cases
				return MakeModuleInternal(range, $"__wikiLink|href={NormalizeUrl("=" + pp[0])}|displaytext={pp[1]}");
			}

			// If no labeling text was needed, a module is needed for DB lookups (eg `[4022S]`)
			// DB lookup will be required for links like [4022S], so use __wikiLink
			// TODO:  __wikilink should probably be its own AST type??
			return MakeModuleInternal(range, $"__wikiLink|href={NormalizeUrl("=" + pp[0])}|implicitdisplaytext={pp[0]}");
		}

		// In other cases, return raw literal text.  This doesn't quite match the old wiki, which could look for formatting in these, but should be good enough
		return [new Text(range, $"[{text}]")];
	}

	internal static INode MakeLink(StringIndices range, ReadOnlySpan<char> text, INode child)
	{
		var href = NormalizeUrl(text);
		var attrs = new List<KeyValuePair<string, string>>
		{
			Attr("href", href)
		};
		if (!UriString.IsToExternalDomain(href))
		{
			// internal
			attrs.Add(Attr("class", "intlink"));
		}
		else
		{
			attrs.Add(Attr("rel", "noopener external nofollow"));
		}

		return new Element(range, "a", attributes: attrs, child);
	}

	private static Element MakeImage(StringIndices range, IEnumerable<string> pp, out bool unusedParams)
	{
		unusedParams = false;
		var iter = pp.GetEnumerator();
		_ = iter.MoveNext();
		var attrs = new List<KeyValuePair<string, string>>
		{
			Attr("src", NormalizeImageUrl(iter.Current)),
		};
		StringBuilder classString = new("embed");
		while (iter.MoveNext())
		{
			var s = iter.Current;
			if (s == "left")
			{
				classString.Append("left");
			}
			else if (s == "right")
			{
				classString.Append("right");
			}
			else if (s.StartsWith("title="))
			{
				attrs.Add(Attr("title", s[6..]));
			}
			else if (s.StartsWith("alt="))
			{
				attrs.Add(Attr("alt", s[4..]));
			}
			else if (s.StartsWith("h="))
			{
				attrs.Add(Attr("height", s[2..]));
			}
			else if (s.StartsWith("w="))
			{
				attrs.Add(Attr("width", s[2..]));
			}
			else
			{
				unusedParams = true;
			}
		}

		classString.Append(" mw-100");

		attrs.Add(Attr("class", classString.ToString()));

		return new(range, "img", attributes: attrs);
	}

	[GeneratedRegex(@"^(\d+)$")]
	private static partial Regex FootnoteRegex();

	[GeneratedRegex(@"^#(\d+)$")]
	private static partial Regex FootnoteLinkRegex();

	[GeneratedRegex("^module:(.*)$")]
	private static partial Regex RealModuleRegex();

	[GeneratedRegex(@"^[A-Za-z0-9._/#\- ]+(\|.+)?$")]
	private static partial Regex ImplicitWikiLinkRegex();
}
