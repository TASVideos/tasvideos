using System;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace TASVideos.WikiEngine.AST
{
	public enum NodeType
	{
		Text,
		Element,
		IfModule,
		Module
	}
	public interface Node
	{
		[JsonConverter(typeof(StringEnumConverter))]
		NodeType Type { get; }
	}

	public class Text : Node
	{
		public NodeType Type => NodeType.Text;
		public string Content { get; }
		public Text(string content)
		{
			Content = content;
		}
	}
	
	public class Element : Node
	{
		private static readonly IEnumerable<Node> EmptyChildren = new Node[0];
		public NodeType Type => NodeType.Element;
		public IEnumerable<Node> Children { get; }
		public string Tag { get; }
		public Element(string tag, IEnumerable<Node> children = null)
		{
			Tag = tag;
			Children = children?.ToList().AsReadOnly() ?? EmptyChildren;
		}
		public static Element CreateDefinition(IEnumerable<Node> term, IEnumerable<Node> def)
		{
			return new Element("gloss", new[] { new Element("term", term), new Element("def", def) });
		}
	}

	public class IfModule : Node
	{
		public NodeType Type => NodeType.IfModule;
		public IEnumerable<Node> Children { get; }
		public string Condition { get; }
		public IfModule(Node condition, IEnumerable<Node> children)
		{
			Condition = ((Text)condition).Content;
			Children = children.ToList().AsReadOnly();
		}
	}

	public class Module : Node
	{
		public NodeType Type => NodeType.Module;
		public string Text { get; }
		public Module(Node text)
		{
			Text = ((Text)text).Content;
		}
	}
}
