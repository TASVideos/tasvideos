using Microsoft.AspNetCore.Identity;

namespace TASVideos.Data.Entity;

public class Role : IdentityRole<int>, ITrackable
{
	[StringLength(50)]
	public new string Name
	{
		get => base.Name!;
		set => base.Name = value;
	}

	/// <summary>
	/// Gets or sets a value indicating whether the role is automatically assigned to new users.
	/// </summary>
	public bool IsDefault { get; set; }

	[StringLength(300)]
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

	public virtual ICollection<RolePermission> RolePermission { get; set; } = [];
	public virtual ICollection<UserRole> UserRole { get; set; } = [];
	public virtual ICollection<RoleLink> RoleLinks { get; set; } = [];
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
