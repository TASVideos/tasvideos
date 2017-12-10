using System;
using System.IO;
using Newtonsoft.Json;

namespace TASVideos.WikiEngine
{
	public static class Util
	{
		public static string DebugParseWikiPage(string content)
		{
			var parser = new Wiki();
			var result = parser.GetMatch(content, parser.Document);
			var ret = "";
			var r = result.Results; //TopLevelPasses.MergeDefinitions(result.Results);
			ret += JsonConvert.SerializeObject(r, Formatting.Indented);
			if (result.Success && result.NextIndex == content.Length)
			{
			}
			else
			{
				ret += JsonConvert.SerializeObject(new
				{
					Error = result.Error,
					ErrorIndex = result.ErrorIndex
				});
			}
			return ret;
		}
		public static void DebugWriteHtml(string content, TextWriter w)
		{
			var parser = new Wiki();
			var result = parser.GetMatch(content, parser.Document);
			foreach (var r in result.Results)
				r.WriteHtml(w);
			if (result.Success && result.NextIndex == content.Length)
			{
			}
			else
			{
				w.Write($"<!-- ERROR {result.Error} @{result.ErrorIndex} -->");
			}
		}

		public static void RenderRazor(string content, TextWriter w)
		{
			var parser = new Wiki();
			var result = parser.GetMatch(content, parser.Document);
			if (result.Success && result.NextIndex == content.Length)
			{
				foreach (var r in result.Results)
					r.WriteHtml(w);
			}
			else
			{
				throw new InvalidOperationException("Parse error at index " + result.ErrorIndex);
			}
		}
	}
}
