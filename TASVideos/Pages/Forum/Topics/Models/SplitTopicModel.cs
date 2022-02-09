using System.ComponentModel.DataAnnotations;

namespace TASVideos.Pages.Forum.Topics.Models;

public class SplitTopicModel
{
	[Display(Name = "Split Posts Starting At")]
	public int? PostToSplitId { get; set; }

	[Display(Name = "Create New Topic In")]
	public int SplitToForumId { get; set; }

	[Required]
	[Display(Name = "New Topic Name")]
	public string SplitTopicName { get; set; } = "";

	public string Title { get; set; } = "";

	public int ForumId { get; set; }
	public string ForumName { get; set; } = "";

	public IList<Post> Posts { get; set; } = new List<Post>();

	public class Post
	{
		public int Id { get; set; }
		public DateTime PostCreateTimestamp { get; set; }
		public bool EnableHtml { get; set; }
		public bool EnableBbCode { get; set; }
		public string? Subject { get; set; }
		public string Text { get; set; } = "";
		public int PosterId { get; set; }
		public string PosterName { get; set; } = "";
		public string? PosterAvatar { get; set; }
		public bool Selected { get; set; }
	}
}
