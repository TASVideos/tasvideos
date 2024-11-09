namespace TASVideos.Data.Entity.Forum;

[ExcludeFromHistory]
public class ForumCategory : BaseEntity
{
	public int Id { get; set; }
	public ICollection<Forum> Forums { get; set; } = [];

	[StringLength(30)]
	public string Title { get; set; } = "";

	public int Ordinal { get; set; }

	[StringLength(1000)]
	public string? Description { get; set; }
}
