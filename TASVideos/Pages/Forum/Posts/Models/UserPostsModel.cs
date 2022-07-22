using TASVideos.Core;
using TASVideos.Core.Services;
using TASVideos.Data.Entity;
using TASVideos.Data.Entity.Forum;
using TASVideos.Pages.Forum.Models;

namespace TASVideos.Pages.Forum.Posts.Models;

public class UserPagePost : IForumPostEntry
{
	public int Id { get; set; }

	[Sortable]
	public DateTime CreateTimestamp { get; set; }
	public DateTime LastUpdateTimestamp { get; set; }
	public bool EnableBbCode { get; set; }
	public bool EnableHtml { get; set; }
	public string Text { get; set; } = "";
	public string? Subject { get; set; }
	public int TopicId { get; set; }
	public string TopicTitle { get; set; } = "";
	public int ForumId { get; set; }
	public string ForumName { get; set; } = "";
	public ForumPostMood PosterMood { get; set; }

	// Not needed
	public bool Highlight => false;
	public bool IsEditable => false;
	public bool IsDeletable => false;

	// Fill with user info
	public string PosterName { get; set; } = "";
	public string? Signature { get; set; }
	public int PosterPostCount { get; set; }
	public string? PosterLocation { get; set; }
	public DateTime PosterJoined { get; set; }
	public double PosterPlayerPoints { get; set; }
	public string? PosterAvatar { get; set; }
	public string? PosterMoodUrlBase { get; set; }
	public IList<string> PosterRoles { get; set; } = new List<string>();
	public PreferredPronounTypes PosterPronouns { get; set; }
	public IEnumerable<AwardAssignmentSummary> Awards { get; set; } = new List<AwardAssignmentSummary>();
}
