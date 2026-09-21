namespace TASVideos.Data.Entity.Forum;

[Index(nameof(PostId), nameof(UserId), IsUnique = true)]
public class ForumPostReaction : BaseEntity
{
	public int Id { get; set; }

	public int PostId { get; set; }
	public ForumPost? Post { get; set; }

	public int UserId { get; set; }
	public User? User { get; set; }

	public string Reaction { get; set; } = "";
}
