namespace TASVideos.WikiEngine.AST;

public class Module : INode
{
	public NodeType Type => NodeType.Module;
	public string Name { get; }
	public IReadOnlyDictionary<string, string> Parameters { get; }
	public int CharStart { get; }
	public int CharEnd { get; set; }
	public Module(int charStart, int charEnd, string text)
	{
		CharStart = charStart;
		CharEnd = charEnd;

		var pp = text.Split('|');
		Name = pp[0];
		Parameters = pp.Skip(1)
			.Select(p => p.Split(['='], 2))
			.Where(p => !string.IsNullOrWhiteSpace(p[0]))
			.Select(p => new KeyValuePair<string, string>(p[0].Trim().ToLowerInvariant(), p.Length > 1 ? p[1].Trim() : ""))
			.GroupBy(kvp => kvp.Key, StringComparer.InvariantCultureIgnoreCase)
			.Select(g => g.Last()) // When an attribute appears more than once, choose the last value to match the old parser
			.ToDictionary(kvp => kvp.Key, kvp => kvp.Value, StringComparer.InvariantCultureIgnoreCase);
	}

	public async Task WriteHtmlAsync(TextWriter w, WriterContext ctx)
	{
		if (Name.Equals("settableattributes", StringComparison.InvariantCultureIgnoreCase))
		{
			if (!ctx.AddTdStyleFilter(Parameters))
			{
				var div = new Element(CharStart, "div") { CharEnd = CharEnd };
				div.Children.Add(new Text(CharStart, "Module Error for settableattributes: Couldn't parse parameter string.") { CharEnd = CharEnd });
				div.Attributes["class"] = "module-error";
				await div.WriteHtmlAsync(w, ctx);
			}
		}
		else if (ModuleNames.IsModule(Name))
		{
			await ctx.Helper.RunViewComponentAsync(w, Name, Parameters);
		}
		else
		{
			var div = new Element(CharStart, "div") { CharEnd = CharEnd };
			div.Children.Add(new Text(CharStart, "Unknown module " + Name) { CharEnd = CharEnd });
			div.Attributes["class"] = "module-error";
			await div.WriteHtmlAsync(w, ctx);
		}
	}

	public async Task WriteTextAsync(TextWriter writer, WriterContext ctx)
	{
		if (Name.Equals("settableattributes", StringComparison.InvariantCultureIgnoreCase))
		{
			// Do nothing for this special module
		}
		else if (ModuleNames.IsModule(Name))
		{
			// It's the caller's responsibility to provide a view component runner that will create text output.
			await ctx.Helper.RunViewComponentAsync(writer, Name, Parameters);
		}
		else
		{
			await writer.WriteAsync($"ERROR:  Unknown module {Name}");
		}
	}

	public async Task WriteMetaDescriptionAsync(StringBuilder sb, WriterContext ctx)
	{
		if (sb.Length >= SiteGlobalConstants.MetaDescriptionLength)
		{
			return;
		}

		if (ModuleNames.IsModule(Name))
		{
			StringWriter sw = new();

			// It's the caller's responsibility to provide a view component runner that will create text output.
			await ctx.Helper.RunViewComponentAsync(sw, Name, Parameters);

			string moduleContent = sw.ToString();
			if (sb.Length + moduleContent.Length < SiteGlobalConstants.MetaDescriptionLength)
			{
				sb.Append(moduleContent);
			}
			else
			{
				sb.Append('…');
				sb.Append(' ', SiteGlobalConstants.MetaDescriptionLength - sb.Length);
			}
		}
	}

	public INode Clone()
	{
		return (Module)MemberwiseClone();
	}

	public string InnerText(IWriterHelper h)
	{
		// Too hard to run modules here, and not useful.
		// But, for the specific case of __wikiLink, it "feels" like a link and not a module
		// to the end user.  TODO:  __wikiLink really needs to be its own AST type.
		if (Name == "__wikiLink")
		{
			if (Parameters.TryGetValue("displaytext", out var displayText))
			{
				return displayText;
			}

			if (Parameters.TryGetValue("href", out var href))
			{
				return href[1..];
			}

			return "";
		}

		return "";
	}

	public void DumpContentDescriptive(TextWriter w, string padding)
	{
		w.Write(padding);
		w.Write('(');
		w.Write(Name);
		foreach (var (k, v) in Parameters.OrderBy(kvp => kvp.Key))
		{
			w.Write('|');
			w.Write(k);
			w.Write('=');
			w.Write(v);
		}

		w.WriteLine(')');
	}

	public IEnumerable<INode> CloneForToc()
	{
		// See comment above
		if (Name != "__wikiLink")
		{
			return [];
		}

		Parameters.TryGetValue("displaytext", out var displayText);
		Parameters.TryGetValue("href", out var href);
		return new[] { new Text(CharStart, displayText ?? href?[1..] ?? "") { CharEnd = CharEnd } };
	}
}
