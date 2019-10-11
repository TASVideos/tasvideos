using System.Collections.Generic;

namespace TASVideos.ForumEngine
{
	public class VideoParameters
	{
		public int? Width { get; set; }
		public int? Height { get; set; }
		public string Host { get; set; }
		public Dictionary<string, string> QueryParams { get; set; } = new Dictionary<string, string>();
		public string Path { get; set; }

		public VideoParameters(string host, string path)
		{
			Host = host;
			Path = path;
		}
	}
}
