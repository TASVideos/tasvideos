using System.Collections.Generic;

namespace TASVideos.Data.Entity.Forum
{
	public class ForumTopic : BaseEntity
	{
		public int Id { get; set; }

		public int ForumId { get; set; }
		public virtual Forum Forum { get; set; }

		public virtual ICollection<ForumPost> ForumPosts { get; set; } = new HashSet<ForumPost>();

		public string Title { get; set; }

		public int PosterId { get; set; }
		public virtual User Poster { get; set; }
	}
}
