namespace TASVideos.ViewComponents;

public class WikiTextChangelogModel
{
	public DateTime CreateTimestamp { get; init; }
	public string? Author { get; init; }
	public string PageName { get; init; } = "";
	public int Revision { get; init; }
	public bool MinorEdit { get; init; }
	public string? RevisionMessage { get; init; }
}
