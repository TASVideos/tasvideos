namespace TASVideos.Data.Entity.Forum;

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
	public Forum? Forum { get; set; }

	public ICollection<ForumPost> ForumPosts { get; init; } = [];
	public ICollection<ForumTopicWatch> ForumTopicWatches { get; init; } = [];

	public string Title { get; set; } = "";

	public int PosterId { get; set; }
	public User? Poster { get; set; }

	public ForumTopicType Type { get; set; }

	public bool IsLocked { get; set; }

	public int? PollId { get; set; }
	public ForumPoll? Poll { get; set; }

	public int? SubmissionId { get; set; }
	public Submission? Submission { get; set; }

	public int? GameId { get; set; }
	public Game.Game? Game { get; set; }
}

public static class ForumTopicQueryableExtensions
{
	public static IQueryable<ForumTopic> ExcludeRestricted(this IQueryable<ForumTopic> query, bool seeRestricted)
		=> seeRestricted ? query : query.Where(f => !f.Forum!.Restricted);

	public static IQueryable<ForumTopic> ForForum(this IQueryable<ForumTopic> query, int forumId)
		=> query.Where(t => t.ForumId == forumId);

	public static IQueryable<ForumTopic> ForGame(this IQueryable<ForumTopic> query, int gameId)
		=> query.Where(t => t.GameId == gameId);
}
