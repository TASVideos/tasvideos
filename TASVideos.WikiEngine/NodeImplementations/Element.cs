// ReSharper disable MethodHasAsyncOverload

namespace TASVideos.WikiEngine.AST;

public partial class Element : INodeWithChildren
{
	private static readonly Regex AllowedTagNames = AllowedTagNamesRegex();
	private static readonly Regex AllowedAttributeNames = AllowedAttributeNamesRegex();
	private static readonly HashSet<string> VoidTags =
	[
		"area",
		"base",
		"br",
		"col",
		"embed",
		"hr",
		"img",
		"input",
		"keygen",
		"link",
		"meta",
		"param",
		"source",
		"track",
		"wbr"
	];
	public NodeType Type => NodeType.Element;
	public List<INode> Children { get; private set; } = [];
	public IDictionary<string, string> Attributes { get; private set; } = new Dictionary<string, string>();
	public string Tag { get; }
	public int CharStart { get; }
	public int CharEnd { get; set; }
	public Element(int charStart, string tag)
	{
		if (!AllowedTagNames.IsMatch(tag))
		{
			throw new InvalidOperationException("Invalid tag name");
		}

		if (tag is "script" or "style")
		{
			// we don't escape for these
			throw new InvalidOperationException("Unsupported tag!");
		}

		CharStart = charStart;
		Tag = tag;
	}

	public Element(int charStart, string tag, IEnumerable<INode> children)
		: this(charStart, tag)
	{
		Children.AddRange(children);
	}

	public Element(int charStart, string tag, IEnumerable<KeyValuePair<string, string>> attributes, IEnumerable<INode> children)
		: this(charStart, tag, children)
	{
		foreach (var kvp in attributes)
		{
			Attributes.Add(kvp.Key, kvp.Value);
		}
	}

	public async Task WriteHtmlAsync(TextWriter w, WriterContext ctx)
	{
		if (VoidTags.Contains(Tag) && Children.Count > 0)
		{
			throw new InvalidOperationException("Void tag with child content!");
		}

		IEnumerable<KeyValuePair<string, string>> attrs = Attributes;
		if (Tag == "td")
		{
			var style = ctx.RunTdStyleFilters(InnerText(ctx.Helper));
			if (style != null)
			{
				attrs = attrs.Concat([new KeyValuePair<string, string>("style", style)]);
			}
		}

		w.Write('<');
		w.Write(Tag);
		foreach (var a in attrs)
		{
			if (!AllowedAttributeNames.IsMatch(a.Key))
			{
				throw new InvalidOperationException("Invalid attribute name");
			}

			w.Write(' ');
			w.Write(a.Key);
			w.Write("=\"");
			var value = a.Value;
			if (Tag == "a" && a.Key == "href")
			{
				_ = ctx.Helper.AbsoluteUrl(value);
			}

			foreach (var c in a.Value)
			{
				switch (c)
				{
					case '<':
						w.Write("&lt;");
						break;
					case '&':
						w.Write("&amp;");
						break;
					case '"':
						w.Write("&quot;");
						break;
					default:
						w.Write(c);
						break;
				}
			}

			w.Write('"');
		}

		if (VoidTags.Contains(Tag))
		{
			w.Write(" />");
		}
		else
		{
			w.Write('>');
			foreach (var c in Children)
			{
				await c.WriteHtmlAsync(w, ctx);
			}

			w.Write("</");
			w.Write(Tag);
			w.Write('>');
		}
	}

	public async Task WriteTextAsync(TextWriter writer, WriterContext ctx)
	{
		foreach (var c in Children)
		{
			await c.WriteTextAsync(writer, ctx);
		}

		switch (Tag)
		{
			case "a":
				if (Attributes.TryGetValue("href", out var href))
				{
					writer.Write(" ( ");
					writer.Write(ctx.Helper.AbsoluteUrl(href).Replace(" ", "%20"));
					writer.Write(" )");
				}

				break;
			case "div":
			case "br":
				writer.Write('\n');
				break;
			case "hr":
				writer.Write("--------\n");
				break;
		}
	}

	public async Task WriteMetaDescriptionAsync(StringBuilder sb, WriterContext ctx)
	{
		// write all except for divs that aren't p (which aims to exclude TOC, tabs, etc.)
		if (Tag != "div" || Attributes.TryGetValue("class", out string? classes) && classes.Split(' ').Contains("p"))
		{
			foreach (var c in Children)
			{
				if (sb.Length >= SiteGlobalConstants.MetaDescriptionLength)
				{
					break;
				}

				await c.WriteMetaDescriptionAsync(sb, ctx);
			}
		}
	}

	public INode Clone()
	{
		var ret = (Element)MemberwiseClone();
		ret.Children = Children.Select(c => c.Clone()).ToList();
		ret.Attributes = new Dictionary<string, string>(Attributes);
		return ret;
	}

	public string InnerText(IWriterHelper h)
	{
		return string.Join("", Children.Select(c => c.InnerText(h)));
	}

	public void DumpContentDescriptive(TextWriter w, string padding)
	{
		w.Write(padding);
		w.Write('[');
		w.Write(Tag);
		w.Write(' ');
		foreach (var kvp in Attributes.OrderBy(z => z.Key))
		{
			w.Write(kvp.Key);
			w.Write('=');
			w.Write(kvp.Value);
			w.Write(' ');
		}

		w.WriteLine();
		foreach (var child in Children)
		{
			child.DumpContentDescriptive(w, padding + '\t');
		}

		w.Write(padding);
		w.Write(']');
		w.Write(Tag);
		w.WriteLine();
	}

	private static readonly HashSet<string> TocTagBlacklist = ["a", "br"];

	public IEnumerable<INode> CloneForToc()
	{
		var children = Children.SelectMany(n => n.CloneForToc());
		if (TocTagBlacklist.Contains(Tag))
		{
			return children;
		}

		var ret = (Element)MemberwiseClone();
		ret.Children = children.ToList();
		ret.Attributes = new Dictionary<string, string>(Attributes);
		return new[] { ret };
	}

	[GeneratedRegex("^[a-z0-9]+$")]
	private static partial Regex AllowedTagNamesRegex();

	[GeneratedRegex("^[a-z\\-]+$")]
	private static partial Regex AllowedAttributeNamesRegex();
}
