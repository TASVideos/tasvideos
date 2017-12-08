using System;
using Newtonsoft.Json;

namespace TASVideos.WikiEngine
{
	public static class Util
	{
		public static string DebugParseWikiPage(string content)
		{
			var parser = new Wiki();
			var result = parser.GetMatch(content, parser.Document);
			if (result.Success)
			{
				return JsonConvert.SerializeObject(result.Results, Formatting.Indented);
			}
			throw new Exception("Waaaaaaaaaaaaaaaaaaaaaaaaaa");
		}
	}
}
