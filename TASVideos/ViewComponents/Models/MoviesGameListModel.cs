namespace TASVideos.ViewComponents;

public class MoviesGameListModel
{
	public int? SystemId { get; init; }
	public string? SystemCode { get; init; }

	public IReadOnlyCollection<GameEntry> Games { get; init; } = new List<GameEntry>();
	public class GameEntry
	{
		public int Id { get; init; }
		public string Name { get; init; } = "";
		public IReadOnlyCollection<int> PublicationIds { get; init; } = new List<int>();
	}
}
