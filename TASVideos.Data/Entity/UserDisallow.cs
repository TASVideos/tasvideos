namespace TASVideos.Data.Entity;

public class UserDisallow : BaseEntity
{
	public int Id { get; set; }

	public string RegexPattern { get; set; } = "";
}
