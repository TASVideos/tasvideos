using TASVideos.WikiEngine.AST;

namespace TASVideos.WikiEngine;

public static partial class Builtins
{
	/// <summary>
	/// Make an error page from wiki content.
	/// </summary>
	/// <param name="content">The full unparsed document.</param>
	/// <param name="e">The syntax exception that came from parsing.</param>
	/// <returns>Human-readable document showing the markup and the error.</returns>
	public static List<INode> MakeErrorPage(string content, NewParser.SyntaxException e)
	{
		static INode ClassedText(int at, string content, string? clazz = null)
		{
			Element ret = new(
				at,
				"span",
				attributes: clazz is not null ? [new("class", clazz)] : [],
				new Text(at, content));
			return ret;
		}

		var lines = content.Split('\n');
		var linePadding = (lines.Length + 1).ToString().Length;

		var ret = new List<INode>();

		Element head = new(0, "h1", attributes: [], new Text(0, "Syntax Error"));
		ret.Add(head);

		Element elt = new(0, "pre", attributes: [new("class", "error-code")]);

		var lineNo = 1;
		var charAt = 0;

		foreach (var line in lines)
		{
			elt.Children.Add(ClassedText(charAt, $"{lineNo.ToString().PadLeft(linePadding)}. ", "info"));
			if (e.TextLocation >= charAt && e.TextLocation <= charAt + line.Length)
			{
				elt.Children.Insert(0, ClassedText(0, $"Syntax Error on line {lineNo}\n", "info"));
				var column = e.TextLocation - charAt;
				elt.Children.Add(ClassedText(charAt, line[..column]));
				Element marker = new(
					e.TextLocation,
					"span",
					attributes: [new("class", "error-marker")],
					ClassedText(e.TextLocation, e.Message));
				elt.Children.Add(marker);
				elt.Children.Add(ClassedText(charAt, string.Concat(line.AsSpan(column), "\n")));
			}
			else
			{
				elt.Children.Add(ClassedText(charAt, line + '\n'));
			}

			charAt += line.Length + 1;
			lineNo++;
		}

		ret.Add(elt);

		return ret;
	}
}
