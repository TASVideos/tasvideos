using TASVideos.WikiEngine.AST;

namespace TASVideos.WikiEngine;

public static partial class Builtins
{
	private static readonly Regex Footnote = new(@"^(\d+)$");
	private static readonly Regex FootnoteLink = new(@"^#(\d+)$");
	private static readonly Regex RealModule = new("^module:(.*)$");

	/// <summary>
	/// Turns text inside [square brackets] into the appropriate thing, usually module or link.  Does not handle [if:].
	/// </summary>
	public static IEnumerable<INode> MakeBracketed(int charStart, int charEnd, string text)
	{
		if (text.StartsWith("if:"))
		{
			throw new InvalidOperationException("Internal parser error!  `if:` should not come to MakeBracketed");
		}

		switch (text)
		{
			// literal | escape
			case "|":
				return new[] { new Text(charStart, "|") { CharEnd = charEnd } };

			// literal : escape (for inside dd/dt)
			case ":":
				return new[] { new Text(charStart, ":") { CharEnd = charEnd } };
			case "expr:UserGetWikiName":
				return MakeModuleInternal(charStart, charEnd, "UserGetWikiName");
		}

		Match match;
		if ((match = Footnote.Match(text)).Success)
		{
			return MakeFootnote(charStart, charEnd, match.Groups[1].Value);
		}

		if ((match = FootnoteLink.Match(text)).Success)
		{
			return MakeFootnoteLink(charStart, charEnd, match.Groups[1].Value);
		}

		if ((match = RealModule.Match(text)).Success)
		{
			return MakeModuleInternal(charStart, charEnd, match.Groups[1].Value);
		}

		return MakeLinkOrImage(charStart, charEnd, text);
	}

	private static IEnumerable<INode> MakeModuleInternal(int charStart, int charEnd, string module)
	{
		return new[]
		{
			new Module(charStart, charEnd, module)
		};
	}

	private static IEnumerable<INode> MakeFootnote(int charStart, int charEnd, string n)
	{
		return new INode[]
		{
				new Text(charStart, "[") { CharEnd = charStart },
				new Element(charStart, "a", new[] { Attr("id", n) }, Array.Empty<INode>()) { CharEnd = charStart },
				new Element(charStart, "a", new[] { Attr("href", "#r" + n) }, new[]
				{
					new Text(charStart, n) { CharEnd = charEnd }
				}) { CharEnd = charEnd },
				new Text(charEnd, "]") { CharEnd = charEnd }
		};
	}

	private static IEnumerable<INode> MakeFootnoteLink(int charStart, int charEnd, string n)
	{
		return new[]
		{
			new Element(charStart, "a", new[] { Attr("id", "r" + n) }, Array.Empty<INode>()) { CharEnd = charStart },
			new Element(charStart, "sup", new INode[]
			{
				new Text(charStart, "[") { CharEnd = charStart },
				new Element(charStart, "a", new[] { Attr("href", "#" + n) }, new[]
				{
					new Text(charStart, n) { CharEnd = charEnd }
				}) { CharEnd = charEnd },
				new Text(charEnd, "]") { CharEnd = charEnd }
			}) { CharEnd = charEnd }
		};
	}

	private static readonly string[] ImageSuffixes = { ".svg", ".png", ".gif", ".jpg", ".jpeg" };
	private static readonly string[] LinkPrefixes = { "=", "http://", "https://", "ftp://", "//", "irc://", "user:", "#" };

	// You can always make a wikilink by starting with "[=", and that will accept a wide range of characters
	// This regex is just for things that we'll make implicit wiki links out of; contents of brackets that don't match any other known pattern
	private static readonly Regex ImplicitWikiLink = new(@"^[A-Za-z0-9._/#\- ]+(\|.+)?$");
	private static bool IsLink(string text)
	{
		return LinkPrefixes.Any(text.StartsWith);
	}

	private static bool IsImage(string text)
	{
		return IsLink(text) && ImageSuffixes.Any(text.EndsWith);
	}

	private static string NormalizeImageUrl(string text)
	{
		if (text[0] == '=')
		{
			return string.Concat("/", text.AsSpan(text[1] == '/' ? 2 : 1));
		}

		return text;
	}

	private static string NormalizeUrl(string text)
	{
		if (text[0] == '=')
		{
			if (text == "=" || text == "=/")
			{
				return "/";
			}

			return NormalizeInternalLink(string.Concat("/", text.AsSpan(text[1] == '/' ? 2 : 1)));
		}

		if (text.StartsWith("user:"))
		{
			return NormalizeInternalLink("/Users/Profile/" + text[5..]);
		}

		return text;
	}

