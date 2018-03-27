using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Extensions;
using TASVideos.Models;

namespace TASVideos.Tasks
{
    public class PermissionTasks
	{
		private static readonly List<PermissionDisplayViewModel> PermissionData = Enum.GetValues(typeof(PermissionTo))
			.Cast<PermissionTo>()
			.Select(p => new PermissionDisplayViewModel
			{
				Id = p,
				Name = p.EnumDisplayName(),
				Group = p.Group(),
				Description = p.Description()
			})
			.ToList();

		private readonly ApplicationDbContext _db;

		public PermissionTasks(ApplicationDbContext db)
		{
			_db = db;
		}

		/// <summary>
		/// Returns a list of all permissions, descriptions and groupings for the purpose of display
		/// </summary>
		public async Task<IEnumerable<PermissionDisplayViewModel>> GetAllPermissionsForDisplay()
		{
			using (_db.Database.BeginTransactionAsync())
			{
				var allRoles = await _db.Roles
					.Select(r => new
					{
						r.Name,
						RolePermissionId = r.RolePermission
							.Select(p => p.PermissionId)
							.ToList()
					})
					.ToListAsync();

				foreach (var permission in PermissionData)
				{
					permission.Roles = allRoles
						.Where(r => r.RolePermissionId.Any(p => p == permission.Id))
						.Select(r => r.Name);
				}

				return PermissionData;
			}
		}
	}
}
