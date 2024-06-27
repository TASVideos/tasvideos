namespace TASVideos.Data.Entity;

public class Flag
{
	public int Id { get; set; }

	[StringLength(32)]
	public string Name { get; set; } = "";

	[StringLength(48)]
	public string? IconPath { get; set; }

	[StringLength(48)]
	public string? LinkPath { get; set; }

	[StringLength(24)]
	public string Token { get; set; } = "";

	public PermissionTo? PermissionRestriction { get; set; }

	public double Weight { get; set; }
}
