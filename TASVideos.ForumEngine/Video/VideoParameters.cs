using System;
using System.Collections.Generic;

namespace TASVideos.ForumEngine
{
	public class VideoParameters
	{
		public int? Width { get; set; }
		public int? Height { get; set; }
		public string UrlRaw { get; set; }
		public string Host { get; set; }
		public Dictionary<string, string> QueryParams { get; set; }
		public string Path { get; set; }
	}
}
