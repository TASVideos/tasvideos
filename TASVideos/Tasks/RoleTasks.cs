using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;
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
		public IEnumerable<RoleDisplayViewModel> GetAllRolesForDisplay()
		{
			return _db.Roles
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
				.ToList();
		}

		/// <summary>
		/// Returns a <see cref="TASVideos.Data.Entity.Role" /> with the given id for the purpose of editing
		/// </summary>
		public RoleEditViewModel GetRoleForEdit(int? id)
		{
			using (_db.Database.BeginTransaction())
			{
				var model = id.HasValue
					? _db.Roles
						.Select(p => new RoleEditViewModel
						{
							Id = p.Id,
							Name = p.Name,
							Description = p.Description,
							SelectedPermisisons = p.RolePermission.Select(rp => rp.PermissionId)
						})
						.Single(p => p.Id == id.Value)
					: new RoleEditViewModel();

				model.AvailablePermissions = PermissionsSelectList;

				return model;
			}
		}

		// TODO: document
		public IEnumerable<SelectListItem> PermissionsSelectList =>
			_db.Permissions
				.Select(p => new
				{
					p.Id,
					p.Name
				})
				.ToList()
				.Select(p => new SelectListItem
				{
					Value = ((int) p.Id).ToString(),
					Text = p.Name
				});

		// TODO: documentation
		public void AddUpdateRole(RoleEditViewModel model)
		{
			// TODO
		}
	}
}
