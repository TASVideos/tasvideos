using System.Text.Json.Serialization;
using NpgsqlTypes;

namespace TASVideos.Data.Entity.Forum;

public class ForumPost : BaseEntity
{
	public int Id { get; set; }

	public int? TopicId { get; set; }
	public ForumTopic? Topic { get; set; }

	public int ForumId { get; set; }
	public Forum? Forum { get; set; }

	public int PosterId { get; set; }
	public User? Poster { get; set; }

	public string? IpAddress { get; set; }

	public string? Subject { get; set; }

	public string Text { get; set; } = "";

	public DateTime? PostEditedTimestamp { get; set; }

	public bool EnableHtml { get; set; }
	public bool EnableBbCode { get; set; }

	public ForumPostMood PosterMood { get; set; }

	[JsonIgnore]
	public NpgsqlTsVector SearchVector { get; set; } = null!;
}

public static class ForumPostQueryableExtensions
{
	public static IQueryable<ForumPost> ExcludeRestricted(this IQueryable<ForumPost> query, bool seeRestricted)
		=> query.Where(f => seeRestricted || !f.Topic!.Forum!.Restricted);

	public static IQueryable<ForumPost> ForTopic(this IQueryable<ForumPost> query, int topicId)
		=> query.Where(p => p.TopicId == topicId);

	public static IQueryable<ForumPost> WebSearch(this IQueryable<ForumPost> query, string searchTerms)
		=> query.Where(w => w.SearchVector.Matches(EF.Functions.WebSearchToTsQuery(searchTerms)));

	public static IOrderedQueryable<ForumPost> ByWebRanking(this IQueryable<ForumPost> query, string searchTerms)
		=> query.OrderByDescending(p => p.SearchVector.Rank(EF.Functions.WebSearchToTsQuery(searchTerms)));
}
