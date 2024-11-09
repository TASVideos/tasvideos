namespace TASVideos.Data.Entity.Forum;

[ExcludeFromHistory]
public class Forum : BaseEntity
{
	public int Id { get; set; }

	public int CategoryId { get; set; }
	public ForumCategory? Category { get; set; }

	public ICollection<ForumTopic> ForumTopics { get; init; } = [];
	public ICollection<ForumPost> ForumPosts { get; init; } = [];

	[StringLength(50)]
	public string Name { get; set; } = "";

	[StringLength(10)]
	public string ShortName { get; set; } = "";

	[StringLength(1000)]
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
