using System;
using System.Collections.Generic;

namespace TASVideos.Pages.Forum.Models
{
	public class ForumCategoryModel
	{
		public int Id { get; init; }
		public int Ordinal { get; init; }
		public string Title { get; init; } = "";
		public string? Description { get; init; }

		public IEnumerable<Forum> Forums { get; init; } = new List<Forum>();
		public class Forum
		{
			public int Id { get; init; }
			public int Ordinal { get; init; }
			public bool Restricted { get; init; }
			public string Name { get; init; } = "";
			public string? Description { get; init; }

			public Post? LastPost { get; init; } = new ();
			public class Post
			{
				public int Id { get; init; }
				public DateTime CreateTimestamp { get; init; }
				public string? CreateUserName { get; init; } = "";
			}
		}
	}
}
