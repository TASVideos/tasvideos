using TASVideos.Core.Services;
using TASVideos.Data.Entity;
using TASVideos.Data.Entity.Forum;

namespace TASVideos.Pages.Forum.Models;

public interface IForumPostEntry
{
	public int Id { get; }
	public bool Highlight { get; }
	public DateTime CreateTimestamp { get; }
	public DateTime LastUpdateTimestamp { get; }
	public string? Subject { get; }
	public string? Signature { get; }
	public int TopicId { get; }
	public bool IsEditable { get; }
	public bool IsDeletable { get; }
	public string Text { get; }
	public bool EnableHtml { get; }
	public bool EnableBbCode { get; }
	public ForumPostMood PosterMood { get; }

	// Poster
	public string PosterName { get; }
	public int PosterPostCount { get; }
	public string? PosterLocation { get; }
	public DateTime PosterJoined { get; }
	public double PosterPlayerPoints { get; }
	public string? PosterAvatar { get; }
	public string? PosterMoodUrlBase { get; }
	public IList<string> PosterRoles { get; }
	public PreferredPronounTypes PosterPronouns { get; }
	public IEnumerable<AwardAssignmentSummary> Awards { get; }
}
