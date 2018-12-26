using System.Linq;

namespace TASVideos.Data.Entity.Forum
{
	public class ForumTopicWatch
	{
		public int UserId { get; set; }
		public virtual User User { get; set; }

		public int ForumTopicId { get; set; }
		public virtual ForumTopic ForumTopic { get; set; }

		public bool IsNotified { get; set; }
	}

	public static class ForumTopicWatchQueryableExtensions
	{
		public static IQueryable<ForumTopicWatch> ExcludeRestricted(this IQueryable<ForumTopicWatch> list, bool seeRestricted)
		{
			return list.Where(f => seeRestricted || !f.ForumTopic.Forum.Restricted);
		}

		public static IQueryable<ForumTopicWatch> ForUser(this IQueryable<ForumTopicWatch> query, int userId)
		{
			return query.Where(pr => pr.UserId == userId);
		}
	}
}
