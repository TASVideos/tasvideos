namespace TASVideos.Data.Entity;

[ExcludeFromHistory]
public class PublicationTag
{
	public int PublicationId { get; set; }
	public Publication? Publication { get; set; }

	public int TagId { get; set; }
	public Tag? Tag { get; set; }
}

public static class PublicationTagExtensions
{
	public static void AddTags(this ICollection<PublicationTag> tags, IEnumerable<int> tagIds)
	{
		tags.AddRange(tagIds.Select(t => new PublicationTag
		{
			TagId = t
		}));
	}
}
