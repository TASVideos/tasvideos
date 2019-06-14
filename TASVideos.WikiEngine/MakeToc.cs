using System.Collections.Generic;
using System.Linq;
using TASVideos.WikiEngine.AST;

namespace TASVideos.WikiEngine
{
	public static partial class Builtins
	{
		internal static readonly HashSet<string> TocHeadings = new HashSet<string>
		{
			// h1, h5, h6 are not involved in TOC generation
			"h2", "h3", "h4"
		};

		public static Element MakeToc(List<INode> document, int charStart)
		{
			var headings = NodeUtils.Find(document, e => e.Type == NodeType.Element && TocHeadings.Contains(((Element)e).Tag))
				.Cast<Element>()
				.ToList();

			var ret = new Element(charStart, "div");
			ret.Attributes["class"] = "toc";
			var stack = new Stack<Element>();
			stack.Push(ret);

			var pos = 1;
			foreach (var h in headings)
			{
				var i = h.Tag[1] - '0'; // 2, 3, or 4?
				while (i > pos)
				{
					var next = new Element(charStart, "ul");
					stack.Peek().Children.Add(next);
					stack.Push(next);
					pos++;
				}
				while (i < pos)
				{
					if (stack.Pop().Tag == "ul")
						pos--;
				}
				{
					if (stack.Peek().Tag == "li")
						stack.Pop();

					var link = new Element(charStart, "a",
						new[] { Attr("href", "#" + h.Attributes["id"]) },
						h.Children.SelectMany(c => c.CloneForToc()));

					var li = new Element(charStart, "li", new[] { link });
					stack.Peek().Children.Add(li);
				}
			}
			return ret;
		}
	}
}
