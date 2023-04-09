namespace TASVideos.Core.Services.Wiki;

/// <summary>
/// Represents a wiki page revision
/// </summary>
public interface IWikiPage
{
	string PageName { get; }
	string Markup { get; }
	int Revision { get; }
	string? RevisionMessage { get; }
	int? AuthorId { get; }
	string? AuthorName { get; }
	bool IsCurrent();
	DateTime CreateTimestamp { get; }
	bool MinorEdit { get; }
}
