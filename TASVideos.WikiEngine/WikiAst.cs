using System;
using System.Collections.Generic;

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
			WikiTree = new WikiNode(null, NodeType.OpenTag);
		}
	}

	// TODO: separate file
	public class WikiNode
	{
		public WikiNode(WikiNode parent, NodeType type)
		{
			Parent = parent;
			Type = type;
		}

		public bool IsRoot => Parent == null;

		public IEnumerable<WikiNode> ChildNodes { get; set; }

		public WikiNode Parent { get; }
		public NodeType Type { get; }
	}

	public enum NodeType
	{
		Text,
		OpenTag,
		CloseTag,
		SelClosingTag
	}
}
