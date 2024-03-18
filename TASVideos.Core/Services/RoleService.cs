﻿
namespace TASVideos.Core.Services;

public interface IRoleService
{
	Task<IEnumerable<AssignableRole>> GetAllRolesUserCanAssign(int userId, IEnumerable<int> assignedRoles);
	Task RemoveRolesFromUser(int userId);
	Task<bool> IsInUse(int roleId);
	Task<IReadOnlyCollection<string>> GetRolesThatCanBeAssignedBy(IEnumerable<PermissionTo> permissionIds);
}

public record AssignableRole(int Id, string Name, bool Disabled);

internal class RoleService(ApplicationDbContext db) : IRoleService
{
	public async Task<IEnumerable<AssignableRole>> GetAllRolesUserCanAssign(int userId, IEnumerable<int> assignedRoles)
	{
		if (assignedRoles is null)
		{
			throw new ArgumentException($"{nameof(assignedRoles)} can not be null");
		}

		var assignedRoleList = assignedRoles.ToList();
		var assignablePermissions = await db.Users
			.Where(u => u.Id == userId)
			.SelectMany(u => u.UserRoles)
			.SelectMany(ur => ur.Role!.RolePermission)
			.Where(rp => rp.CanAssign)
			.Select(rp => rp.PermissionId)
			.ToListAsync();

		var roles = await db.Roles
			.Where(r => r.RolePermission.All(rp => assignablePermissions.Contains(rp.PermissionId))
				|| assignedRoleList.Contains(r.Id))
			.Select(r => new {
				r.Id,
				r.Name,
				Diabled = !r.RolePermission.All(rp => assignablePermissions.Contains(rp.PermissionId))
					&& assignedRoleList.Any() // EF Core 2.1 issue, needs this or a user with no assigned roles blows up
					&& assignedRoleList.Contains(r.Id)
				})
			.OrderBy(s => s.Name)
			.ToListAsync();

		return roles.Select(r => new AssignableRole(r.Id, r.Name, r.Diabled));
	}

	public async Task RemoveRolesFromUser(int userId)
	{
		var userRoles = await db.UserRoles
			.Where(ur => ur.UserId == userId)
			.ToListAsync();

		try
		{
			db.RemoveRange(userRoles);
			await db.SaveChangesAsync();
		}
		catch (DbUpdateConcurrencyException)
		{
			// Eat it for now
		}
	}

	public async Task<bool> IsInUse(int roleId)
	{
		return await db.Users.AnyAsync(u => u.UserRoles.Any(ur => ur.RoleId == roleId));
	}

	public async Task<IReadOnlyCollection<string>> GetRolesThatCanBeAssignedBy(IEnumerable<PermissionTo> permissionIds)
	{
		return await db.Roles
			.ThatCanBeAssignedBy(permissionIds)
			.Select(r => r.Name)
			.ToListAsync();
	}
}
