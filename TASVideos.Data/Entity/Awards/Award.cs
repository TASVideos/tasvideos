using Microsoft.EntityFrameworkCore;

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

	[Required]
	[StringLength(25)]
	public string ShortName { get; set; } = "";

	[Required]
	[StringLength(50)]
	public string Description { get; set; } = "";
}
