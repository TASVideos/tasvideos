using TASVideos.Core;
using TASVideos.Core.Services;
using TASVideos.Data.Entity;
using TASVideos.Data.Entity.Forum;

namespace TASVideos.Pages.Forum.Posts.Models;

public class ForumPostEntry
{
	public int Id { get; set; }
	public int TopicId { get; set; }
	public bool Highlight { get; set; }
	public int PosterId { get; set; }
	public string PosterName { get; set; } = "";
	public string? PosterAvatar { get; set; }
	public string? PosterLocation { get; set; }
	public int PosterPostCount { get; set; }
	public double PosterPlayerPoints { get; set; }
	public DateTime PosterJoined { get; set; }
	public string? PosterMoodUrlBase { get; set; }
	public ForumPostMood PosterMood { get; set; }
	public PreferredPronounTypes PosterPronouns { get; set; }
	public IEnumerable<string> PosterRoles { get; set; } = new List<string>();
	public string Text { get; set; } = "";
	public string? Subject { get; set; }
	public string? Signature { get; set; }

	public IEnumerable<AwardAssignmentSummary> Awards { get; set; } = new List<AwardAssignmentSummary>();

	public bool EnableHtml { get; set; }
	public bool EnableBbCode { get; set; }

	[Sortable]
	public DateTime CreateTimestamp { get; set; }
	public DateTime LastUpdateTimestamp { get; set; }

	public bool IsLastPost { get; set; }
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

public class PostsSinceLastVisitModel : ForumPostEntry
{
	public string TopicTitle { get; set; } = "";
	public int ForumId { get; set; }
	public string ForumName { get; set; } = "";
}
