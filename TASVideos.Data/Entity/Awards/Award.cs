namespace TASVideos.Data.Entity.Awards;

public enum AwardType
{
	User = 1,
	Movie
}

[ExcludeFromHistory]
public class Award
{
	public int Id { get; set; }
	public AwardType Type { get; set; }

	[StringLength(25)]
	public string ShortName { get; set; } = "";

	[StringLength(50)]
	public string Description { get; set; } = "";
}
