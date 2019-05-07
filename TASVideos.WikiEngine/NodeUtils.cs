using System;
using System.Collections.Generic;
using System.Linq;

namespace TASVideos.WikiEngine.AST
{
	public class WikiLinkInfo
	{
		public string Link { get; set; }
		public string Excerpt { get; set; }
	}

	public static class NodeUtils
	{
		public static void Replace(IList<INode> input, Func<INode, bool> predicate, Func<INode, INode> transform)
		{
			for (var i = 0; i < input.Count; i++)
			{
				if (predicate(input[i]))
				{
					input[i] = transform(input[i]);
				}
				var cc = input[i] as INodeWithChildren;
				if (cc != null)
					Replace(cc.Children, predicate, transform);
			}
		}

		public static IEnumerable<INode> Find(IEnumerable<INode> input, Func<INode, bool> predicate)
		{
			foreach (var n in input)
			{
				if (predicate(n))
					yield return n;
				var cc = n as INodeWithChildren;
				if (cc != null)
				{
					foreach (var c in Find(cc.Children, predicate))
						yield return c;
				}
			}
		}

		public static List<WikiLinkInfo> GetAllWikiLinks(string content, IEnumerable<INode> input)
		{
			return NodeUtils.Find(input, e => e.Type == NodeType.Module && ((Module)e).Text.StartsWith("__wikiLink|"))
				.Select(e =>
				{
					var text = ((Module)e).Text;
					var link = text.Substring(11);
					var si = Math.Max(e.CharStart - 20, 0);
					var se = Math.Min(e.CharEnd + 20, content.Length);
					return new WikiLinkInfo
					{
						Link = link,
						Excerpt = content.Substring(si, se - si)
					};
				})
				.ToList();
		}
	}
}
