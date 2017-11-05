using System.Collections.Generic;
using System.Linq;
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

		// TODO: document
		public IEnumerable<PermissionViewModel> GetAllPermissionsForDisplay()
		{
			var permissions = _db.Permissions
				.Include(p => p.RolePermission)
				.Include("RolePermission.Role") // TODO: figure out the lambda version of child of child in EF Core, EF 6 would allow this: .Include(p => p.RolePermission.Select(rp => rp.Role))
				.Select(p => new PermissionViewModel
				{
					Name = p.Name,
					Description = p.Description,
					Group = p.Group,
					Roles = p.RolePermission.Select(rp => rp.Role.Name)
				})
				.OrderBy(p => p.Group)
				.ThenBy(p => p.Name)
				.ToList();

			return permissions;
		}
	}
}
