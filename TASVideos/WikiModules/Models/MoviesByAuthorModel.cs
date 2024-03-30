namespace TASVideos.WikiModules.Models;

public class MoviesByAuthorModel
{
	public bool MarkNewbies { get; set; }
	public bool ShowClasses { get; set; }

	public IReadOnlyCollection<string> NewbieAuthors { get; set; } = [];

	public IReadOnlyCollection<PublicationEntry> Publications { get; set; } = [];

	public class PublicationEntry
	{
		public int Id { get; set; }
		public string Title { get; set; } = "";
		public IEnumerable<string> Authors { get; set; } = [];
		public string? PublicationClassIconPath { get; set; } = "";
	}
}
