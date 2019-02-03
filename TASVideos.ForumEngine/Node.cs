using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TASVideos.ForumEngine
{
	public interface Node
	{
		void WriteHtml(TextWriter w);
	}

	internal static class Helpers
	{
		public static void WriteText(TextWriter w, string s)
		{
			foreach (var c in s)
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

		public static void WriteAttributeValue(TextWriter w, string s)
		{
			w.Write('"');
			foreach (var c in s)
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
	}

	public class Text : Node
	{
		public string Content { get; set; }
		public void WriteHtml(TextWriter w)
		{
			Helpers.WriteText(w, Content);
		}
	}

	public class Element : Node
	{
		public string Name { get; set; }
		public string Options { get; set; } = "";
		public List<Node> Children { get; set; } = new List<Node>();
		private string GetChildText()
		{
			var sb = new StringBuilder();
			foreach (var c in Children.Cast<Text>())
				sb.Append(c.Content);
			return sb.ToString();
		}

		private void WriteChildren(TextWriter w)
		{
			foreach (var c in Children)
				c.WriteHtml(w);
		}

		private void WriteSimpleTag(TextWriter w, string t)
		{
			w.Write('<');
			w.Write(t);
			w.Write('>');
			WriteChildren(w);
			w.Write("</");
			w.Write(t);
			w.Write('>');
		}

		private void WriteComplexTag(TextWriter w, string open, string close)
		{
			w.Write(open);
			WriteChildren(w);
			w.Write(close);
		}

		private bool TryParseSize(out int w, out int h)
		{
			var ss = Options.Split('x');
			w = 0;
			h = 0;
			if (ss.Length != 2)
			{
				return false;
			}
			var ret = int.TryParse(ss[0], out w) && int.TryParse(ss[1], out h);
			if (!ret)
			{
				w = 0;
				h = 0;
			}
			return ret;
		}

		private void WriteHref(TextWriter w, Func<string, string> transformUrl)
		{
			w.Write("<a href=");
			var href = transformUrl(Options != "" ? Options : GetChildText());
			Helpers.WriteAttributeValue(w, href);
			w.Write('>');
			WriteChildren(w);
			w.Write("</a>");
		}

		public void WriteHtml(TextWriter w)
		{
			switch (Name)
			{
				case "b":
				case "i":
				case "u":
				case "s":
				case "sub":
				case "sup":
				case "tt":
				case "li":
					WriteSimpleTag(w, Name);
					break;
				case "left":
					WriteComplexTag(w, "<div class=a-l>", "</div>");
					break;
				case "center":
					WriteComplexTag(w, "<div class=a-c>", "</div>");
					break;
				case "right":
					WriteComplexTag(w, "<div class=a-r>", "</div>");
					break;
				case "spoiler":
					WriteComplexTag(w, "<span class=spoiler>", "</span>");
					break;
				case "quote":
					w.Write("<div class=quotecontainer>");
					if (Options != "")
					{
						w.Write("<cite>");
						Helpers.WriteText(w, Options);
						w.Write(" wrote:</cite>");
					}
					w.Write("<blockquote>");
					WriteChildren(w);
					w.Write("</blockquote></div>");
					break;
				case "code":
					w.Write("<code");
					if (Options != "")
					{
						w.Write(" class=");
						Helpers.WriteAttributeValue(w, "language-" + Options);
					}
					w.Write("><pre>");
					WriteChildren(w);
					w.Write("</pre></code>");
					break;
				case "img":
					{
						w.Write("<img");
						if (TryParseSize(out var width, out var height))
						{
							w.Write(" width=");
							Helpers.WriteAttributeValue(w, width.ToString());
							w.Write(" height=");
							Helpers.WriteAttributeValue(w, height.ToString());
						}
						w.Write(" src=");
						Helpers.WriteAttributeValue(w, GetChildText());
						w.Write('>');
					}
					break;
				case "url":
					WriteHref(w, s => s);
					break;
				case "email":
					WriteHref(w, s => "mailto:" + s);
					break;
				case "thread":
					WriteHref(w, s => "/forum/t/" + s);
					break;
				case "post":
					WriteHref(w, s => "/forum/p/" + s + "#" + s);
					break;
				case "movie":
					WriteHref(w, s => "/" + s + "M.html");
					break;
				case "submission":
					WriteHref(w, s => "/" + s + "S.html");
					break;
				case "userfile":
					WriteHref(w, s => "/userfiles/info/" + s);
					break;
				case "wiki":
					WriteHref(w, s => "/" + s + ".html");
					break;
				case "frames":
					{
						var ss = GetChildText().Split('@');
						int.TryParse(ss[0], out var n);
						var fps = 60.0;
						if (ss.Length > 1)
							double.TryParse(ss[1], out fps);
						if (fps <= 0)
							fps = 60.0;
						w.Write("<abbr title=");
						Helpers.WriteAttributeValue(w, $"{n} Frames @${fps} FPS");
						w.Write('>');
						w.Write(n / fps);
						w.Write("</abbr>");
						break;
					}
				case "color":
					w.Write("<span style=");
					// TODO: More fully featured anti-style injection
					Helpers.WriteAttributeValue(w, "color: " + Options.Split(';')[0]);
					w.Write('>');
					WriteChildren(w);
					w.Write("</span>");
					break;
				case "size":
					w.Write("<span style=");
					// TODO: More fully featured anti-style injection
					Helpers.WriteAttributeValue(w, "font-size: " + Options.Split(';')[0]);
					w.Write('>');
					WriteChildren(w);
					w.Write("</span>");
					break;
				case "noparse":
					WriteChildren(w);
					break;
				case "google":
					if (Options == "images")
					{
						w.Write("a href=");
						Helpers.WriteAttributeValue(w, "//www.google.com/images?q=" + GetChildText());
						w.Write('>');
						Helpers.WriteText(w, "Google Images Search: " + GetChildText());
						w.Write("</a>");
					}
					else
					{
						w.Write("a href=");
						Helpers.WriteAttributeValue(w, "//www.google.com/search?q=" + GetChildText());
						w.Write('>');
						Helpers.WriteText(w, "Google Search: " + GetChildText());
						w.Write("</a>");
					}
					break;
				case "video":
					Helpers.WriteText(w, "TODO: SUPPORT THE VIDEO TAG");
					break;
				case "_root":
					WriteComplexTag(w, "<div class=postbody>", "</div>");
					break;
				case "list":
					WriteSimpleTag(w, Options == "1" ? "ol" : "ul");
					break;
				// these four are only used by converted HTML posts
				case "div":
				case "p":
				case "span":
					WriteSimpleTag(w, Name);
					break;
				case "br":
					w.Write("<br>");
					break;

				default:
					throw new InvalidOperationException("Internal error on tag " + Name);
			}
		}
	}
}
