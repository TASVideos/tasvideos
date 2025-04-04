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
	public static IQueryable<ForumTopicWatch> ExcludeRestricted(this IQueryable<ForumTopicWatch> query, bool seeRestricted)
		=> query.Where(f => seeRestricted || !f.ForumTopic!.Forum!.Restricted);

	public static IQueryable<ForumTopicWatch> ForUser(this IQueryable<ForumTopicWatch> query, int userId)
		=> query.Where(pr => pr.UserId == userId);
}
