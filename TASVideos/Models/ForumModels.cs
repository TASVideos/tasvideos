using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

using TASVideos.Data;
using TASVideos.Data.Entity.Forum;

namespace TASVideos.Models
{
	public class ForumIndexModel
	{
		public IEnumerable<ForumCategory> Categories { get; set; } = new List<ForumCategory>();
	}

	public class ForumRequest : PagedModel
	{
		public ForumRequest()
		{
			PageSize = 50;
			SortDescending = true;
			SortBy = nameof(ForumModel.ForumTopicEntry.CreateTimestamp);
		}

		public int Id { get; set; }
	}

	public class ForumModel
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public string Description { get; set; }

		public PageOf<ForumTopicEntry> Topics { get; set; }

		public class ForumTopicEntry
		{
			public int Id { get; set; }
			public string Title { get; set; }

			[Display(Name = "Author")]
			public string CreateUserName { get; set; }

			[Sortable]
			public DateTime CreateTimestamp { get; set; }

			public int PostCount { get; set; }
			public int Views { get; set; }

			public ForumTopicType Type { get; set; }

			public DateTime LastPost { get; set; }
		}
	}

	public class TopicRequest : PagedModel
	{
		public TopicRequest()
		{
			PageSize = 25;
			SortDescending = false;
			SortBy = nameof(ForumTopicModel.ForumPostEntry.CreateTimestamp);
		}

		public int Id { get; set; }
	}

	public class ForumTopicModel
	{
		public int Id { get; set; }
		public string Title { get; set; }

		public PageOf<ForumPostEntry> Posts { get; set; }
		public PollModel Poll { get; set; }

		public class ForumPostEntry
		{
			public int Id { get; set; }
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

			public IEnumerable<AwardDisplayModel> Awards { get; set; } = new List<AwardDisplayModel>();

			public bool EnableHtml { get; set; }
			public bool EnableBbCode { get; set; }

			[Sortable]
			public DateTime CreateTimestamp { get; set; }
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

	public class TopicCreateModel : TopicCreatePostModel
	{
		public string ForumName { get; set; }
	}

	public class TopicCreatePostModel
	{
		public int ForumId { get; set; }

		[Required]
		[StringLength(100, MinimumLength = 5)]
		public string Title { get; set; }

		[Required]
		[StringLength(1000, MinimumLength = 5)]
		public string Post { get; set; }

		public ForumTopicType Type { get; set; } = ForumTopicType.Regular;
	}

	/// <summary>
	/// Data necessary to create a post
	/// </summary>
	public class ForumPostModel
	{
		public int TopicId { get; set; }
		public string Subject { get; set; }
		public string Post { get; set; }
	}

	/// <summary>
	/// Data necessary to present to the user for creating a post
	/// as well as the data neccessary to create a post
	/// </summary>
	public class ForumPostCreateModel : ForumPostModel
	{
		public string TopicTitle { get; set; }

		public string UserAvatar { get; set; }
		public string UserSignature { get; set; }
	}

	public class PollResultModel
	{
		public string TopicTitle { get; set; }
		public int TopicId { get; set; }

		public int PollId { get; set; }
		public string Question { get; set; }

		public IEnumerable<VoteResult> Votes { get; set; } = new List<VoteResult>();

		public class VoteResult
		{
			public int UserId { get; set; }

			[Display(Name = "User")]
			public string UserName { get; set; }

			public int Ordinal { get; set; }

			[Display(Name = "Option")]
			public string OptionText { get; set; }

			[Display(Name = "Vote On")]
			public DateTime CreateTimestamp { get; set; }

			[Display(Name = "IP Address")]
			public string IpAddress { get; set; }
		}
	}
}
