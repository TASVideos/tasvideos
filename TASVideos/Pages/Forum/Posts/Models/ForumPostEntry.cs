using TASVideos.Core;
using TASVideos.Data.Entity.Forum;
using TASVideos.Pages.Forum.Models;

namespace TASVideos.Pages.Forum.Posts.Models;

public class ForumPostEntry : IForumPostEntry
{
	public int Id { get; init; }
	public int TopicId { get; init; }
	public bool Highlight { get; set; }
	public bool Restricted { get; init; }
	public int PosterId { get; init; }
	public string PosterName { get; init; } = "";
	public string? PosterAvatar { get; init; }
	public string? PosterLocation { get; init; }
	public int PosterPostCount { get; init; }
	public double PosterPlayerPoints { get; set; }
	public DateTime PosterJoined { get; init; }
	public string? PosterMoodUrlBase { get; init; }
	public ForumPostMood PosterMood { get; init; }
	public PreferredPronounTypes PosterPronouns { get; init; }
	public IList<string> PosterRoles { get; init; } = [];
	public string? PosterPlayerRank { get; set; }
	public string Text { get; init; } = "";
	public DateTime? PostEditedTimestamp { get; init; }
	public string? Subject { get; init; }
	public string? Signature { get; init; }

	public ICollection<AwardAssignmentSummary> Awards { get; set; } = [];

	public bool EnableHtml { get; init; }
	public bool EnableBbCode { get; init; }

	[Sortable]
	public DateTime CreateTimestamp { get; init; }
	public DateTime LastUpdateTimestamp { get; init; }

	public bool IsLastPost { get; init; }
	public bool IsEditable { get; set; }
	public bool IsDeletable { get; set; }

	public string? GetCurrentAvatar()
	{
		var currentAvatar = PosterAvatar;

		if (PosterMood != ForumPostMood.None && !string.IsNullOrWhiteSpace(PosterMoodUrlBase))
		{
			currentAvatar = PosterMoodUrlBase.Replace("$", ((int)PosterMood).ToString());
		}

		return currentAvatar;
	}
}
