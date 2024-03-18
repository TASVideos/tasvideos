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
	public IEnumerable<PermissionTo> Permissions { get; set; } = [];

	[Display(Name = "Related Links")]
	public IEnumerable<string> Links { get; set; } = [];

	[Display(Name = "Users with this Role")]
	public IReadOnlyCollection<UserWithRole> Users { get; set; } = [];

	public class UserWithRole
	{
		public int Id { get; set; }
		public string UserName { get; set; } = "";
	}
}
