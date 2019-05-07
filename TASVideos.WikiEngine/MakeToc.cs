using System.Collections.Generic;
using System.Linq;
using TASVideos.WikiEngine.AST;

namespace TASVideos.WikiEngine
{
	public static partial class Builtins
	{
		private static readonly HashSet<string> Headings = new HashSet<string>
		{
			// h1, h5, h6 are not involved in TOC generation
			"h2", "h3", "h4"
		};

		public static Element MakeToc(List<INode> document, int charStart)
		{
			var headings = NodeUtils.Find(document, e => e.Type == NodeType.Element && Headings.Contains(((Element)e).Tag))
				.Cast<Element>()
				.ToList();

			var ret = new Element(charStart, "div");
			ret.Attributes["class"] = "toc";
			var stack = new Stack<Element>();
			{
				var ul = new Element(charStart, "ul");
				ret.Children.Add(ul);
				stack.Push(ul);
			}
			var pos = 2;
			foreach (var h in headings)
			{
				var i = h.Tag[1] - '0'; // 2, 3, or 4?
				while (i > pos)
				{
					var next = new Element(charStart, "ul");
					var li = new Element(charStart, "li", new[] { next });
					stack.Peek().Children.Add(li);
					stack.Push(next);
					pos++;
				}
				while (i < pos)
				{
					stack.Pop();
					pos--;
				}
				{
					var id = "heading-" + h.CharStart;

					var link = new Element(charStart, "a");
					var li = new Element(charStart, "li", new[] { link });
					link.Attributes["href"] = "#" + id;
					link.Children.AddRange(h.Children.SelectMany(c => c.CloneForToc()));
					stack.Peek().Children.Add(li);

					var anchor = new Element(h.CharStart, "a");
					anchor.Attributes["id"] = id;
					h.Children.Add(anchor);
				}
			}
			return ret;
		}
	}
}
