namespace TASVideos.ViewComponents;

public class PlatformAuthorListModel
{
	public bool ShowClasses { get; init; }

	public IEnumerable<PublicationEntry> Publications { get; init; } = new List<PublicationEntry>();

	public class PublicationEntry
	{
		public int Id { get; init; }
		public string Title { get; init; } = "";
		public IEnumerable<string> Authors { get; init; } = new List<string>();
		public string? ClassIconPath { get; init; } = "";
	}
}
