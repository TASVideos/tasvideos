using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TASVideos.Data;
using TASVideos.Models;

namespace TASVideos.Tasks
{
    public class PermissionTasks
    {
		private readonly ApplicationDbContext _db;

		public PermissionTasks(ApplicationDbContext db)
		{
			_db = db;
		}

		/// <summary>
		/// Returns all of the <see cref="TASVideos.Data.Entity.Permission" /> records for the purpose of display
		/// </summary>
		public IEnumerable<PermissionDisplayViewModel> GetAllPermissionsForDisplay()
		{
			return _db.Permissions
				.Include(p => p.RolePermission)
				.ThenInclude(rp => rp.Role)
				.Select(p => new PermissionDisplayViewModel
				{
					Name = p.Name,
					Description = p.Description,
					Group = p.Group,
					Roles = p.RolePermission.Select(rp => rp.Role.Name)
				})
				.OrderBy(p => p.Group)
				.ThenBy(p => p.Name)
				.ToList();
		}

		/// <summary>
		/// Returns all of the <see cref="TASVideos.Data.Entity.Permission" /> records for the purpose of editing the metadata
		/// </summary>
		public IEnumerable<PermissionEditViewModel> GetAllPermissionsForEdit()
		{
			return _db.Permissions
				.Select(p => new PermissionEditViewModel
				{
					Id = p.Id,
					Name = p.Name,
					Description = p.Description,
					Group = p.Group,
				})
				.OrderBy(p => p.Group)
				.ThenBy(p => p.Name)
				.ToList();
		}

		/// <summary>
		/// Updates all of the given <see cref="TASVideos.Data.Entity.Permission" /> records
		/// </summary>
		public void UpdatePermissionDetails(IEnumerable<PermissionEditViewModel> model)
		{
			if (model == null)
			{
				throw new ArgumentException($"{nameof(model)} can not be null");
			}

			var newPermisions = model.ToList();

			var permissions = _db.Permissions
				.Where(p => newPermisions.Select(np => np.Id).Contains(p.Id))
				.ToList();

			foreach (var permission in permissions)
			{
				var permModel = newPermisions.Single(p => p.Id == permission.Id);

				permission.Name = permModel.Name;
				permission.Description = permModel.Description;
				permission.Group = permModel.Group;
			}

			_db.SaveChanges();
		}
	}
}
