namespace TASVideos.ViewComponents;

public class MoviesGameListModel
{
	public int? SystemId { get; init; }
	public string? SystemCode { get; init; }

	public ICollection<GameEntry> Games { get; init; } = new List<GameEntry>();
	public class GameEntry
	{
		public int Id { get; init; }
		public string Name { get; init; } = "";
		public ICollection<int> PublicationIds { get; init; } = new List<int>();
	}
}
