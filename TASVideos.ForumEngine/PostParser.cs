using System.Collections.Generic;
using System.Linq;

namespace TASVideos.ForumEngine
{
	public class PostParser
	{
		public static Element Parse(string text, bool enableBbCode, bool enableHtml)
		{
			if (enableHtml)
			{
				var ret = HtmlParser.Parse(text);
				return enableBbCode 
					? RecursiveBbParse(ret).Cast<Element>().Single() 
					: ret;
			}
			else if (enableBbCode)
			{
				return BbParser.Parse(text);
			}
			else
			{
				var ret = new Element { Name = "_root" };
				ret.Children.Add(new Text { Content = text });
				return ret;
			}
		}

		private static IEnumerable<Node> RecursiveBbParse(Node n)
		{
			var e = n as Element;
			if (e != null)
			{
				e.Children = e.Children.SelectMany(RecursiveBbParse).ToList();
				return new[] { e };
			}

			var t = n as Text;
			var results = BbParser.Parse(t.Content);
			return results.Children;
		}
	}
}
