using System.Linq;

namespace TASVideos.Data.Entity.Forum
{
	public class ForumPost : BaseEntity
	{
		public int Id { get; set; }

		public int? TopicId { get; set; }
		public virtual ForumTopic Topic { get; set; }

		public int PosterId { get; set; }
		public virtual User Poster { get; set; }

		public string IpAddress { get; set; }

		public string Subject { get; set; }
		public string Text { get; set; }

		public bool EnableHtml { get; set; }
		public bool EnableBbCode { get; set; }
	}

	public static class ForumPostQueryableExtensions
	{
		public static IQueryable<ForumPost> ExcludeRestricted(this IQueryable<ForumPost> list, bool seeRestricted)
		{
			return list.Where(f => seeRestricted || !f.Topic.Forum.Restricted);
		}
	}
}
