using TASVideos.Data.AutoHistory;

namespace TASVideos.Data.Entity;

[IncludeInAutoHistory]
public class Flag
{
	public int Id { get; set; }

	public string Name { get; set; } = "";

	public string? IconPath { get; set; }

	public string? LinkPath { get; set; }

	public string Token { get; set; } = "";

	public PermissionTo? PermissionRestriction { get; set; }

	public double Weight { get; set; }
}
