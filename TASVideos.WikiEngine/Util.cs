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
			if (result.Success && result.NextIndex == content.Length)
			{
				var r = result.Results; //TopLevelPasses.MergeDefinitions(result.Results);
				return JsonConvert.SerializeObject(r, Formatting.Indented);
			}
			else
			{
				return JsonConvert.SerializeObject(new
				{
					Error = result.Error,
					ErrorIndex = result.ErrorIndex
				});
			}
		}
		public static void DebugWriteHtml(string content, TextWriter w)
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
				w.Write($"<!-- ERROR {result.Error} @{result.ErrorIndex} -->");
			}
		}
	}
}
