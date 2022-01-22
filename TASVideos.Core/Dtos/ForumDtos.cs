using System;
using System.Collections.Generic;
using TASVideos.Data.Entity.Forum;

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

	public record PostPositionDto(int Page, int TopicId);

	public record PollCreateDto(string? Question, int? DaysOpen, bool MultiSelect, IEnumerable<string> Options);
	public record PostCreateDto(
		int ForumId,
		int TopicId,
		string? Subject,
		string Text,
		int PosterId,
		string PosterName,
		ForumPostMood Mood,
		string IpAddress,
		bool WatchTopic);
}
