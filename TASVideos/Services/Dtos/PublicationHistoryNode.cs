using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
		public string Title { get; set; }
		public string Branch { get; set; }

		public PublicationHistoryNode Obsoletes { get; set; }
	}
}
