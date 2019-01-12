using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using TASVideos.WikiEngine.AST;

namespace TASVideos.WikiEngine
{
	public static class Util
	{
		public static string DebugParseWikiPage(string content)
		{
			try
			{
				var results = NewParser.Parse(content);
				return JsonConvert.SerializeObject(results, Formatting.Indented);
			}
			catch (NewParser.SyntaxException e)
			{
				return JsonConvert.SerializeObject(new
				{
					Error = e.Message
				}, Formatting.Indented);
			}
		}
		public static void DebugWriteRazor(string content, TextWriter w)
		{
			try
			{
				var results = NewParser.Parse(content);
				foreach (var r in results)
					r.WriteRazor(w);
			}
			catch (NewParser.SyntaxException e)
			{
				w.Write($"<!-- ERROR {e.Message} -->");
			}
		}

		public static void RenderRazor(string pageName, string content, TextWriter w)
		{
			var results = NewParser.Parse(content);

			foreach (var r in results)
			{
				r.WriteRazor(w);
			}
		}
		public static void RenderHtml(string content, TextWriter w)
		{
			var results = NewParser.Parse(content);

			foreach (var r in results)
			{
				r.WriteHtml(w);
			}
		}

		public static void RenderHtmlDynamic(string content, TextWriter w, IWriterHelper h)
		{
			var results = NewParser.Parse(content);

			foreach (var r in results)
			{
				r.WriteHtmlDynamic(w, h);
			}			
		}

		public static IEnumerable<NewParser.WikiLinkInfo> GetAllWikiLinks(string content)
		{
			return NewParser.GetAllWikiLinks(content).Where(l => !l.Link.Contains("user:"));
		}
	}
}
