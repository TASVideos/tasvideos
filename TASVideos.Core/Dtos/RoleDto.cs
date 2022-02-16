namespace TASVideos.Core.Services;

/// <summary>
/// Represents a concise view of a Role
/// </summary>
public class RoleDto
{
	public int Id { get; init; }
	public string? Name { get; init; }
	public string Description { get; init; } = "";
}
