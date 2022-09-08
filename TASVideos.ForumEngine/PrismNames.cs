namespace TASVideos.ForumEngine;

public static class PrismNames
{
	public static string FixLanguage(string s)
	{
		return s switch
		{
			"bat" => "batch",
			"sh" => "shell",
			"c++" => "cpp",
			_ => s
		};
	}
}
