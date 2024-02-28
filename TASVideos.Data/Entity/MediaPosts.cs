namespace TASVideos.Data.Entity;

/// <summary>
/// Data storage for an external media post (such as Irc, Discord).
/// </summary>
[ExcludeFromHistory]
public class MediaPost : BaseEntity
{
	public int Id { get; set; }

	[StringLength(512)]
	public string Title { get; set; } = "";

	[StringLength(255)]
	public string Link { get; set; } = "";

	[StringLength(1024)]
	public string Body { get; set; } = "";

	[StringLength(255)]
	public string Group { get; set; } = "";

	[StringLength(100)]
	public string Type { get; set; } = "";

	[StringLength(100)]
	public string User { get; set; } = "";
}
