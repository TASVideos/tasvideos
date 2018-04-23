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

		public class ForumPostEntry
		{
			public int Id { get; set; }
			public DateTime PostTime { get; set; }
			public int PosterId { get; set; }
			public string PosterName { get; set; }
			public string PosterAvatar { get; set; }
			public string PosterLocation { get; set; }
			public int PosterPostCount { get; set; }
			public DateTime PosterJoined { get; set; }
			public IEnumerable<string> PosterRoles { get; set; }
			public string Text { get; set; }
			public string Subject { get; set; }
			public string Signature { get; set; }

			public IEnumerable<AwardDisplayModel> Awards { get; set; } = new List<AwardDisplayModel>();

			[Sortable]
			public DateTime CreateTimestamp { get; set; }
		}
	}

	public class ForumPostModel
	{
		public int TopicId { get; set; }
		public string Subject { get; set; }
		public string Post { get; set; }
	}

	public class ForumInboxModel
	{
		public int UserId { get; set; }
		public string UserName { get; set; }
		public IEnumerable<InboxEntry> Inbox { get; set; } = new List<InboxEntry>();

		public class InboxEntry
		{
			public int Id { get; set; }

			[Display(Name = "Subject")]
			public string Subject { get; set; }

			[Display(Name = "From")]
			public string FromUser { get; set; }

			[Display(Name = "Date")]
			public DateTime SendDate { get; set; }

			public bool IsRead { get; set; }
		}
	}

	public class ForumPrivateMessageModel
	{
		public int Id { get; set; }
		public string Subject { get; set; }
		public DateTime SentOn { get; set; }
		public string Text { get; set; }
		public int FromUserId { get; set; }
		public string FromUserName { get; set; }
	}
}
