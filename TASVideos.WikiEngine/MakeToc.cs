using TASVideos.WikiEngine.AST;

namespace TASVideos.WikiEngine;

public static partial class Builtins
{
	internal static readonly HashSet<string> TocHeadings = new()
	{
		// h1, h5, h6 are not involved in TOC generation
		"h2",
		"h3",
		"h4"
	};

	public static Element MakeToc(List<INode> document, int charStart)
	{
		var headings = NodeUtils.Find(document, e => e.Type == NodeType.Element && TocHeadings.Contains(((Element)e).Tag))
			.Cast<Element>()
			.ToList();

		var ret = new Element(charStart, "div") { Attributes = { ["class"] = "toc" } };
		var stack = new Stack<Element>();
		stack.Push(ret);

		var pos = (headings.Min(h => (int?)(h.Tag[1] - '0')) ?? 2) - 1; // if the biggest heading is h3 or h4, make that the top level

		foreach (var h in headings)
		{
			var i = h.Tag[1] - '0'; // 2, 3, or 4?
			while (i > pos)
			{
				if (stack.Peek().Tag == "ul")
				{
					var li = new Element(charStart, "li");
					stack.Peek().Children.Add(li);
					stack.Push(li);
				}

				var next = new Element(charStart, "ul");
				stack.Peek().Children.Add(next);
				stack.Push(next);
				pos++;
			}

			while (i < pos)
			{
				if (stack.Pop().Tag == "ul")
				{
					pos--;
				}
			}

			{
				if (stack.Peek().Tag == "li")
				{
					stack.Pop();
				}

				var link = new Element(
					charStart,
					"a",
					new[] { Attr("href", "#" + h.Attributes["id"]) },
					h.Children.SelectMany(c => c.CloneForToc()));

				var li = new Element(charStart, "li", new[] { link });
				stack.Peek().Children.Add(li);
				stack.Push(li);
			}
		}

		return ret;
	}
}
