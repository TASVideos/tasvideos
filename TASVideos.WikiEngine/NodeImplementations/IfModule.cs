namespace TASVideos.WikiEngine.AST;

public class IfModule(int charStart, string condition) : INodeWithChildren
{
	public NodeType Type => NodeType.IfModule;
	public List<INode> Children { get; private set; } = [];
	public string Condition { get; } = condition;
	public int CharStart { get; } = charStart;
	public int CharEnd { get; set; }

	public IfModule(int charStart, string condition, IEnumerable<INode> children)
		: this(charStart, condition)
	{
		Children.AddRange(children);
	}

	public async Task WriteHtmlAsync(TextWriter w, WriterContext ctx)
	{
		if (ctx.Helper.CheckCondition(Condition))
		{
			foreach (var c in Children)
			{
				await c.WriteHtmlAsync(w, ctx);
			}
		}
	}

	public async Task WriteTextAsync(TextWriter writer, WriterContext ctx)
	{
		if (ctx.Helper.CheckCondition(Condition))
		{
			foreach (var c in Children)
			{
				await c.WriteTextAsync(writer, ctx);
			}
		}
	}

	public async Task WriteMetaDescriptionAsync(StringBuilder sb, WriterContext ctx)
	{
		if (ctx.Helper.CheckCondition(Condition))
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
		var ret = (IfModule)MemberwiseClone();
		ret.Children = Children.Select(c => c.Clone()).ToList();
		return ret;
	}

	public string InnerText(IWriterHelper h)
	{
		return h.CheckCondition(Condition)
			? string.Join("", Children.Select(c => c.InnerText(h)))
			: "";
	}

	public void DumpContentDescriptive(TextWriter w, string padding)
	{
		w.Write(padding);
		w.Write("?IF ");
		w.Write(Condition);
		w.WriteLine();
		foreach (var child in Children)
		{
			child.DumpContentDescriptive(w, padding + '\t');
		}

		w.Write(padding);
		w.Write("?ENDIF ");
		w.Write(Condition);
		w.WriteLine();
	}

	public IEnumerable<INode> CloneForToc()
	{
		return [Clone()];
	}
}
