using System.Collections.Generic;

namespace TASVideos.Services
{
	public class PublicationHistoryGroup
	{
		public int GameId { get; set; }

		public IEnumerable<PublicationHistoryNode> Branches { get; set; } = new List<PublicationHistoryNode>();
	}

	public class PublicationHistoryNode
	{
		public int Id { get; set; }
		public string Title { get; set; } = "";
		public string? Branch { get; set; }
		
		public IEnumerable<PublicationHistoryNode> Obsoletes => ObsoleteList;

		internal int? ObsoletedById { get; set; }

		internal List<PublicationHistoryNode> ObsoleteList { get; set; } = new();
	}
}
