using System.Collections.Generic;

namespace TASVideos.ViewComponents
{
	public class MoviesListModel
	{
		public string? SystemCode { get; set; }
		public string? SystemName { get; set; }

		public ICollection<MovieEntry> Movies { get; set; } = new List<MovieEntry>();

		public class MovieEntry
		{
			public int Id { get; set; }
			public bool IsObsolete { get; set; }
			public string GameName { get; set; } = "";
		}
	}
}
