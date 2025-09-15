using Microsoft.AspNetCore.Identity;
using TASVideos.Data.AutoHistory;

namespace TASVideos.Data.Entity;

[IncludeInAutoHistory]
public class Role : IdentityRole<int>, ITrackable
{
	public new string Name
	{
		get => base.Name!;
		set => base.Name = value;
	}

	/// <summary>
	/// Gets or sets a value indicating whether the role is automatically assigned to new users.
	/// </summary>
	public bool IsDefault { get; set; }

	public string Description { get; set; } = "";

	/// <summary>
	/// Gets or sets the number of forum posts a user needs
	/// to be automatically assigned this role.
	/// If null, no automatic behavior will occur.
	/// </summary>
	public int? AutoAssignPostCount { get; set; }

	/// <summary>
	/// Gets or sets a value indicating whether the role should be auto-assigned
	/// when an author gets a publication.
	/// </summary>
	public bool AutoAssignPublications { get; set; }

	public DateTime CreateTimestamp { get; set; }

	public DateTime LastUpdateTimestamp { get; set; }

	public ICollection<RolePermission> RolePermission { get; init; } = [];
	public ICollection<UserRole> UserRole { get; init; } = [];
	public ICollection<RoleLink> RoleLinks { get; init; } = [];
}

public static class RoleExtensions
{
	public static IQueryable<Role> ThatCanBeAssignedBy(this IQueryable<Role> query, IEnumerable<PermissionTo> permissions)
		=> query.Where(r => r.RolePermission
				.All(rp => permissions.Contains(rp.PermissionId)));

	public static IQueryable<Role> ThatAreDefault(this IQueryable<Role> query) => query.Where(r => r.IsDefault);
}
