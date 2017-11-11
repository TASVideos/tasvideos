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
		/// Returns all of the <see cref="TASVideos.Data.Entity.Role" /> records for the purpose of display
		/// </summary>
		public async Task<IEnumerable<RoleDisplayViewModel>> GetAllRolesForDisplayAsync()
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
		/// Returns a <see cref="TASVideos.Data.Entity.Role" /> with the given id for the purpose of editing
		/// </summary>
		public async Task<RoleEditViewModel> GetRoleForEditAsync(int? id)
		{
			var model = id.HasValue
				? await _db.Roles
					.Select(p => new RoleEditViewModel
					{
						Id = p.Id,
						Name = p.Name,
						Description = p.Description,
						SelectedPermisisons = p.RolePermission.Select(rp => rp.PermissionId)
					})
					.SingleAsync(p => p.Id == id.Value)
				: new RoleEditViewModel();

			return model;
		}

		/// <summary>
		/// Adds or Updates the given <seealso cref="Role"/>
		/// If an Id is provided, the Role is updated
		/// If no id is provided, then it is inserted
		/// </summary>
		/// <param name="model"></param>
		public async Task<int> AddUpdateRoleAsync(RoleEditViewModel model)
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

			_db.RolePermission.AddRange(model.SelectedPermisisons
				.Select(p => new RolePermission
				{
					RoleId = role.Id,
					PermissionId = p
				}));

			return await _db.SaveChangesAsync();
		}
	}
}
