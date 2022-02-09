namespace TASVideos.Core.Services;

/// <summary>
/// Represents a concise view of a Role
/// </summary>
public class RoleDto
{
	public int Id { get; set; }
	public string? Name { get; set; }
	public string Description { get; set; } = "";
}
