namespace TASVideos.Data.Entity.Forum;

public class ForumTopicWatch
{
	public int UserId { get; set; }
	public User? User { get; set; }

	public int ForumTopicId { get; set; }
	public ForumTopic? ForumTopic { get; set; }

	public bool IsNotified { get; set; }
}

public static class ForumTopicWatchQueryableExtensions
{
	extension(IQueryable<ForumTopicWatch> query)
	{
		public IQueryable<ForumTopicWatch> ExcludeRestricted(bool seeRestricted)
			=> query.Where(f => seeRestricted || !f.ForumTopic!.Forum!.Restricted);

		public IQueryable<ForumTopicWatch> ForUser(int userId) => query.Where(pr => pr.UserId == userId);
	}
}
