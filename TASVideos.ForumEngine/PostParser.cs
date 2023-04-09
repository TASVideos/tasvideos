namespace TASVideos.ForumEngine;

public static class PostParser
{
	public static Element Parse(string text, bool enableBbCode, bool enableHtml)
	{
		if (enableHtml || enableBbCode)
		{
			return BbParser.Parse(text, enableHtml, enableBbCode);
		}

		var ret = new Element { Name = "_root" };
		ret.Children.Add(new Text { Content = text });
		return ret;
	}
}
