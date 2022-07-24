namespace TASVideos.ViewComponents;

public class MovieHistoryModel
{
	public IReadOnlyCollection<MovieHistoryEntry> MovieHistory { get; init; } = new List<MovieHistoryEntry>();

	public class MovieHistoryEntry
	{
		public DateTime Date { get; init; }
		public IReadOnlyCollection<PublicationEntry> Pubs { get; init; } = new List<PublicationEntry>();
	}

	public class PublicationEntry
	{
		public int Id { get; init; }
		public string Name { get; init; } = "";
		public bool IsNewGame { get; init; }
		public bool IsNewBranch { get; init; }
	}
}
