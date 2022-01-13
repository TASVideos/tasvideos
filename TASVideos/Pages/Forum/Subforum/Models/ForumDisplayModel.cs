using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using TASVideos.Core;
using TASVideos.Data.Entity.Forum;

namespace TASVideos.Pages.Forum.Subforum.Models
{
	public class ForumDisplayModel
	{
		public int Id { get; set; }
		public string Name { get; set; } = "";
		public string? Description { get; set; }

		public PageOf<ForumTopicEntry> Topics { get; set; } = PageOf<ForumTopicEntry>.Empty();

		public class ForumTopicEntry
		{
			[TableIgnore]
			public int Id { get; set; }
			[Sortable]
			[DisplayName("Topics")]
			public string Title { get; set; } = "";
			[MobileHide]
			[Sortable]
			[DisplayName("Replies")]
			public int PostCount { get; set; }

			[MobileHide]
			[Sortable]
			[DisplayName("Author")]
			public string? CreateUserName { get; set; }

			[TableIgnore]
			[Sortable]
			public DateTime CreateTimestamp { get; set; }

			[TableIgnore]
			[Sortable]
			public ForumTopicType Type { get; set; }

			[TableIgnore]
			public bool IsLocked { get; set; }

			[TableIgnore]
			public ForumPost? LastPost { get; set; }

			[Sortable]
			[DisplayName("Last Post")]
			public DateTime LastPostDateTime { get; set; }

			[TableIgnore]
			public int? LastPostId => LastPost?.Id ?? 0;
			[TableIgnore]
			public string? LastPostUserName => LastPost?.CreateUserName ?? string.Empty;
		}
	}
}
