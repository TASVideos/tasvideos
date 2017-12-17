using System;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.RegularExpressions;

namespace TASVideos.WikiEngine.AST
{
	public enum NodeType
	{
		Text,
		Element,
		IfModule,
		Module
	}
	public interface INode
	{
		[JsonConverter(typeof(StringEnumConverter))]
		NodeType Type { get; }
		void WriteHtml(TextWriter w);
	}
	public interface INodeWithChildren : INode
	{
		List<INode> Children { get; }
	}

	public class Text : INode
	{
		public NodeType Type => NodeType.Text;
		public string Content { get; }
		public Text(string content)
		{
			Content = content;
		}
		public void WriteHtml(TextWriter w)
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
					case '@':
						w.Write("&#64;");
						break;
					default:
						w.Write(c);
						break;
				}
			}
		}
	}
	
	public class Element : INodeWithChildren
	{
		private static readonly Regex AllowedTagNames = new Regex("^[a-z0-9]+$");
		private static readonly Regex AllowedAttributeNames = new Regex("^[a-z\\-]+$");
		private static readonly HashSet<string> VoidTags = new HashSet<string>
		{
			"area", "base", "br", "col", "embed", "hr", "img", "input",
			"keygen", "link", "meta", "param", "source", "track", "wbr"
		};
		public NodeType Type => NodeType.Element;
		public List<INode> Children { get; } = new List<INode>();
		public IDictionary<string, string> Attributes { get; } = new Dictionary<string, string>();
		public string Tag { get; }
		public Element(string tag)
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
			Tag = tag;
		}
		public Element(string tag, IEnumerable<INode> children)
			:this(tag)
		{
			Children.AddRange(children);
		}
		public Element(string tag, IEnumerable<KeyValuePair<string, string>> attributes, IEnumerable<INode> children)
			:this(tag, children)
		{
			foreach (var kvp in attributes)
				Attributes.Add(kvp.Key, kvp.Value);
		}
		public void WriteHtml(TextWriter w)
		{	if (VoidTags.Contains(Tag) && Children.Count > 0)
			{
				throw new InvalidOperationException("Void tag with child content!");
			}
			w.Write('<');
			w.Write(Tag);
			foreach (var a in Attributes)
			{
				if (!AllowedAttributeNames.IsMatch(a.Key))
				{
					throw new InvalidOperationException("Invalid attribute name");
				}
				w.Write(' ');
				w.Write(a.Key);
				w.Write("=\"");
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
						case '@':
							w.Write("&#64;");
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
					c.WriteHtml(w);
				w.Write("</");
				w.Write(Tag);
				w.Write('>');
			}
		}
	}

	public class IfModule : INodeWithChildren
	{
		public NodeType Type => NodeType.IfModule;
		public List<INode> Children { get; } = new List<INode>();
		public string Condition { get; }
		public IfModule(string condition)
		{
			Condition = condition;
		}
		public IfModule(string condition, IEnumerable<INode> children)
			: this(condition)
		{
			Children.AddRange(children);
		}
		public void WriteHtml(TextWriter w)
		{
			// razor stuff
			w.Write("@if(Html.WikiCondition(");
			Escape.WriteCSharpString(w, Condition);
			w.Write(")){<text>");
			foreach (var c in Children)
				c.WriteHtml(w);
			w.Write("</text>}");
		}
	}

	public class Module : INode
	{
		private static readonly Dictionary<string, string> ModuleNameMaps = new Dictionary<string, string>
		{
			["listsubpages"] = "ListSubpages",
			["__wikiLink"] = "WikiLink",
			["WikiOrphans"] = "WikiOrphans"
		};
		public NodeType Type => NodeType.Module;
		public string Text { get; }
		public Module(string text)
		{
			Text = text;
		}
		public void WriteHtml(TextWriter w)
		{
			var pp = Text.Split(new[] { '|' }, 2);
			var moduleName = pp[0];
			var moduleParams = pp.Length > 1 ? pp[1] : "";
			if (ModuleNameMaps.TryGetValue(moduleName, out string realModuleName))
			{
				w.Write("@await Component.InvokeAsync(");
				Escape.WriteCSharpString(w, realModuleName);
				w.Write(", new { pageData = Model, pp = ");
				Escape.WriteCSharpString(w, moduleParams);
				w.Write(" })");
			}
			else
			{
				var div = new Element("div");
				div.Children.Add(new Text("Unknown module " + moduleName));
				div.Attributes["class"] = "module-error";
				div.WriteHtml(w);
			}
		}
	}
}
