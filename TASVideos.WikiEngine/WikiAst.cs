using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TASVideos.WikiEngine
{
	public class WikiAst
	{
		private readonly string _markup;

		public WikiAst(string markup)
		{
			_markup = markup;
			Parse(_markup);
		}

		public WikiNode WikiTree { get; private set; }

		public string ToHtml()
		{
			return "TODO";
		}

		private void Parse(string markup)
		{
			WikiTree = new WikiNode(null, NodeType.OpenTag, "div");
			WikiTree.ChildNodes = _markup
				.Split(new[] {" "}, StringSplitOptions.RemoveEmptyEntries)
				.Select(s => new WikiNode(WikiTree, NodeType.Text))
				.ToList();
		}

		public override string ToString()
		{
			return WikiTree.ToString();
		}
	}

	// TODO: separate file
	public class WikiNode
	{
		public WikiNode(WikiNode parent, NodeType type, string name = null)
		{
			Parent = parent;
			Type = type;
			Name = !string.IsNullOrWhiteSpace(name) ? name : "text";
		}

		public bool IsRoot => Parent == null;

		public IEnumerable<WikiNode> ChildNodes { get; set; } = new List<WikiNode>();

		public WikiNode Parent { get; }
		public NodeType Type { get; }
		public string Name { get; }

		public override string ToString()
		{
			var sb = new StringBuilder();
			sb
				.Append($"{Type} [{Name}]")
				.AppendLine()
				.Append("\t");

			foreach (var child in ChildNodes)
			{
				sb.Append(child);
			}

			return sb.ToString();
		}
	}

	public enum NodeType
	{
		Text,
		OpenTag,
		CloseTag,
		SelClosingTag
	}
}
