namespace TASVideos.ViewComponents;

public class MoviesListModel
{
	public string? SystemCode { get; init; }
	public string? SystemName { get; init; }

	public IReadOnlyCollection<MovieEntry> Movies { get; init; } = new List<MovieEntry>();

	public class MovieEntry
	{
		public int Id { get; init; }
		public bool IsObsolete { get; init; }
		public string GameName { get; init; } = "";
	}
}
