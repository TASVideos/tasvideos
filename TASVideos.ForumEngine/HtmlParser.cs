using System;
using System.Linq;
using System.Collections.Generic;
using AngleSharp.Dom;
using System.Text;

namespace TASVideos.ForumEngine
{
	public class HtmlParser
	{
		public static bool ContainsHtml(string text)
		{
			var p = new AngleSharp.Parser.Html.HtmlParser();
			var dom = p.Parse("<html><body></body></html>");
			var nodes = p.ParseFragment(text, dom.Body);
			var sb = new StringBuilder();
			foreach (var node in nodes)
			{
				if (node.NodeType != NodeType.Text)
					return false;
				sb.Append(node.TextContent);
			}
			return sb.ToString() == text;
		}

		public static Element Parse(string text)
		{
			var p = new AngleSharp.Parser.Html.HtmlParser();
			var dom = p.Parse("<html><body></body></html>");
			var nodes = p.ParseFragment(text, dom.Body);
			return new Element { Name = "_root", Children = nodes.SelectMany(NodeToNodes).ToList() };
		}

		private static Node[] NodeToNodes(INode n)
		{
			switch (n.NodeType)
			{
				case NodeType.Text:
					return new[] { new Text { Content = n.TextContent } };
				case NodeType.Element:
					return new[] { ElementToNode((IElement)n) };
				case NodeType.Comment:
					return new Node[0];
				default:
					throw new Exception("Unknown type " + n.NodeType);
			}
		}

		private static readonly Dictionary<string, string> NoOptionElements = new Dictionary<string, string>
		{
			{ "B", "b" },
			{ "I", "i" },
			{ "EM", "i" },
			{ "U", "u" },
			{ "PRE", "code" },
			{ "CODE", "code" },
			{ "TT", "tt" },
			{ "STRIKE", "s" },
			{ "S", "s" },
			{ "DEL", "s" },
			{ "SUP", "sup" },
			{ "SUB", "sub" },
			{ "DIV", "div" }, // NOT ALLOWED OR PRODUCED BY BBCODE PARSER
			{ "P", "p" }, // NOT ALLOWED OR PRODUCED BY BBCODE PARSER
			{ "SPAN", "span" }, // NOT ALLOWED OR PRODUCED BY BBCODE PARSER
		};

		private static readonly Dictionary<string, string> NoOptionVoidElements = new Dictionary<string, string>
		{
			{ "BR", "br" } // NOT ALLOWED OR PRODUCED BY BBCODE PARSER
		};

		private static readonly HashSet<string> _junkedTags = new HashSet<string>
		{
			"TABLE",
			"H4",
			"H3",
			"BLOCKQUOTE",
			"STYLE"
		};

		private static Node ElementToNode(IElement e)
		{
			if (NoOptionElements.TryGetValue(e.TagName, out var name))
			{
				return new Element { Name = name, Children = e.ChildNodes.SelectMany(NodeToNodes).ToList() };
			}
			if (NoOptionVoidElements.TryGetValue(e.TagName, out name))
			{
				return new Element { Name = name };
			}
			if (_junkedTags.Contains(e.TagName))
			{
				return new Text { Content = e.OuterHtml };
			}
			if (e.TagName == "A")
			{
				var href = e.GetAttribute("href");
				if (string.IsNullOrEmpty("href"))
					throw new Exception("Empty href");
				return new Element { Name = "url", Options = href, Children = e.ChildNodes.SelectMany(NodeToNodes).ToList() };
			}
			if (e.TagName == "SMALL")
			{
				// TODO: What are the right Options to use here?
				return new Element { Name = "size", Options = "small", Children = e.ChildNodes.SelectMany(NodeToNodes).ToList() };
			}
			if (e.TagName == "HR")
			{
				return new Text { Content = "\n" };
			}
			throw new Exception("Unknown tag " + e.TagName);
		}
	}
}
