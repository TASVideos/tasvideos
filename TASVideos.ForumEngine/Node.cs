﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;

namespace TASVideos.ForumEngine
{
	public interface INode
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

	public class Text : INode
	{
		public string Content { get; set; } = "";
		public void WriteHtml(TextWriter w)
		{
			Helpers.WriteText(w, Content);
		}
	}

	public class Element : INode
	{
		public string Name { get; set; } = "";
		public string Options { get; set; } = "";
		public List<INode> Children { get; set; } = new ();
		private string GetChildText()
		{
			var sb = new StringBuilder();
			foreach (var c in Children.Cast<Text>())
			{
				sb.Append(c.Content);
			}

			return sb.ToString();
		}

		private void WriteChildren(TextWriter w)
		{
			foreach (var c in Children)
			{
				c.WriteHtml(w);
			}
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

		private void WriteSimpleHtmlTag(TextWriter w, string t)
		{
			// t looks like `html:b`
			WriteSimpleTag(w, t[5..]);
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

		private void WriteHref(TextWriter w, Func<string, string> transformUrl, Func<string, string> transformUrlText)
		{
			w.Write("<a href=");
			var href = transformUrl(Options != "" ? Options : GetChildText());
			Helpers.WriteAttributeValue(w, href);
			w.Write('>');
			if (Options != "")
			{
				WriteChildren(w);
			}
			else
			{
				// these were all parsed as ChildTagsIfParam, so we're guaranteed to have a single text child
				var text = Children.Cast<Text>().Single();
				Helpers.WriteText(w, transformUrlText(text.Content));
			}

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
				case "table":
				case "tr":
				case "td":
					WriteSimpleTag(w, Name);
					break;
				case "*":
					WriteSimpleTag(w, "li");
					break;
				case "html:b":
				case "html:i":
				case "html:em":
				case "html:u":
				case "html:pre":
				case "html:code":
				case "html:tt":
				case "html:strike":
				case "html:s":
				case "html:del":
				case "html:sup":
				case "html:sub":
				case "html:div":
				case "html:small":
					WriteSimpleHtmlTag(w, Name);
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
				case "warning":
					WriteComplexTag(w, "<div class=warning>", "</div>");
					break;
				case "note":
					WriteComplexTag(w, "<div class=forumline>", "</div>");
					break;
				case "highlight":
					WriteComplexTag(w, "<span class=highlight>", "</span>");
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
					{
						// If Options is "foo" then that's a language tag.
						// If Options is "foo.bar" then "foo.bar" is a downloadable filename and "bar" is a language tag.
						var osplit = Options.Split('.', StringSplitOptions.RemoveEmptyEntries);
						if (osplit.Length == 2)
						{
							w.Write("<a class='btn btn-info code-download' href=");
							Helpers.WriteAttributeValue(w, "data:text/plain," + Uri.EscapeDataString(GetChildText().TrimStart()));
							w.Write(" download=");
							Helpers.WriteAttributeValue(w, Options);
							w.Write(">Download ");
							Helpers.WriteText(w, Options);
							w.Write("</a>");
						}

						w.Write("<pre><code");
						if (osplit.Length > 0)
						{
							w.Write(" class=");
							Helpers.WriteAttributeValue(w, $"language-{osplit[osplit.Length - 1]}");
						}

						w.Write('>');
						WriteChildren(w);
						w.Write("</code></pre>");
					}

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
					WriteHref(w, s => s, s => s);
					break;
				case "email":
					WriteHref(w, s => "mailto:" + s, s => s);
					break;
				case "thread":
					WriteHref(w, s => "/forum/t/" + s, s => "Thread #" + s);
					break;
				case "post":
					WriteHref(w, s => "/forum/p/" + s + "#" + s, s => "Post #" + s);
					break;
				case "movie":
					// TODO: On the old site, this actually shows the movie title
					// `[123] NES Guerrilla War (USA) in 14:39.18 by lithven`
					WriteHref(w, s => "/" + s + "M", s => "Movie #" + s);
					break;
				case "submission":
					// TODO: On the old site, this actually shows the submission title
					// `#123: kopernical's NES Mega Man 5 in 36:29.94`
					WriteHref(w, s => "/" + s + "S", s => "Submission #" + s);
					break;
				case "userfile":
					WriteHref(w, s => "/userfiles/info/" + s, s => "User movie #" + s);
					break;
				case "wip":
					WriteHref(w, s => "/userfiles/info/" + s, s => "WIP #" + s);
					break;
				case "wiki":
					WriteHref(w, s => "/" + s, s => "Wiki: " + s);
					break;
				case "frames":
					{
						var ss = GetChildText().Split('@');
						int.TryParse(ss[0], out var n);
						var fps = 60.0;
						if (ss.Length > 1)
						{
							double.TryParse(ss[1], out fps);
						}

						if (fps <= 0)
						{
							fps = 60.0;
						}

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
				case "bgcolor":
					w.Write("<span style=");

					// TODO: More fully featured anti-style injection
					Helpers.WriteAttributeValue(w, "background-color: " + Options.Split(';')[0]);
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
					{
						var href = GetChildText();
						if (Uri.IsWellFormedUriString(href, UriKind.Absolute))
						{
							var uri = new Uri(href, UriKind.Absolute);
							var qq = uri.PathAndQuery.Split('?');
							var pp = new VideoParameters(uri.Host, qq[0]);
							if (qq.Length > 1)
							{
								var parsedQuery = HttpUtility.ParseQueryString(qq[1]);

								for (var i = 0; i < parsedQuery.Count; i++)
								{
									pp.QueryParams[parsedQuery.Keys[i]!] = parsedQuery.GetValues(i)![0];
								}
							}

							if (TryParseSize(out var width, out var height))
							{
								pp.Width = width;
								pp.Height = height;
							}

							WriteVideo.Write(w, pp);
						}

						w.Write("<a href=");
						Helpers.WriteAttributeValue(w, href);
						w.Write(">Link to video</a>");
						break;
					}

				case "_root":
					WriteComplexTag(w, "<div class=postbody>", "</div>");
					break;
				case "list":
					WriteSimpleTag(w, Options == "1" ? "ol" : "ul");
					break;
				case "html:br":
					w.Write("<br>");
					break;
				case "html:hr":
					w.Write("<hr>");
					break;

				default:
					throw new InvalidOperationException("Internal error on tag " + Name);
			}
		}
	}
}
