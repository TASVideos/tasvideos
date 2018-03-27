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
				.Select(r => new RoleDisplayViewModel
				{
					Id = r.Id,
					Name = r.Name,
					Description = r.Description,
					Permissions = r.RolePermission
						.Select(rp => rp.PermissionId).ToList(),
					Links = r.RoleLinks
						.Select(rl => rl.Link).ToList()
				})
				.ToListAsync();
		}

		/// <summary>
		/// Returns a <see cref="Role" /> with the given id for the purpose of editing
		/// </summary>
		public async Task<RoleEditViewModel> GetRoleForEdit(int? id)
		{
			if (!id.HasValue)
			{
				return new RoleEditViewModel();
			}

			// Query all the role data first, then slice it up post query
			// Because EF doesn't like trying to hit the same subtable twice, 
			// plus that is likely less effecient from a query perspective 
			// since the projection is only reducing 2 columns
			var raw = (await _db.Roles
				.Select(r => new
				{
					r.Id,
					r.Name,
					r.Description,
					Links = r.RoleLinks.Select(rl => rl.Link).ToList(),
					r.RolePermission
				})
				.Where(r => r.Id == id.Value) // Workaround for preview 1 bug: https://github.com/aspnet/EntityFrameworkCore/issues/11092
				.ToListAsync()) // When this bug is fixed, remove these and do SingleAsync(r => r.Id == id.Value)
				.SingleOrDefault();

			if (raw == null)
			{
				return null;
			}

			var model = new RoleEditViewModel
			{
				Id = raw.Id,
				Name = raw.Name,
				Description = raw.Description,
				Links = raw.Links,
				SelectedPermissions = raw.RolePermission
					.Select(rp => (int)rp.PermissionId),
				SelectedAssignablePermissions = raw.RolePermission
					.Where(rp => rp.CanAssign)
					.Select(rp => (int)rp.PermissionId)
			};

			return model;
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
				_db.RoleLinks.RemoveRange(_db.RoleLinks.Where(rp => rp.Role.Id == model.Id));
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

			_db.RoleLinks.AddRange(model.Links.Select(rl => new RoleLink
			{
				Link = rl,
				Role = role
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
