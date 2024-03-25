using System.ComponentModel.DataAnnotations;

using TASVideos.Data.Entity;

namespace TASVideos.Pages.Roles.Models;

/// <summary>
/// Represents a Role entry for the purpose of display
/// </summary>
public class RoleDisplayModel
{
	public bool IsDefault { get; set; }
	public int Id { get; set; }
	public string? Name { get; set; }
	public string Description { get; set; } = "";

	[Display(Name = "Permissions")]
	public List<PermissionTo> Permissions { get; set; } = [];

	[Display(Name = "Related Links")]
	public List<string> Links { get; set; } = [];

	[Display(Name = "Users with this Role")]
	public List<UserWithRole> Users { get; set; } = [];

	public record UserWithRole(int Id, string UserName);
}
