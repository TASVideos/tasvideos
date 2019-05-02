using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using TASVideos.WikiEngine.AST;

namespace TASVideos.WikiEngine
{
	public static class Builtins
	{
		private static int UniqueId;
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
			// TODO: Fix up CharEnds
			var navClass = tabset.Tag == "htabs" ? "nav nav-pills nav-stacked col-md-3" : "nav nav-tabs";
			var liClass = tabset.Tag == "htabs" ? "" : "nav-item";
			var aClass = tabset.Tag == "htabs" ? "" : "nav-link";
			var tabClass = tabset.Tag == "htabs" ? "tab-content col-md-9" : "tab-content";
			var nav = new List<INode>();
			var content = new List<INode>();
			var first = true;
			foreach (var child in tabset.Children.Cast<Element>())
			{
				var id = GetUniqueId();
				nav.Add(new Element(
					child.CharStart,
					"li",
					new[]
					{
						Attr("class",  liClass)
					},
					new[]
					{
						new Element(
							child.CharStart,
							"a",
							new[] { Attr("href", "#" + id), Attr("data-toggle", "tab"), Attr("class", (first ? "active " : "") + aClass) },
							new[]
							{
								new Text(child.CharStart, child.Attributes["data-name"])
							})
					}));
				content.Add(new Element(child.CharStart, "div", new[] { Attr("id", id), Attr("class", "tab-pane" + (first ? " active" : " fade")) }, child.Children));
				first = false;
			}

			return new Element(tabset.CharStart, "div", new[] { Attr("class", "") }, new[]
			{
				new Element(tabset.CharStart, "ul", new[] { Attr("class", navClass), Attr("role", "tablist") }, nav),
				new Element(tabset.CharStart, "div", new[] { Attr("class", tabClass) }, content)
			});
		}

		private static readonly Regex Footnote = new Regex(@"^(\d+)$");
		private static readonly Regex FootnoteLink = new Regex(@"^#(\d+)$");
		private static readonly Regex RealModule = new Regex("^module:(.*)$");
		public static IEnumerable<INode> MakeModule(int charStart, int charEnd, string text)
		{
			if (text == "|") // literal | escape
				return new[] { new Text(charStart, "|") { CharEnd = charEnd } };
			if (text == ":") // literal : escape (for inside dd/dt)
				return new[] { new Text(charStart, ":") { CharEnd = charEnd } };
			if (text == "expr:UserGetWikiName")
				return MakeModuleInternal(charStart, charEnd, "UserGetWikiName");
			if (text == "expr:WikiGetCurrentEditLink")
				return MakeModuleInternal(charStart, charEnd, "WikiGetCurrentEditLink");

			Match match;
			if ((match = Footnote.Match(text)).Success)
				return MakeFootnote(charStart, charEnd, match.Groups[1].Value);
			if ((match = FootnoteLink.Match(text)).Success)
				return MakeFootnoteLink(charStart, charEnd, match.Groups[1].Value);
			if ((match = RealModule.Match(text)).Success)
				return MakeModuleInternal(charStart, charEnd, match.Groups[1].Value);
			if (text.StartsWith("user:"))
				// user homepages are the same __wikiLink module as other wiki links, but match them here so we can catch special characters in user names
				// pass the `user:` part to the module as well so it can disambiguate between regular and user pages
				return MakeModuleInternal(charStart, charEnd, "__wikiLink|" + text);

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
				new Element(charStart, "a", new[] { Attr("id", n) }, new INode[0]) { CharEnd = charStart },
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
				new Element(charStart, "a", new[] { Attr("id", "r" + n) }, new INode[0]) { CharEnd = charStart },
				new Element(charStart, "sup", new INode[]
				{
					new Text(charStart, "[") { CharEnd = charStart },
					new Element(charStart, "a", new[] { Attr("href", "#" + n) }, new []
					{
						new Text(charStart, n) { CharEnd = charEnd }
					}) { CharEnd = charEnd },
					new Text(charEnd, "]") { CharEnd = charEnd }
				}) { CharEnd = charEnd }
			};
		}

		private static readonly string[] ImageSuffixes = { ".svg", ".png", ".gif", ".jpg", ".jpeg" };
		private static readonly string[] LinkPrefixes = { "=", "http://", "https://", "ftp://", "//", "irc://" };

		// You can always make a wikilink by starting with "[=", and that will accept a wide range of characters
		// This regex is just for things that we'll make implicit wiki links out of; contents of brackets that don't match any other known pattern
		private static readonly Regex ImplicitWikiLink = new Regex(@"^[A-Za-z0-9._/#\- ]+(\|.+)?$");
		private static bool IsLink(string text)
		{
			return LinkPrefixes.Any(text.StartsWith);
		}

		private static bool IsImage(string text)
		{
			return IsLink(text) && ImageSuffixes.Any(text.EndsWith);
		}

		private static string UrlFromLinkText(string text)
		{
			if (text[0] == '=')
			{
				if (text.Length == 1) // Just a single equals, apparently people expect this to link to home
				{
					return "/";
				}

				return "/" + text.Substring(text[1] == '/' ? 2 : 1);
			}

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
				if (pp.Length > 1)
				{
					return new[] { MakeLink(charStart, charEnd, pp[0], new Text(charStart, pp[1]) { CharEnd = charEnd }) };
				}
				else
				{
					// TODO: the existing forum code does this, but that can't possibly be intended??
					// return new[] { MakeLink(pp[0], new Text(pp[0])) };

					return new[] { MakeLink(charStart, charEnd, pp[0], new Text(charStart, UrlFromLinkText(pp[0])) { CharEnd = charEnd }) };
				}
			}

			// at this point, we have text between [] that doesn't look like a module, doesn't look like a link, and doesn't look like
			// any of the other predetermined things we scan for
			// it could be an internal wiki link, but it could also be a lot of other not-allowed garbage
			if (ImplicitWikiLink.Match(text).Success)
			{
				// wiki links needs to be in a module because the href, title, and possibly the text will be adjusted/normalized based
				// on what other wiki pages exist and their content
				return MakeModuleInternal(charStart, charEnd, "__wikiLink|" + text);
			}

			// In other cases, return raw literal text.  This doesn't quite match the old wiki, which could look for formatting in these, but should be good enough
			return new[] { new Text(charStart, "[" + text + "]") { CharEnd = charEnd } };
		}

		private static INode MakeLink(int charStart, int charEnd, string text, INode child)
		{
			var attrs = new List<KeyValuePair<string, string>>
			{
				Attr("href", UrlFromLinkText(text))
			};
			if (text[0] != '=') // external
			{
				attrs.Add(Attr("rel", "nofollow"));
				attrs.Add(Attr("class", "extlink"));
			}

			return new Element(charStart, "a", attrs, new[] { child }) { CharEnd = charEnd };
		}

		private static INode MakeImage(int charStart, int charEnd, string[] pp, int index)
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

			return new Element(charStart, "img", attrs, new INode[0]) { CharEnd = charEnd };
		}
	}
}
