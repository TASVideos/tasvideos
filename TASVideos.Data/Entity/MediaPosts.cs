namespace TASVideos.Data.Entity;

/// <summary>
/// Data storage for an external media post (such as Irc, Discord).
/// </summary>
public class MediaPost : BaseEntity
{
	public int Id { get; set; }

	[Required]
	[StringLength(512)]
	public string Title { get; set; } = "";

	[Required]
	[StringLength(255)]
	public string Link { get; set; } = "";

	[Required]
	[StringLength(1024)]
	public string Body { get; set; } = "";

	[Required]
	[StringLength(255)]
	public string Group { get; set; } = "";

	[Required]
	[StringLength(100)]
	public string Type { get; set; } = "";

	[Required]
	[StringLength(100)]
	public string User { get; set; } = "";
}
