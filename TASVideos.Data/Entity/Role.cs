using Microsoft.AspNetCore.Identity;

namespace TASVideos.Data.Entity;

public class Role : IdentityRole<int>, ITrackable
{
	/// <summary>
	/// Gets or sets a value indicating whether or not the role is automatically assigned to new users.
	/// </summary>
	public bool IsDefault { get; set; }

	[Required]
	[StringLength(300)]
	public string Description { get; set; } = "";

	/// <summary>
	/// Gets or sets the number of forum posts a user needs
	/// to be automatically assigned this role.
	/// If null, no automatic behavior will occur.
	/// </summary>
	public int? AutoAssignPostCount { get; set; }

	/// <summary>
	/// Gets or sets a value indicating whether or not the role should be auto-assigned
	/// when an author gets a publication.
	/// </summary>
	public bool AutoAssignPublications { get; set; }

	public DateTime CreateTimestamp { get; set; }
	public string? CreateUserName { get; set; }

	public DateTime LastUpdateTimestamp { get; set; }
	public string? LastUpdateUserName { get; set; }

	public virtual ICollection<RolePermission> RolePermission { get; set; } = new HashSet<RolePermission>();
	public virtual ICollection<UserRole> UserRole { get; set; } = new HashSet<UserRole>();
	public virtual ICollection<RoleLink> RoleLinks { get; set; } = new HashSet<RoleLink>();
}

public static class RoleExtensions
{
	public static IQueryable<Role> ThatCanBeAssignedBy(this IQueryable<Role> query, IEnumerable<PermissionTo> permissions)
	{
		return query
			.Where(r => r.RolePermission
				.All(rp => permissions.Contains(rp.PermissionId)));
	}

	public static IQueryable<Role> ThatAreDefault(this IQueryable<Role> query)
	{
		return query.Where(r => r.IsDefault);
	}
}
