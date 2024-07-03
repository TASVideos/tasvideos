using TASVideos.WikiEngine.AST;

namespace TASVideos.WikiEngine;

public static partial class Builtins
{
	// h1, h5, h6 are not involved in TOC generation
	internal static readonly HashSet<string> TocHeadings =
	[
		"h2",
		"h3",
		"h4"
	];

	public static Element MakeToc(List<INode> document, int charStart)
	{
		var headings = NodeUtils.Find(document, e => e.Type == NodeType.Element && TocHeadings.Contains(((Element)e).Tag))
			.Cast<Element>()
			.ToList();

		Element strong = new(charStart, "strong", attributes: [], new Text(charStart, "Table of contents"));
		Element header = new(charStart, "div", attributes: [new("class", "card-header")], strong);
		Element ret = new(charStart, "div", attributes: [new("class", "card mb-2")], header);

		var stack = new Stack<Element>();
		Element body = new(charStart, "div", attributes: [new("class", "card-body")]);
		ret.Children.Add(body);
		stack.Push(body);

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
					attributes: [Attr("href", "#" + h.Attributes["id"])],
					children: h.Children.SelectMany(static c => c.CloneForToc()));
				Element li = new(charStart, "li", attributes: [], link);
				stack.Peek().Children.Add(li);
				stack.Push(li);
			}
		}

		return ret;
	}
}
