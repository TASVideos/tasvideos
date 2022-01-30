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
			var ret = new Element(at, "span");
			if (clazz != null)
			{
				ret.Attributes.Add("class", clazz);
			}

			ret.Children.Add(new Text(at, content));
			return ret;
		}

		var lines = content.Split('\n');
		var linePadding = (lines.Length + 1).ToString().Length;

		var ret = new List<INode>();

		var head = new Element(0, "h1", new[] { new Text(0, "Syntax Error") });
		ret.Add(head);

		var elt = new Element(0, "pre")
		{
			Attributes = { ["class"] = "error-code" }
		};

		var lineNo = 1;
		var charAt = 0;

		foreach (var line in lines)
		{
			elt.Children.Add(ClassedText(charAt, $"{lineNo.ToString().PadLeft(linePadding)}. ", "info"));
			if (e.TextLocation >= charAt && e.TextLocation <= charAt + line.Length)
			{
				elt.Children.Insert(0, ClassedText(0, $"Syntax Error on line {lineNo}\n", "info"));
				var column = e.TextLocation - charAt;
				elt.Children.Add(ClassedText(charAt, line.Substring(0, column)));
				var marker = new Element(e.TextLocation, "span", new[] { ClassedText(e.TextLocation, e.Message) })
				{
					Attributes = { ["class"] = "error-marker" }
				};
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
