using System.Collections.Generic;
using TASVideos.WikiEngine.AST;

namespace TASVideos.WikiEngine
{
	public static partial class Builtins
	{
		/// <summary>
		/// Make an error page from wiki content
		/// </summary>
		/// <param name="content">The full unparsed document</param>
		/// <param name="e">The syntax exception that came from parsing</param>
		/// <returns>Human-readable document showing the markup and the error</returns>
		public static List<INode> MakeErrorPage(string content, NewParser.SyntaxException e)
		{
			var ret = new List<INode>();

			var head = new Element(0, "h1", new[] { new Text(0, "Syntax Error") });
			ret.Add(head);

			var elt = new Element(0, "pre");
			elt.Attributes["class"] = "error-code";
			var i = e.TextLocation;
			elt.Children.Add(new Text(0, content.Substring(0, i)));
			var marker = new Element(i, "span", new[] { new Element(i, "span", new[] { new Text(i, e.Message) }) });
			marker.Attributes["class"] = "error-marker";
			elt.Children.Add(marker);
			elt.Children.Add(new Text(i, content.Substring(i)));
			ret.Add(elt);

			return ret;
		}
	}
}
