using System.Collections.Generic;

namespace TASVideos.ForumEngine
{
	public interface Node
	{
	}

	public class Text : Node
	{
		public string Content { get; set; }
	}

	public class Element : Node
	{
		public string Name { get; set; }
		public string Options { get; set; } = "";
		public List<Node> Children { get; set; } = new List<Node>();
	}
}
