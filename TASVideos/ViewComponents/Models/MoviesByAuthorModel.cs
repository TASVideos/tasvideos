namespace TASVideos.ViewComponents.Models;

public class MoviesByAuthorModel
{
	public bool MarkNewbies { get; set; }
	public bool ShowClasses { get; set; }

	public ICollection<string> NewbieAuthors { get; set; } = new List<string>();

	public ICollection<PublicationEntry> Publications { get; set; } = new List<PublicationEntry>();

	public class PublicationEntry
	{
		public int Id { get; set; }
		public string Title { get; set; } = "";
		public IEnumerable<string> Authors { get; set; } = new List<string>();
		public string? PublicationClassIconPath { get; set; } = "";
	}
}
