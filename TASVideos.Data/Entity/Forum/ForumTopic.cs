using System.Collections.Generic;
using System.Linq;

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
		public virtual ICollection<ForumTopicWatch> ForumTopicWatches { get; set; } = new HashSet<ForumTopicWatch>();

		public string Title { get; set; }

		public int PosterId { get; set; }
		public virtual User Poster { get; set; }

		public int Views { get; set; }
		public ForumTopicType Type { get; set; }

		public bool IsLocked { get; set; }

		public int? PollId { get; set; }
		public virtual ForumPoll Poll { get; set; }

		public string PageName { get; set; }
	}

	public static class ForumTopicQueryableExtensions
	{
		public static IQueryable<ForumTopic> ExcludeRestricted(this IQueryable<ForumTopic> list, bool seeRestricted)
		{
			return list.Where(f => seeRestricted || !f.Forum.Restricted);
		}

		public static IQueryable<ForumTopic> ForForum(this IQueryable<ForumTopic> list, int forumId)
		{
			return list.Where(t => t.ForumId == forumId);
		}
	}
}
