namespace TASVideos.Data.Entity.Forum;

public class ForumPollOption : BaseEntity
{
	public int Id { get; set; }

	public string Text { get; set; } = "";

	public int Ordinal { get; set; }

	public int PollId { get; set; }
	public ForumPoll? Poll { get; set; }

	public ICollection<ForumPollOptionVote> Votes { get; init; } = [];
}

public static class ForumPollOptionExtensions
{
	public static IQueryable<ForumPollOption> ForPoll(this IQueryable<ForumPollOption> query, int pollId)
		=> query.Where(o => o.PollId == pollId);
}
