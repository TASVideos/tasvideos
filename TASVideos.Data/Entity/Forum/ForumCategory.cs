namespace TASVideos.Data.Entity.Forum;

public class ForumCategory : BaseEntity
{
	public int Id { get; set; }
	public ICollection<Forum> Forums { get; init; } = [];

	public string Title { get; set; } = "";

	public int Ordinal { get; set; }

	public string? Description { get; set; }
}