	public static string NormalizeInternalLink(string input)
	{
		var hashParts = input.Split('#');

		var text = hashParts[0].TrimEnd('/');
		var ss = text.Split('/');

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
				s = s.Substring(0, s.Length - 5);
			}

			// Ditto TODO
			if (i == ss.Length - 1 && s.EndsWith(".cgi", StringComparison.OrdinalIgnoreCase))
			{
				s = s.Substring(0, s.Length - 4);
			}

			ss[i] = s;
		}

		var newText = string.Join("/", ss);
		hashParts[0] = newText;
		return string.Join("#", hashParts);
	}

	private static string DisplayTextForUrl(string text)
	{
		// If users don't like this, they should use links with explicit display text
		if (text.StartsWith("user:"))
		{
			text = text[5..];
		}

		text = text.Trim('/').Trim('=').Trim('#');
		return text;
	}

	private static IEnumerable<INode> MakeLinkOrImage(int charStart, int charEnd, string text)
	{
		var pp = text.Split('|');
		if (pp.Length >= 2 && IsLink(pp[0]) && IsImage(pp[1]))
		{
			return new[] { MakeLink(charStart, charEnd, pp[0], MakeImage(charStart, charEnd, pp, 1)) };
		}

		if (IsImage(pp[0]))
		{
			return new[] { MakeImage(charStart, charEnd, pp, 0) };
		}

		if (IsLink(pp[0]))
		{
			return pp.Length > 1
				? new[] { MakeLink(charStart, charEnd, pp[0], new Text(charStart, pp[1]) { CharEnd = charEnd }) }
				: new[] { MakeLink(charStart, charEnd, pp[0], new Text(charStart, DisplayTextForUrl(pp[0])) { CharEnd = charEnd }) };
		}

		// at this point, we have text between [] that doesn't look like a module, doesn't look like a link, and doesn't look like
		// any of the other predetermined things we scan for
		// it could be an internal wiki link, but it could also be a lot of other not-allowed garbage
		if (ImplicitWikiLink.Match(text).Success)
		{
			if (pp.Length >= 2)
			{
				// same as the IsLink(pp[0]) && pp.Length >= 2 case, except add the '=' because it was implicitly resolved to an internal link
				return new[] { MakeLink(charStart, charEnd, NormalizeUrl("=" + pp[0]), new Text(charStart, pp[1]) { CharEnd = charEnd }) };
			}

			// If no labeling text was needed, a module is needed for DB lookups (eg `[4022S]`)
			// DB lookup will be required for links like [4022S], so use __wikiLink
			// TODO:  __wikilink should probably be its own AST type??
			return MakeModuleInternal(charStart, charEnd, "__wikiLink|href=" + NormalizeUrl("=" + pp[0]) + "|displaytext=" + pp[0]);
		}

		// In other cases, return raw literal text.  This doesn't quite match the old wiki, which could look for formatting in these, but should be good enough
		return new[] { new Text(charStart, "[" + text + "]") { CharEnd = charEnd } };
	}

	internal static INode MakeLink(int charStart, int charEnd, string text, INode child)
	{
		var href = NormalizeUrl(text);
		var attrs = new List<KeyValuePair<string, string>>
			{
				Attr("href", href)
			};
		if (href[0] == '#' || href[0] == '/' && (href.Length == 1 || href[1] != '/'))
		{
			// internal
			attrs.Add(Attr("class", "intlink"));
		}
		else
		{
			attrs.Add(Attr("rel", "nofollow"));
			attrs.Add(Attr("class", "extlink"));
		}

		return new Element(charStart, "a", attrs, new[] { child }) { CharEnd = charEnd };
	}

	private static INode MakeImage(int charStart, int charEnd, string[] pp, int index)
	{
		var attrs = new List<KeyValuePair<string, string>>
		{
			Attr("src", NormalizeImageUrl(pp[index++]))
		};
		StringBuilder classString = new("embed");
		for (; index < pp.Length; index++)
		{
			var s = pp[index];
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
		}

		classString.Append(" mw-100");

		attrs.Add(Attr("class", classString.ToString()));

		return new Element(charStart, "img", attrs, Array.Empty<INode>()) { CharEnd = charEnd };
	}
}
