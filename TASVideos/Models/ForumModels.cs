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
			[Sortable]
			public int Id { get; set; }
			public string Title { get; set; }

			[Display(Name = "Author")]
			public string CreateUserName { get; set; }
			public DateTime CreateTimestamp { get; set; }

			public int PostCount { get; set; }
		}
	}
}
