using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using TASVideos.WikiEngine.AST;

namespace TASVideos.WikiEngine
{
	public static class Builtins
	{
		private static int UniqueId = 0;
		private static string GetUniqueId()
		{
			var i = Interlocked.Increment(ref UniqueId);
			return "y-" + i;
		}
		private static KeyValuePair<string, string> Attr(string name, string value)
		{
			return new KeyValuePair<string, string>(name, value);
		}
		public static INode MakeTabs(Element tabset)
		{
			var navclass = tabset.Tag == "htabs" ? "nav nav-pills nav-stacked col-md-3" : "nav nav-tabs";
			var tabclass = tabset.Tag == "htabs" ? "tab-content col-md-9" : "tab-content";
			var nav = new List<INode>();
			var content = new List<INode>();
			var first = true;
			foreach (var child in tabset.Children.Cast<Element>())
			{
				var id = GetUniqueId();
				nav.Add(new Element("li", first ? new[] { Attr("class", "active") } : new KeyValuePair<string, string>[0], new[]
				{
					new Element("a", new[] { Attr("href", "#" + id), Attr("data-toggle", "tab") }, new[]
					{
						new Text(child.Attributes["data-name"])
					})
				}));
				content.Add(new Element("div", new[] { Attr("id", id), Attr("class", first ? "tab-pane active" : "tab-pane") }, child.Children));
				first = false;
			}
			return new Element("div", new[] { Attr("class", "row") }, new[]
			{
				new Element("ul", new[] { Attr("class", navclass) }, nav),
				new Element("div", new[] { Attr("class", tabclass) }, content)
			});
		}

		private static readonly Regex Footnote = new Regex(@"^(\d+)$");
		private static readonly Regex FootnoteLink = new Regex(@"^#(\d+)$");
		private static readonly Regex RealModule = new Regex("^module:(.*)$");
		public static IEnumerable<INode> MakeModule(string text)
		{
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
