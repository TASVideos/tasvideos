using System.Collections.Generic;

namespace TASVideos.Data.Entity.Forum
{
	public enum ForumTopicType
	{
		Regular = 0,
		Sticky = 1,
		Announcement = 2
	}

	public class ForumTopic : BaseEntity
	{
		public int Id { get; set; }

		public int ForumId { get; set; }
		public virtual Forum Forum { get; set; }

		public virtual ICollection<ForumPost> ForumPosts { get; set; } = new HashSet<ForumPost>();

		public string Title { get; set; }

		public int PosterId { get; set; }
		public virtual User Poster { get; set; }

		public int Views { get; set; }
		public ForumTopicType Type { get; set; }

		public bool IsLocked { get; set; }

		public int? PollId { get; set; }
		public virtual ForumPoll Poll { get; set; }
	}
}
