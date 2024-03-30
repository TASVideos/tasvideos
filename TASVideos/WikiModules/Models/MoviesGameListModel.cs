namespace TASVideos.WikiModules;

public class MoviesGameListModel
{
	public int? SystemId { get; init; }
	public string? SystemCode { get; init; }

	public IReadOnlyCollection<GameEntry> Games { get; init; } = [];
	public class GameEntry
	{
		public int Id { get; init; }
		public string Name { get; init; } = "";
		public IReadOnlyCollection<int> PublicationIds { get; init; } = [];
	}
}
