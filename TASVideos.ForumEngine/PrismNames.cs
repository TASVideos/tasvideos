namespace TASVideos.ForumEngine;

public static class PrismNames
{
	public static string FixLanguage(string s)
	{
		switch (s)
		{
			case "bat":
				return "batch";
			case "sh":
				return "shell";
			case "c++":
				return "cpp";
			default:
				return s;
		}
	}
}
