using System;
using System.IO;
using System.Text;

namespace TASVideos.ForumEngine;

public static class WriteVideo
{
	private static void DoTemplate(TextWriter w, string template, int width, int height, string id)
	{
		foreach (var ss in template.Split(new[] { "$$" }, StringSplitOptions.None))
		{
			switch (ss)
			{
				case "w":
					w.Write(width);
					break;
				case "h":
					w.Write(height);
					break;
				case "id":
					foreach (var c in id)
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

					break;
				default:
					w.Write(ss);
					break;
			}
		}
	}

	private static readonly string YouTube =
@"<iframe width=$$w$$ height=$$h$$
src=""https://www.youtube.com/embed/$$id$$""
frameborder=0
allow=""accelerometer; autoplay; encrypted-media; gyroscope; picture-in-picture""
allowfullscreen
class=""mw-100""></iframe>
";
	private static readonly string YouTubePlaylist =
@"<iframe width=$$w$$ height=$$h$$
src=""https://www.youtube.com/embed/videoseries?list=$$id$$""
frameborder=0
allow=""accelerometer; autoplay; encrypted-media; gyroscope; picture-in-picture""
allowfullscreen
class=""mw-100""></iframe>
";
	private static readonly string DailyMotion =
@"<iframe
frameborder=0 width=$$w$$ height=$$h$$
src=""https://www.dailymotion.com/embed/video/$$id$$"" allowfullscreen allow=autoplay class=""mw-100""></iframe>
";
	private static readonly string Vimeo =
@"<iframe src=""https://player.vimeo.com/video/$$id$$""
width=$$w$$ height=$$h$$ frameborder=0
allow=""autoplay; fullscreen"" allowfullscreen class=""mw-100""></iframe>
";

	private static readonly string NicoVideoDocument =
@"<!DOCTYPE html>
<html><head><title>NicoVideo Player</title><style>
html { overflow:hidden; }
body, div { margin:0; padding:0; overflow:hidden; }
</style></head><body>
<div>
<script src=""https://embed.nicovideo.jp/watch/$$id$$/script?w=$$w$$&h=$$h$$""></script>
</div>
</body></html>
";
	private static readonly string NicoVideo =
@"<iframe src=""data:text/html;base64,$$id$$""
width=$$w$$ height=$$h$$ frameborder=0 class=""mw-100""></iframe>
";

	private static readonly string ArchiveOrg =
@"<iframe src=""https://archive.org/embed/$$id$$""
width=$$w$$ height=$$h$$ frameborder=0 webkitallowfullscreen=true mozallowfullscreen=true allowfullscreen class=""mw-100""></iframe>
";

	public static void Write(TextWriter w, VideoParameters pp)
	{
		int width = pp.Width ?? 480;
		int height = pp.Height ?? 270;
		switch (pp.Host)
		{
			case "youtube.com":
			case "www.youtube.com":
				if (pp.Path == "/watch" && pp.QueryParams.ContainsKey("v")) // https://www.youtube.com/watch?v=yLORZbc-PZw
				{
					DoTemplate(w, YouTube, width, height, pp.QueryParams["v"]);
					return;
				}

				if (pp.Path.StartsWith("/embed/") && pp.Path.Length > 7) // if they paste an embed link
				{
					DoTemplate(w, YouTube, width, height, pp.Path[7..]);
					return;
				}

				if (pp.Path == "/view_play_list" && pp.QueryParams.ContainsKey("p")) // http://www.youtube.com/view_play_list?p=76E50B82FA870C1D
				{
					DoTemplate(w, YouTubePlaylist, width, height, pp.QueryParams["p"]);
				}

				break;
			case "youtu.be":
				if (pp.Path.Length > 1) // https://youtu.be/yLORZbc-PZw
				{
					DoTemplate(w, YouTube, width, height, pp.Path[1..]);
				}

				break;
			case "vimeo.com":
				if (pp.Path.Length > 1) // http://vimeo.com/49142543
				{
					DoTemplate(w, Vimeo, width, height, pp.Path[1..]);
				}

				break;
			case "dailymotion.com":
			case "www.dailymotion.com": // http://www.dailymotion.com/video/xf4u2m_snes-breath-of-fire-wip-by-janus_videogames
				if (pp.Path.StartsWith("/video/") && pp.Path.Length > 7)
				{
					DoTemplate(w, DailyMotion, width, height, pp.Path[7..].Split('_')[0]);
				}

				break;
			case "www.nicovideo.jp": // https://www.nicovideo.jp/watch/sm35061034
				if (pp.Path.StartsWith("/watch/") && pp.Path.Length > 7)
				{
					var vid = pp.Path[7..];
					var sw = new StringWriter();
					DoTemplate(sw, NicoVideoDocument, width, height, vid);
					DoTemplate(w, NicoVideo, width, height, Convert.ToBase64String(Encoding.UTF8.GetBytes(sw.ToString())));
				}

				break;
			case "archive.org":
				// https://blog.archive.org/2011/03/31/how-archive-org-items-are-structured/
				string[] prefixes = { "/details/", "/download/" };
				foreach (var prefix in prefixes)
				{
					if (pp.Path.StartsWith(prefix))
					{
						var vid = pp.Path[prefix.Length..];
						if (vid.Length > 0)
						{
							// Retain everything after the prefix, which is an identifer and optional filename.
							DoTemplate(w, ArchiveOrg, width, height, vid);
							break;
						}
					}
				}

				break;
		}
	}
}
