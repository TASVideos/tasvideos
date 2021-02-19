using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace TASVideos.ForumEngine
{
	/// <summary>
	/// Provides helpers that the forum engine needs to render markup
	/// </summary>
	public interface IWriterHelper
	{
		/// <summary>
		/// Get the title of a movie.
		/// </summary>
		/// <returns>`null` if not found</returns>
		Task<string?> GetMovieTitle(int id);

		/// <summary>
		/// Get the title of a submission.
		/// </summary>
		/// <returns>`null` if not found</returns>
		Task<string?> GetSubmissionTitle(int id);
	}

	public class NullWriterHelper : IWriterHelper
	{
		public Task<string?> GetMovieTitle(int id) => Task.FromResult<string?>(null);
		public Task<string?> GetSubmissionTitle(int id) => Task.FromResult<string?>(null);

		private NullWriterHelper()
		{
		}

		public static readonly NullWriterHelper Instance = new ();
	}

	public interface INode
	{
		Task WriteHtml(TextWriter w, IWriterHelper h);
	}

	public class Text : INode
	{
		public string Content { get; set; } = "";
		public Task WriteHtml(TextWriter w, IWriterHelper h)
		{
			Helpers.WriteText(w, Content);
			return Task.CompletedTask;
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

		private async Task WriteChildren(TextWriter w, IWriterHelper h)
		{
			foreach (var c in Children)
			{
				await c.WriteHtml(w, h);
			}
		}

		private async Task WriteSimpleTag(TextWriter w, IWriterHelper h, string t)
		{
			w.Write('<');
			w.Write(t);
			w.Write('>');
			await WriteChildren(w, h);
			w.Write("</");
			w.Write(t);
			w.Write('>');
		}

		private async Task WriteSimpleHtmlTag(TextWriter w, IWriterHelper h, string t)
		{
			// t looks like `html:b`
			await WriteSimpleTag(w, h, t[5..]);
		}

		private async Task WriteComplexTag(TextWriter w, IWriterHelper h, string open, string close)
		{
			w.Write(open);
			await WriteChildren(w, h);
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

		private async Task WriteHref(TextWriter w, IWriterHelper h, Func<string, string> transformUrl, Func<string, Task<string>> transformUrlText)
		{
			w.Write("<a href=");
			var href = transformUrl(Options != "" ? Options : GetChildText());
			Helpers.WriteAttributeValue(w, href);
			w.Write('>');
			if (Options != "")
			{
				await WriteChildren(w, h);
			}
			else
			{
				// these were all parsed as ChildTagsIfParam, so we're guaranteed to have a single text child
				var text = Children.Cast<Text>().Single();
				Helpers.WriteText(w, await transformUrlText(text.Content));
			}

			w.Write("</a>");
		}

		public async Task WriteHtml(TextWriter w, IWriterHelper h)
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
					await WriteSimpleTag(w, h, Name);
					break;
				case "*":
					await WriteSimpleTag(w, h, "li");
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
					await WriteSimpleHtmlTag(w, h, Name);
					break;
				case "left":
					await WriteComplexTag(w, h, "<div class=a-l>", "</div>");
					break;
				case "center":
					await WriteComplexTag(w, h, "<div class=a-c>", "</div>");
					break;
				case "right":
					await WriteComplexTag(w, h, "<div class=a-r>", "</div>");
					break;
				case "spoiler":
					await WriteComplexTag(w, h, "<span class=spoiler>", "</span>");
					break;
				case "warning":
					await WriteComplexTag(w, h, "<div class=warning>", "</div>");
					break;
				case "note":
					await WriteComplexTag(w, h, "<div class=forumline>", "</div>");
					break;
				case "highlight":
					await WriteComplexTag(w, h, "<span class=highlight>", "</span>");
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
					await WriteChildren(w, h);
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
						// "text" is not a supported language for prism,
						// so it will just get the same text formatting as languages, but no syntax highlighting.
						var lang = osplit.Length > 0 ? osplit[osplit.Length - 1] : "text";
						w.Write(" class=");
						Helpers.WriteAttributeValue(w, $"language-{lang}");

						w.Write('>');
						await WriteChildren(w, h);
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
					await WriteHref(w, h, s => s, async s => s);
					break;
				case "email":
					await WriteHref(w, h, s => "mailto:" + s, async s => s);
					break;
				case "thread":
					await WriteHref(w, h, s => "/forum/t/" + s, async s => "Thread #" + s);
					break;
				case "post":
					await WriteHref(w, h, s => "/forum/p/" + s + "#" + s, async s => "Post #" + s);
					break;
				case "movie":
					await WriteHref(
						w,
						h,
						s => "/" + s + "M",
						async s => (int.TryParse(s, out var id) ? await h.GetMovieTitle(id) : null) ?? "Movie #" + s);
					break;
				case "submission":
					await WriteHref(
						w,
						h,
						s => "/" + s + "S",
						async s => (int.TryParse(s, out var id) ? await h.GetSubmissionTitle(id) : null) ?? "Submission #" + s);
					break;
				case "userfile":
					await WriteHref(w, h, s => "/userfiles/info/" + s, async s => "User movie #" + s);
					break;
				case "wip":
					await WriteHref(w, h, s => "/userfiles/info/" + s, async s => "WIP #" + s);
					break;
				case "wiki":
					await WriteHref(w, h, s => "/" + s, async s => "Wiki: " + s);
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
					await WriteChildren(w, h);
					w.Write("</span>");
					break;
				case "bgcolor":
					w.Write("<span style=");

					// TODO: More fully featured anti-style injection
					Helpers.WriteAttributeValue(w, "background-color: " + Options.Split(';')[0]);
					w.Write('>');
					await WriteChildren(w, h);
					w.Write("</span>");
					break;
				case "size":
					w.Write("<span style=");

					// TODO: More fully featured anti-style injection
					Helpers.WriteAttributeValue(w, "font-size: " + Options.Split(';')[0]);
					w.Write('>');
					await WriteChildren(w, h);
					w.Write("</span>");
					break;
				case "noparse":
					await WriteChildren(w, h);
					break;
				case "google":
					if (Options == "images")
					{
						w.Write("<a href=");
						Helpers.WriteAttributeValue(w, "//www.google.com/images?q=" + Uri.EscapeDataString(GetChildText()));
						w.Write('>');
						Helpers.WriteText(w, "Google Images Search: " + GetChildText());
						w.Write("</a>");
					}
					else
					{
						w.Write("<a href=");
						Helpers.WriteAttributeValue(w, "//www.google.com/search?q=" + Uri.EscapeDataString(GetChildText()));
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
					// We want to do <div class=postbody> but that part is handled externally now.
					await WriteChildren(w, h);
					break;
				case "list":
					await WriteSimpleTag(w, h, Options == "1" ? "ol" : "ul");
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
