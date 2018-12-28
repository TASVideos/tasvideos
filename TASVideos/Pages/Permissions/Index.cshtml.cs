using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Extensions;
using TASVideos.Pages.Permissions.Models;
using TASVideos.Tasks;

namespace TASVideos.Pages.Permissions
{
	[Authorize]
	public class IndexModel : BasePageModel
	{
		private readonly ApplicationDbContext _db;

		private static readonly List<PermissionDisplayModel> PermissionData = Enum
			.GetValues(typeof(PermissionTo))
			.Cast<PermissionTo>()
			.Select(p => new PermissionDisplayModel
			{
				Id = p,
				Name = p.EnumDisplayName(),
				Group = p.Group(),
				Description = p.Description()
			})
			.ToList();

		public IndexModel(ApplicationDbContext db, UserTasks userTasks)
			: base(userTasks)
		{
			_db = db;
		}

		public IEnumerable<PermissionDisplayModel> Permissions => PermissionData;

		public async Task OnGet()
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
		}
	}
}
