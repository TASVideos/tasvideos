namespace TASVideos.Data.Entity;

public class UserFileComment
{
	[Key]
	public int Id { get; set; }

	public long UserFileId { get; set; }
	public UserFile? UserFile { get; set; }

	public string? Ip { get; set; }

	public int? ParentId { get; set; }
	public UserFileComment? Parent { get; set; }

	public string Text { get; set; } = "";

	public DateTime CreationTimeStamp { get; set; }

	public int UserId { get; set; }
	public User? User { get; set; }

	public ICollection<UserFileComment> Children { get; init; } = [];
}
