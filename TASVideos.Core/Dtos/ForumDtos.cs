using System;
using System.Collections.Generic;

namespace TASVideos.Core.Services
{
	public record LatestPost(int Id, DateTime Timestamp, string PosterName);

	public class ForumCategoryDisplayDto
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
			public LatestPost? LastPost { get; set; }
		}
	}
}
