using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TASVideos.Pages.Forum.Topics.Models
{
	public class SplitTopicModel
	{
		[Required]
		[Display(Name = "Split On Post")]
		public int? PostToSplitId { get; set; }

		[Display(Name = "Create New Topic In")]
		public int SplitToForumId { get; set; }

		[Required]
		[Display(Name = "New Topic Name")]
		public string SplitTopicName { get; set; }

		public string Title { get; set; }

		public int ForumId { get; set; }
		public string ForumName { get; set; }

		public IEnumerable<Post> Posts { get; set; } = new List<Post>();

		public class Post
		{
			public int Id { get; set; }
			public DateTime PostCreateTimeStamp { get; set; }
			public bool EnableHtml { get; set; }
			public bool EnableBbCode { get; set; }
			public string Subject { get; set; }
			public string Text { get; set; }
			public int PosterId { get; set; }
			public string PosterName { get; set; }
			public string PosterAvatar { get; set; }
		}
	}
}
