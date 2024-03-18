namespace TASVideos.ForumEngine;

public class VideoParameters(string host, string path)
{
	public int? Width { get; set; }
	public int? Height { get; set; }
	public string Host { get; set; } = host;
	public Dictionary<string, string> QueryParams { get; set; } = [];
	public string Path { get; set; } = path;
}
