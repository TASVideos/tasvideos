using System;
using System.Collections.Generic;
using TASVideos.Data;

namespace TASVideos.Pages.Forum.Posts.Models
{
	public class UserPostsModel
	{
		public int Id { get; set; }
		public string UserName { get; set; }
		public DateTime Joined { get; set; }
		public string Location { get; set; }
		public string Avatar { get; set; }
		public string Signature { get; set; }
		public string RenderedSignature { get; set; }

		public IEnumerable<string> Roles { get; set; } = new List<string>();

		public PageOf<Post> Posts { get; set; }

		public class Post
		{
			public int Id { get; set; }

			[Sortable]
			public DateTime CreateTimestamp { get; set; }
			public bool EnableBbCode { get; set; }
			public bool EnableHtml { get; set; }
			public string Text { get; set; }
			public string RenderedText { get; set; }
			public string Subject { get; set; }
			public int TopicId { get; set; }
			public string TopicTitle { get; set; }
			public int ForumId { get; set; }
			public string ForumName { get; set; }
		}
	}
}
