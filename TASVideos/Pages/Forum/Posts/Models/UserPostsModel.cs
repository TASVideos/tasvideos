using TASVideos.Core;

namespace TASVideos.Pages.Forum.Posts.Models;

public class UserPostsModel
{
	public int Id { get; set; }
	public string UserName { get; set; } = "";
	public DateTime Joined { get; set; }
	public string? Location { get; set; }
	public string? Avatar { get; set; }
	public string? Signature { get; set; }
	public double PlayerPoints { get; set; }

	public IList<string> Roles { get; set; } = new List<string>();

	public PageOf<Post> Posts { get; set; } = PageOf<Post>.Empty();

	public class Post
	{
		public int Id { get; set; }

		[Sortable]
		public DateTime CreateTimestamp { get; set; }
		public bool EnableBbCode { get; set; }
		public bool EnableHtml { get; set; }
		public string Text { get; set; } = "";
		public string? Subject { get; set; }
		public int TopicId { get; set; }
		public string TopicTitle { get; set; } = "";
		public int ForumId { get; set; }
		public string ForumName { get; set; } = "";
	}
}
