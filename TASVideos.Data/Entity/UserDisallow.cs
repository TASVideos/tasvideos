namespace TASVideos.Data.Entity;

[ExcludeFromHistory]
public class UserDisallow : BaseEntity
{
	public int Id { get; set; }

	[StringLength(100)]
	public string RegexPattern { get; set; } = "";
}
