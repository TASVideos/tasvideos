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
	
	public class Element : INode
	{
		private static readonly Regex AllowedTagNames = new Regex("[a-z]+");
		private static readonly Regex AllowedAttributeNames = new Regex("[a-z\\-]+");
		private static readonly HashSet<string> VoidTags = new HashSet<string>
		{
			"area", "base", "br", "col", "embed", "hr", "img", "input",
			"keygen", "link", "meta", "param", "source", "track", "wbr"
		};
		private static readonly IEnumerable<INode> EmptyChildren = new INode[0];
		private static readonly ReadOnlyDictionary<string, string> EmptyAttributes = new ReadOnlyDictionary<string, string>(new Dictionary<string, string>());
		public NodeType Type => NodeType.Element;
		public IEnumerable<INode> Children { get; } = EmptyChildren;
		public IReadOnlyDictionary<string, string> Attributes { get; } = EmptyAttributes;
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
			var tmp = children.ToList().AsReadOnly();
			if (VoidTags.Contains(tag) && tmp.Count > 0)
			{
				throw new InvalidOperationException("Void tag with child content!");
			}
			Children = tmp;
		}
		public Element(string tag, IEnumerable<KeyValuePair<string, string>> attributes, IEnumerable<INode> children)
			:this(tag, children)
		{
			foreach (var k in attributes.Select(a => a.Key))
			{
				if (!AllowedAttributeNames.IsMatch(k))
				{
					throw new InvalidOperationException("Invalid attribute name");
				}
			}
			Attributes = new ReadOnlyDictionary<string, string>(attributes.ToDictionary(kvp => kvp.Key, kvp => kvp.Value));
		}
		public void WriteHtml(TextWriter w)
		{
			w.Write('<');
			w.Write(Tag);
			foreach (var a in Attributes)
			{
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

	public class IfModule : INode
	{
		public NodeType Type => NodeType.IfModule;
		public IEnumerable<INode> Children { get; }
		public string Condition { get; }
		public IfModule(string condition, IEnumerable<INode> children)
		{
			Condition = condition;
			Children = children.ToList().AsReadOnly();
		}
		public void WriteHtml(TextWriter w)
		{
			// razor stuff
			w.Write("@if(Html.WikiCondition(\"");
			foreach (var c in Condition)
			{
				if (c < 0x20)
				{
					w.Write($"\\x{((int)c).ToString("x2")}");
				}
				else if (c == '"')
				{
					w.Write("\\\"");
				}
				else
				{
					w.Write(c);
				}
			}
			w.Write("\")){<text>");
			foreach (var c in Children)
				c.WriteHtml(w);
			w.Write("</text>}");
		}
	}

	public class Module : INode
	{
		public NodeType Type => NodeType.Module;
		public string Text { get; }
		public Module(string text)
		{
			Text = text;
		}
		public void WriteHtml(TextWriter w)
		{
			w.Write("<!-- TODO MODULE TAG -->");
		}
	}
}
