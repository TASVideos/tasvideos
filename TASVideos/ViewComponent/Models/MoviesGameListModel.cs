using System.Collections.Generic;

namespace TASVideos.ViewComponents
{
	public class MoviesGameListModel
	{
		public int? SystemId { get; set; }
		public string? SystemCode { get; set; }

		public ICollection<GameEntry> Games { get; set; } = new List<GameEntry>();
		public class GameEntry
		{
			public int Id { get; set; }
			public string Name { get; set; } = "";
			public ICollection<int> PublicationIds { get; set; } = new List<int>();
		}
	}
}
