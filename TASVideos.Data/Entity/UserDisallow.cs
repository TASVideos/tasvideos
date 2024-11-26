namespace TASVideos.Data.Entity;

[ExcludeFromHistory]
public class UserDisallow : BaseEntity
{
	public int Id { get; set; }

	public string RegexPattern { get; set; } = "";
}
