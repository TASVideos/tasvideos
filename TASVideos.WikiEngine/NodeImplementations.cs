using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TASVideos.WikiEngine.AST
{
	public class Text : INode
	{
		public NodeType Type => NodeType.Text;
		public string Content { get; }
		public int CharStart { get; }
		public int CharEnd { get; set; }
		public Text(int charStart, string content)
		{
			CharStart = charStart;
			Content = content;
		}

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
			return new[] { Clone() };
		}
	}

	public class Element : INodeWithChildren
	{
		private static readonly Regex AllowedTagNames = new ("^[a-z0-9]+$");
		private static readonly Regex AllowedAttributeNames = new ("^[a-z\\-]+$");
		private static readonly HashSet<string> VoidTags = new ()
		{
			"area", "base", "br", "col", "embed", "hr", "img", "input",
			"keygen", "link", "meta", "param", "source", "track", "wbr"
		};
		public NodeType Type => NodeType.Element;
		public List<INode> Children { get; private set; } = new ();
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

			if (tag == "script" || tag == "style")
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
					attrs = attrs.Concat(new[] { new KeyValuePair<string, string>("style", style) });
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
					value = ctx.Helper.AbsoluteUrl(value);
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
						writer.Write(" (");
						writer.Write(ctx.Helper.AbsoluteUrl(href).Replace(" ", "%20"));
						writer.Write(')');
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

		private static readonly HashSet<string> TocTagBlacklist = new ()
		{
			"a", "br"
		};

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
	}

	public class IfModule : INodeWithChildren
	{
		public NodeType Type => NodeType.IfModule;
		public List<INode> Children { get; private set; } = new ();
		public string Condition { get; }
		public int CharStart { get; }
		public int CharEnd { get; set; }
		public IfModule(int charStart, string condition)
		{
			CharStart = charStart;
			Condition = condition;
		}

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
			return new[] { Clone() };
		}
	}

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
				.Select(p => p.Split(new[] { '=' }, 2))
				.Where(p => !string.IsNullOrWhiteSpace(p[0]))
				.Select(p => new KeyValuePair<string, string>(p[0].Trim().ToLowerInvariant(), p.Length > 1 ? p[1].Trim() : ""))
				.GroupBy(kvp => kvp.Key, StringComparer.InvariantCultureIgnoreCase)
				.Select(g => g.Last()) // When an attribute appears more than once, choose the last value to match the old parser
				.ToDictionary(kvp => kvp.Key, kvp => kvp.Value, StringComparer.InvariantCultureIgnoreCase);
		}

		public async Task WriteHtmlAsync(TextWriter w, WriterContext ctx)
		{
			if (Name.ToLowerInvariant() == "settableattributes")
			{
				if (!ctx.AddTdStyleFilter(Parameters))
				{
					var div = new Element(CharStart, "div") { CharEnd = CharEnd };
					div.Children.Add(new Text(CharStart, "Module Error for settableattributes: Couldn't parse parameter string.") { CharEnd = CharEnd });
					div.Attributes["class"] = "module-error";
					await div.WriteHtmlAsync(w, ctx);
				}
			}
			else if (WikiModules.IsModule(Name))
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
			if (Name.ToLowerInvariant() == "settableattributes")
			{
				// Do nothing for this special module
			}
			else if (WikiModules.IsModule(Name))
			{
				// It's the caller's responsibility to provide a view component runner that will create text output.
				await ctx.Helper.RunViewComponentAsync(writer, Name, Parameters);
			}
			else
			{
				await writer.WriteAsync($"ERROR:  Unknown module {Name}");
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
				Parameters.TryGetValue("displaytext", out var ret);
				return ret ?? "";
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
			if (Name == "__wikiLink")
			{
				Parameters.TryGetValue("displaytext", out var content);
				return new[] { new Text(CharStart, content ?? "") { CharEnd = CharEnd } };
			}

			return Enumerable.Empty<INode>();
		}
	}
}
