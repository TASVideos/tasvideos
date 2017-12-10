using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TASVideos.WikiEngine.AST;

namespace TASVideos.WikiEngine
{
	using W = IronMeta.Matcher.MatchItem<char, INode>;
	partial class Wiki
	{
		private static string Str(W content)
		{
			return new string(content.Inputs.ToArray());
		}
		private static INode MakeText(string content)
		{
			return new Text(content);
		}
		private static INode MakeText(W content)
		{
			return new Text(Str(content));
		}
		private static INode MakeElt(string tag)
		{
			return new Element(tag);
		}
		private static INode MakeElt(string tag, W children)
		{
			return new Element(tag, children.Results);
		}
		private static KeyValuePair<string, string> Attr(string name, W value)
		{
			return new KeyValuePair<string, string>(name, ((Text)value.Results.Single()).Content);
		}
		private static KeyValuePair<string, string> Attr(string name, string value)
		{
			return new KeyValuePair<string, string>(name, value);
		}
		private static INode MakeElt(string tag, W children, params KeyValuePair<string, string>[] attrs)
		{
			return new Element(tag, attrs, children.Results);
		}
		private static INode MakeIf(W condition, W children)
		{
			return new IfModule(((Text)condition.Results.Single()).Content, children.Results);
		}

		private static readonly Regex Footnote = new Regex(@"^(\d+)$");
		private static readonly Regex FootnoteLink = new Regex(@"^#(\d+)$");
		private static readonly Regex RealModule = new Regex("^module:(.*)$");
		private static IEnumerable<INode> MakeModule(W condition)
		{
			// This could have been done in ironmeta.  Would that be easier or harder?

			var text = ((Text)condition.Results.Single()).Content;
			if (text == "|") // literal | escape
				return new[] { new Text("|") };
			if (text == "expr:UserGetWikiName")
				return MakeModuleInternal("UserGetWikiName");
			if (text == "expr:WikiGetCurrentEditLink")
				return MakeModuleInternal("WikiGetCurrentEditLink");
			if (text == "user:user_name")
				return MakeModuleInternal("user_name");

			Match match;
			if ((match = Footnote.Match(text)).Success)
				return MakeFootnote(match.Groups[1].Value);
			if ((match = FootnoteLink.Match(text)).Success)
				return MakeFootnoteLink(match.Groups[1].Value);
			if ((match = RealModule.Match(text)).Success)
				return MakeModuleInternal(match.Groups[1].Value);

			return MakeLinkOrImage(text);
		}
		private static IEnumerable<INode> MakeModuleInternal(string module)
		{
			return new []
			{
				new Module(module)
			};
		}
		private static IEnumerable<INode> MakeFootnote(string n)
		{
			return new INode[]
			{
				new Text("["),
				new Element("a", new[] { Attr("id", n) }, new INode[0]),
				new Element("a", new[] { Attr("href", "#r" + n) }, new []
				{
					new Text(n)
				}),
				new Text("]")
			};
		}
		private static IEnumerable<INode> MakeFootnoteLink(string n)
		{
			return new[]
			{
				new Element("a", new[] { Attr("id", "r" + n) }, new INode[0]),
				new Element("sup", new INode[]
				{
					new Text("["),
					new Element("a", new[] { Attr("href", "#" + n) }, new []
					{
						new Text(n)
					}),
					new Text("]")
				})
			};
		}

		private static readonly string[] ImageSuffixes = new[] { ".svg", ".png", ".gif", ".jpg", ".jpeg" };
		private static readonly string[] LinkPrefixes = new[] { "=", "http://", "https://", "ftp://", "//" };
		private static bool IsLink(string text)
		{
			return LinkPrefixes.Any(p => text.StartsWith(p));
		}
		private static bool IsImage(string text)
		{
			return IsLink(text) && ImageSuffixes.Any(s => text.EndsWith(s));
		}
		private static string UrlFromLinkText(string text)
		{
			if (text[0] == '=')
				return "/" + text.Substring(1);
			return text;
		}
		private static IEnumerable<INode> MakeLinkOrImage(string text)
		{
			var pp = text.Split('|');
			if (pp.Length >= 2 && IsLink(pp[0]) && IsImage(pp[1]))
			{
				return new[] { MakeLink(pp[0], MakeImage(pp, 1)) };
			}
			if (IsImage(pp[0]))
			{
				return new[] { MakeImage(pp, 0) };
			}
			if (IsLink(pp[0]))
			{
				if (pp.Length > 1)
				{
					return new[] { MakeLink(pp[0], new Text(pp[1])) };
				}
				else
				{
					// TODO: the existing forum code does this, but that can't possibly be intended??
					// return new[] { MakeLink(pp[0], new Text(pp[0])) };

					return new[] { MakeLink(pp[0], new Text(UrlFromLinkText(pp[0]))) };
				}
			}
			// wiki links needs to be in a module because the href, title, and possibly the text will be adjusted/normalized based
			// on what other wiki pages exist and their content
			return MakeModuleInternal("__wikiLink|" + text);
		}

		private static INode MakeLink(string text, INode child)
		{
			var attrs = new List<KeyValuePair<string, string>>();
			attrs.Add(Attr("href", UrlFromLinkText(text)));
			if (text[0] != '=') // external
			{
				attrs.Add(Attr("rel", "nofollow"));
				attrs.Add(Attr("class", "extlink"));
			}
			return new Element("a", attrs, new[] { child });
		}
		private static INode MakeImage(string[] pp, int index)
		{
			var attrs = new List<KeyValuePair<string, string>>();
			var classSet = false;
			attrs.Add(Attr("src", UrlFromLinkText(pp[index++])));
			for (; index < pp.Length; index++)
			{
				var s = pp[index];
				if (s == "left")
				{
					attrs.Add(Attr("class", "embedleft"));
					classSet = true;
				}
				else if (s == "right")
				{
					attrs.Add(Attr("class", "embedright"));
					classSet = true;
				}
				else if (s.StartsWith("title="))
				{
					attrs.Add(Attr("title", s.Substring(6)));
				}
				else if (s.StartsWith("alt="))
				{
					attrs.Add(Attr("alt", s.Substring(4)));
				}
			}
			if (!classSet)
			{
				attrs.Add(Attr("class", "embed"));
			}
			return new Element("img", attrs, new INode[0]);
		}
	}
}
