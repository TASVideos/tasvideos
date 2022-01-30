namespace TASVideos.Data.Entity;

public class UserDisallow : BaseEntity
{
	public int Id { get; set; }

	[Required]
	[StringLength(100)]
	public string RegexPattern { get; set; } = "";
}
