using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

using TASVideos.Data.Entity.Forum;

namespace TASVideos.Pages.Forum.Posts.Models;

public class ForumPostEditModel
{
	public int PosterId { get; set; }
	public string PosterName { get; set; } = "";
	public DateTime CreateTimestamp { get; set; }

	public bool EnableBbCode { get; set; }
	public bool EnableHtml { get; set; }

	public int TopicId { get; set; }

	[DisplayName("Topic Title")]
	[StringLength(500)]
	public string TopicTitle { get; set; } = "";

	[StringLength(500)]
	public string? Subject { get; set; }

	[Required]
	public string Text { get; set; } = "";

	public bool IsFirstPost { get; set; }

	public ForumPostMood Mood { get; set; } = ForumPostMood.Normal;
}
