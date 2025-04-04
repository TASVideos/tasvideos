namespace TASVideos.Data.Entity.Forum;

public class Forum : BaseEntity
{
	public int Id { get; set; }

	public int CategoryId { get; set; }
	public ForumCategory? Category { get; set; }

	public ICollection<ForumTopic> ForumTopics { get; init; } = [];
	public ICollection<ForumPost> ForumPosts { get; init; } = [];

	public string Name { get; set; } = "";

	public string ShortName { get; set; } = "";

	public string? Description { get; set; }

	public int Ordinal { get; set; }

	public bool Restricted { get; set; }

	public bool CanCreateTopics { get; set; } = true;
}

public static class ForumQueryableExtensions
{
	public static IQueryable<Forum> ExcludeRestricted(this IQueryable<Forum> query, bool seeRestricted)
		=> query.Where(f => seeRestricted || !f.Restricted);
}
