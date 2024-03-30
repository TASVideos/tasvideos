namespace TASVideos.Data.Entity.Forum;

public enum ForumTopicType
{
	Regular = 0,
	Sticky = 1,
	Announcement = 2
}

[ExcludeFromHistory]
public class ForumTopic : BaseEntity
{
	public int Id { get; set; }

	public int ForumId { get; set; }
	public virtual Forum? Forum { get; set; }

	public virtual ICollection<ForumPost> ForumPosts { get; set; } = [];
	public virtual ICollection<ForumTopicWatch> ForumTopicWatches { get; set; } = [];

	[StringLength(500)]
	public string Title { get; set; } = "";

	public int PosterId { get; set; }
	public virtual User? Poster { get; set; }

	public ForumTopicType Type { get; set; }

	public bool IsLocked { get; set; }

	public int? PollId { get; set; }
	public virtual ForumPoll? Poll { get; set; }

	public int? SubmissionId { get; set; }
	public virtual Submission? Submission { get; set; }

	public int? GameId { get; set; }
	public virtual Game.Game? Game { get; set; }
}

public static class ForumTopicQueryableExtensions
{
	public static IQueryable<ForumTopic> ExcludeRestricted(this IQueryable<ForumTopic> query, bool seeRestricted)
	{
		return seeRestricted ? query : query.Where(f => !f.Forum!.Restricted);
	}

	public static IQueryable<ForumTopic> ForForum(this IQueryable<ForumTopic> query, int forumId)
	{
		return query.Where(t => t.ForumId == forumId);
	}

	public static IQueryable<ForumTopic> ForGame(this IQueryable<ForumTopic> query, int gameId)
	{
		return query.Where(t => t.GameId == gameId);
	}
}
