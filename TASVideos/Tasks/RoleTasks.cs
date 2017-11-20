using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Models;

namespace TASVideos.Tasks
{
    public class RoleTasks
    {
		private readonly ApplicationDbContext _db;

		public RoleTasks(ApplicationDbContext db)
		{
			_db = db;
		}

		/// <summary>
		/// Returns all of the <see cref="Role" /> records for the purpose of display
		/// </summary>
		public async Task<IEnumerable<RoleDisplayViewModel>> GetAllRolesForDisplay()
		{
			return await _db.Roles
				.Include(r => r.RolePermission)
				.ThenInclude(rp => rp.Role)
				.OrderBy(r => r.RolePermission.Count)
				.Select(r => new RoleDisplayViewModel
				{
					Id = r.Id,
					Name = r.Name,
					Description = r.Description,
					Permissions = r.RolePermission
						.Select(rp => rp.Permission.Name)
						.OrderBy(name => name)
				})
				.ToListAsync();
		}

		/// <summary>
		/// Returns a <see cref="Role" /> with the given id for the purpose of editing
		/// </summary>
		public async Task<RoleEditViewModel> GetRoleForEdit(int? id)
		{
			using (await _db.Database.BeginTransactionAsync())
			{
				var model = id.HasValue
					? await _db.Roles
						.Select(p => new RoleEditViewModel
						{
							Id = p.Id,
							Name = p.Name,
							Description = p.Description
						})
						.SingleAsync(p => p.Id == id.Value)
					: new RoleEditViewModel();

				model.SelectedPermissions = await _db.RolePermission
					.Where(rp => rp.RoleId == model.Id)
					.Select(rp => (int)rp.PermissionId)
					.ToListAsync();

				model.SelectedAssignablePermissions = await _db.RolePermission
					.Where(rp => rp.RoleId == model.Id)
					.Where(rp => rp.CanAssign)
					.Select(rp => (int)rp.PermissionId)
					.ToListAsync();

				return model;
			}
		}

		/// <summary>
		/// Adds or Updates the given <see cref="Role"/>
		/// If an Id is provided, the Role is updated
		/// If no id is provided, then it is inserted
		/// </summary>
		public async Task AddUpdateRole(RoleEditViewModel model)
		{
			Role role;
			if (model.Id.HasValue)
			{
				role = await _db.Roles.SingleAsync(r => r.Id == model.Id);
				_db.RolePermission.RemoveRange(_db.RolePermission.Where(rp => rp.RoleId == model.Id));
				await _db.SaveChangesAsync();
			}
			else
			{
				role = new Role();
				_db.Roles.Attach(role);
			}

			role.Name = model.Name;
			role.Description = model.Description;

			_db.RolePermission.AddRange(model.SelectedPermissions
				.Select(p => new RolePermission
				{
					RoleId = role.Id,
					PermissionId = (PermissionTo)p,
					CanAssign = model.SelectedAssignablePermissions.Contains(p)
				}));

			await _db.SaveChangesAsync();
		}

		/// <summary>
		/// Removes the <see cref="Role" /> with the given id
		/// </summary>
		public async Task DeleteRole(int id)
		{
			var role = await _db.Roles.SingleAsync(r => r.Id == id);
			_db.Roles.Remove(role);

			await _db.SaveChangesAsync();
		}

		/// <summary>
		/// Returns a list of names for any <see cref="Role"/> that includes every permission in the given list of permissions
		/// </summary>
		public async Task<IEnumerable<string>> RolesThatCanBeAssignedBy(IEnumerable<PermissionTo> permissionIds)
		{
			return await _db.Roles
				.Where(r => r.RolePermission.All(rp => permissionIds.Contains(rp.PermissionId)))
				.Select(r => r.Name)
				.ToListAsync();
		}
	}
}
