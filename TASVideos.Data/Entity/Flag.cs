namespace TASVideos.Data.Entity;

public class Flag : BaseEntity
{
	public int Id { get; set; }

	[Required]
	[StringLength(32)]
	public string Name { get; set; } = "";

	[StringLength(48)]
	public string? IconPath { get; set; }

	[StringLength(48)]
	public string? LinkPath { get; set; }

	[Required]
	[StringLength(24)]
	public string Token { get; set; } = "";

	public PermissionTo? PermissionRestriction { get; set; }
}
