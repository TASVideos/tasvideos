using System;
using System.Collections.Generic;
using System.Linq;

using TASVideos.Data;
using TASVideos.Services;

namespace TASVideos.Pages.Forum.Topics.Models
{
	public interface IForumTopicActionBar
	{
		int Id { get; }
		bool IsLocked { get; }
		bool IsWatching { get; }
		string Title { get; }
		bool AnyVotes { get; }
	}

	public interface IForumTopicBreadCrumb
	{
		int Id { get; }
		string Title { get; }
		bool IsLocked { get; }
		int ForumId { get; }
		string ForumName { get; }
	}

	public class ForumTopicModel : IForumTopicActionBar, IForumTopicBreadCrumb
	{
		public int Id { get; set; }
		public bool IsWatching { get; set; }
		public bool IsLocked { get; set; }
		public string Title { get; set; }
		public int ForumId { get; set; }
		public string ForumName { get; set; }

		public bool AnyVotes => Poll?.Options.SelectMany(o => o.Voters).Any() ?? false;

		public PageOf<ForumPostEntry> Posts { get; set; }
		public PollModel Poll { get; set; }

		public class ForumPostEntry
		{
			public int Id { get; set; }
			public int TopicId { get; set; }
			public bool Highlight { get; set; }
			public int PosterId { get; set; }
			public string PosterName { get; set; }
			public string PosterAvatar { get; set; }
			public string PosterLocation { get; set; }
			public int PosterPostCount { get; set; }
			public DateTime PosterJoined { get; set; }
			public IEnumerable<string> PosterRoles { get; set; }
			public string Text { get; set; }
			public string RenderedText { get; set; }
			public string Subject { get; set; }
			public string Signature { get; set; }
			public string RenderedSignature { get; set; }

			public IEnumerable<AwardEntryDto> Awards { get; set; } = new List<AwardEntryDto>();

			public bool EnableHtml { get; set; }
			public bool EnableBbCode { get; set; }

			[Sortable]
			public DateTime CreateTimestamp { get; set; }

			public bool IsLastPost { get; set; }
			public bool IsEditable { get; set; }
			public bool IsDeletable { get; set; }
		}

		public class PollModel
		{
			public int PollId { get; set; }
			public string Question { get; set; }

			public IEnumerable<PollOptionModel> Options { get; set; } = new List<PollOptionModel>();

			public class PollOptionModel
			{
				public string Text { get; set; }
				public int Ordinal { get; set; }
				public ICollection<int> Voters { get; set; } = new List<int>();
			}
		}
	}

}
