using System;
using System.ComponentModel.DataAnnotations;

namespace TASVideos.Pages.Forum.Posts.Models
{
	public class UnansweredPostsModel
	{
		public int ForumId { get; set; }

		[Display(Name = "Forum")]
		public string ForumName { get; set; }

		public int TopicId { get; set; }

		[Display(Name = "Topic")]
		public string TopicName { get; set; }

		public int AuthorId { get; set; }

		[Display(Name = "Author")]
		public string AuthorName { get; set; }

		[Display(Name = "Posted On")]
		public DateTime PostDate { get; set; }
	}
}
