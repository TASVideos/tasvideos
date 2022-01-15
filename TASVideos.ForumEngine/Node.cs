using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using TASVideos.Common;

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
		Task WriteHtml(HtmlWriter w, IWriterHelper h);
	}

	public class Text : INode
	{
		public string Content { get; set; } = "";
		public Task WriteHtml(HtmlWriter w, IWriterHelper h)
		{
			w.Text(Content);
			return Task.CompletedTask;
		}
	}

	public class Element : INode
	{
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
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

		private async Task WriteChildren(HtmlWriter w, IWriterHelper h)
		{
			foreach (var c in Children)
			{
				await c.WriteHtml(w, h);
			}
		}

		private async Task WriteSimpleTag(HtmlWriter w, IWriterHelper h, string t)
		{
			w.OpenTag(t);
			await WriteChildren(w, h);
			w.CloseTag(t);
		}

		private async Task WriteSimpleHtmlTag(HtmlWriter w, IWriterHelper h, string t)
		{
			// t looks like `html:b`
			await WriteSimpleTag(w, h, t[5..]);
		}

		private async Task WriteClassyTag(HtmlWriter w, IWriterHelper h, string tag, string clazz)
		{
			w.OpenTag(tag);
			w.Attribute("class", clazz);
			await WriteChildren(w, h);
			w.CloseTag(tag);
		}

		private void TryParseSize(out int? w, out int? h)
		{
			static int? TryParse(string s)
			{
				return int.TryParse(s, out var i) ? i : null;
			}

			var ss = Options.Split('x');
			w = null;
			h = null;

			if (ss.Length > 2)
			{
				return;
			}

			if (ss.Length > 1)
			{
				h = TryParse(ss[1]);
			}

			if (ss.Length > 0)
			{
				w = TryParse(ss[0]);
			}
		}

		private async Task WriteHref(HtmlWriter w, IWriterHelper h, Func<string, string> transformUrl, Func<string, Task<string>> transformUrlText)
		{
			w.OpenTag("a");
			var href = transformUrl(Options != "" ? Options : GetChildText());
			w.Attribute("href", href);
			if (Options != "")
			{
				await WriteChildren(w, h);
			}
			else
			{
				// these were all parsed as ChildTagsIfParam, so we're guaranteed to have zero or one text children.
				var text = Children.Cast<Text>().SingleOrDefault()?.Content ?? "";
				w.Text(await transformUrlText(text));
			}

			w.CloseTag("a");
		}

		public async Task WriteHtml(HtmlWriter w, IWriterHelper h)
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
					await WriteClassyTag(w, h, "div", "a-l");
					break;
				case "center":
					await WriteClassyTag(w, h, "div", "a-c");
					break;
				case "right":
					await WriteClassyTag(w, h, "div", "a-r");
					break;
				case "spoiler":
					await WriteClassyTag(w, h, "span", "spoiler");
					break;
				case "warning":
					await WriteClassyTag(w, h, "div", "warning");
					break;
				case "note":
					await WriteClassyTag(w, h, "div", "forumline");
					break;
				case "highlight":
					await WriteClassyTag(w, h, "span", "highlight");
					break;
				case "quote":
					w.OpenTag("div");
					w.Attribute("class", "quotecontainer");
					if (Options != "")
					{
						w.OpenTag("cite");
						await BbParser.Parse(Options, false, true).WriteHtml(w, h);
						w.Text(" wrote:");
						w.CloseTag("cite");
					}

					w.OpenTag("blockquote");
					await WriteChildren(w, h);
					w.CloseTag("blockquote");
					w.CloseTag("div");
					break;
				case "code":
					{
						// If Options is "foo" then that's a language tag.
						// If Options is "foo.bar" then "foo.bar" is a downloadable filename and "bar" is a language tag.
						var osplit = Options.Split('.', StringSplitOptions.RemoveEmptyEntries);
						if (osplit.Length == 2)
						{
							w.OpenTag("a");
							w.Attribute("class", "btn bg-info text-dark code-download");
							w.Attribute("href", "data:text/plain," + Uri.EscapeDataString(GetChildText().TrimStart()));
							w.Attribute("download", Options);
							w.Text("Download ");
							w.Text(Options);
							w.CloseTag("a");
						}

						w.OpenTag("pre");

						// "text" is not a supported language for prism,
						// so it will just get the same text formatting as languages, but no syntax highlighting.
						var lang = osplit.Length > 0 ? osplit[^1] : "text";

						if (lang != "text")
						{
							w.OpenTag("div");
							w.Text("Language: ");
							w.OpenTag("cite");
							w.Text(lang);
							w.CloseTag("cite");
							w.CloseTag("div");
							w.VoidTag("hr");
						}

						w.OpenTag("code");
						w.Attribute("class", $"language-{lang}");
						await WriteChildren(w, h);
						w.CloseTag("code");
						w.CloseTag("pre");
					}

					break;
				case "img":
					{
						w.VoidTag("img");
						TryParseSize(out var width, out var height);
						if (width != null)
						{
							w.Attribute("width", width.ToString()!);
						}

						if (height != null)
						{
							w.Attribute("height", height.ToString()!);
						}

						w.Attribute("src", GetChildText());
						w.Attribute("class", "mw-100");
					}

					break;
				case "url":
					await WriteHref(w, h, s => s, async s => s);
					break;
				case "email":
					await WriteHref(w, h, s => "mailto:" + s, async s => s);
					break;
				case "thread":
					await WriteHref(w, h, s => "/Forum/Topics/" + s, async s => "Thread #" + s);
					break;
				case "post":
					await WriteHref(w, h, s => "/Forum/Posts/" + s, async s => "Post #" + s);
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

						var timeable = new Timeable
						{
							FrameRate = fps,
							Frames = n
						};
						var time = timeable.Time().ToStringWithOptionalDaysAndHours();

						w.OpenTag("abbr");
						w.Attribute("title", $"{n} Frames @{fps} FPS");
						w.Text(time);
						w.CloseTag("abbr");
						break;
					}

				case "color":
					w.OpenTag("span");

					// TODO: More fully featured anti-style injection
					w.Attribute("style", "color: " + Options.Split(';')[0]);
					await WriteChildren(w, h);
					w.CloseTag("span");
					break;
				case "bgcolor":
					w.OpenTag("span");

					// TODO: More fully featured anti-style injection
					w.Attribute("style", "background-color: " + Options.Split(';')[0]);
					await WriteChildren(w, h);
					w.CloseTag("span");
					break;
				case "size":
					w.OpenTag("span");

					// TODO: More fully featured anti-style injection
					var sizeStr = Options.Split(';')[0];
					if (double.TryParse(sizeStr, out var sizeDouble))
					{
						// default font size of the old site was 12px, so if size was given without a unit, divide by 12 and use em
						w.Attribute("style", $"font-size: {sizeDouble / 12}em");
					}
					else
					{
						w.Attribute("style", $"font-size: {sizeStr}");
					}

					await WriteChildren(w, h);
					w.CloseTag("span");
					break;
				case "noparse":
					await WriteChildren(w, h);
					break;
				case "google":
					if (Options == "images")
					{
						w.OpenTag("a");
						w.Attribute("href", "//www.google.com/images?q=" + Uri.EscapeDataString(GetChildText()));
						w.Text("Google Images Search: " + GetChildText());
						w.CloseTag("a");
					}
					else
					{
						w.OpenTag("a");
						w.Attribute("href", "//www.google.com/search?q=" + Uri.EscapeDataString(GetChildText()));
						w.Text("Google Search: " + GetChildText());
						w.CloseTag("a");
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
									var key = parsedQuery.Keys[i];
									if (key != null)
									{
										pp.QueryParams[key] = parsedQuery.GetValues(i)![0];
									}
								}
							}

							TryParseSize(out var width, out var height);
							if (width != null && height != null)
							{
								pp.Width = width;
								pp.Height = height;
							}

							WriteVideo.Write(w.BaseWriter, pp);
						}

						w.OpenTag("a");
						w.Attribute("href", href);
						w.Text("Link to video");
						w.CloseTag("a");
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
					w.VoidTag("br");
					break;
				case "html:hr":
					w.VoidTag("hr");
					break;

				default:
					throw new InvalidOperationException("Internal error on tag " + Name);
			}
		}
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
	}
}
