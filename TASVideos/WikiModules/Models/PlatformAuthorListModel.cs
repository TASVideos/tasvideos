namespace TASVideos.WikiModules;

public class PlatformAuthorListModel
{
	public bool ShowClasses { get; init; }

	public IEnumerable<PublicationEntry> Publications { get; init; } = [];

	public class PublicationEntry
	{
		public int Id { get; init; }
		public string Title { get; init; } = "";
		public IEnumerable<string> Authors { get; init; } = [];
		public string? ClassIconPath { get; init; } = "";
	}
}
