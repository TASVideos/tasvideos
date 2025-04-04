namespace TASVideos.Data.Entity;

/// <summary>
/// Data storage for an external media post (such as Irc, Discord).
/// </summary>
public class MediaPost : BaseEntity
{
	public int Id { get; set; }

	public string Title { get; set; } = "";

	public string Link { get; set; } = "";

	public string Body { get; set; } = "";

	public string Group { get; set; } = "";

	public string Type { get; set; } = "";

	public string User { get; set; } = "";
}
