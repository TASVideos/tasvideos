using System;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.ObjectModel;

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
	}

	public class Text : INode
	{
		public NodeType Type => NodeType.Text;
		public string Content { get; }
		public Text(string content)
		{
			Content = content;
		}
	}
	
	public class Element : INode
	{
		private static readonly IEnumerable<INode> EmptyChildren = new INode[0];
		private static readonly ReadOnlyDictionary<string, string> EmptyAttributes = new ReadOnlyDictionary<string, string>(new Dictionary<string, string>());
		public NodeType Type => NodeType.Element;
		public IEnumerable<INode> Children { get; } = EmptyChildren;
		public IReadOnlyDictionary<string, string> Attributes { get; } = EmptyAttributes;
		public string Tag { get; }
		public Element(string tag)
		{
			Tag = tag;
		}
		public Element(string tag, IEnumerable<INode> children)
			:this(tag)
		{
			Children = children.ToList().AsReadOnly();
		}
		public Element(string tag, IEnumerable<KeyValuePair<string, string>> attributes, IEnumerable<INode> children)
			:this(tag, children)
		{
			Attributes = new ReadOnlyDictionary<string, string>(attributes.ToDictionary(kvp => kvp.Key, kvp => kvp.Value));
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
	}

	public class Module : INode
	{
		public NodeType Type => NodeType.Module;
		public string Text { get; }
		public Module(string text)
		{
			Text = text;
		}
	}
}
