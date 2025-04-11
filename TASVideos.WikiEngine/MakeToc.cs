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
		var headings = NodeUtils.Find(
				document,
				e => e.Type == NodeType.Element && TocHeadings.Contains(((Element)e).Tag),
				ancestor => ancestor is Element parentElement && parentElement.Tag == "tab")
			.Cast<Element>()
			.ToList();

		var strong = new Element(charStart, "strong");
		strong.Children.Add(new Text(charStart, "Table of contents"));
		var header = new Element(charStart, "div") { Attributes = { ["class"] = "card-header" } };
		header.Children.Add(strong);
		var ret = new Element(charStart, "div") { Attributes = { ["class"] = "card mb-2" } };
		ret.Children.Add(header);

		var stack = new Stack<Element>();
		var body = new Element(charStart, "div") { Attributes = { ["class"] = "card-body" } };
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
					[Attr("href", "#" + h.Attributes["id"])],
					h.Children.SelectMany(c => c.CloneForToc()));

				var li = new Element(charStart, "li", new[] { link });
				stack.Peek().Children.Add(li);
				stack.Push(li);
			}
		}

		return ret;
	}
}
