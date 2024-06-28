namespace TASVideos.WikiEngine.AST;

public class Text(int charStart, string content) : INode
{
	public NodeType Type => NodeType.Text;
	public string Content { get; } = content;
	public int CharStart { get; } = charStart;
	public int CharEnd { get; set; }

	public Task WriteHtmlAsync(TextWriter w, WriterContext ctx)
	{
		foreach (var c in Content)
		{
			switch (c)
			{
				case '<':
					w.Write("&lt;");
					break;
				case '&':
					w.Write("&amp;");
					break;
				default:
					w.Write(c);
					break;
			}
		}

		return Task.CompletedTask;
	}

	public async Task WriteTextAsync(TextWriter writer, WriterContext ctx)
	{
		await writer.WriteAsync(Content);
	}

	public Task WriteMetaDescriptionAsync(StringBuilder sb, WriterContext ctx)
	{
		if (sb.Length >= SiteGlobalConstants.MetaDescriptionLength)
		{
			return Task.CompletedTask;
		}

		sb.Append(Content);
		if (sb.Length > SiteGlobalConstants.MetaDescriptionLength)
		{
			int endIndex = SiteGlobalConstants.MetaDescriptionLength - 1;
			sb.Remove(endIndex, sb.Length - endIndex);
			sb.Append('…');
		}

		return Task.CompletedTask;
	}

	public INode Clone()
	{
		return (Text)MemberwiseClone();
	}

	public string InnerText(IWriterHelper h)
	{
		return Content;
	}

	public void DumpContentDescriptive(TextWriter w, string padding)
	{
		if (Content.Any(c => c < 0x20 && c != '\n' && c != '\t'))
		{
			w.Write(padding);
			w.WriteLine("$UNPRINTABLE TEXT!!!");
		}
		else
		{
			var first = true;
			foreach (var s in Content.Split('\n'))
			{
				if (first)
				{
					first = false;
				}
				else
				{
					w.Write(padding);
					w.WriteLine("$LF");
				}

				if (s.Length > 0)
				{
					w.Write(padding);
					w.Write('"');
					w.WriteLine(s);
				}
			}
		}
	}

	public IEnumerable<INode> CloneForToc()
	{
		return [Clone()];
	}
}
