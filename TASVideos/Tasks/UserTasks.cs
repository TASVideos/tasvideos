using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Models;

namespace TASVideos.Tasks
{
	public class UserTasks
	{
		private readonly ApplicationDbContext _db;

		public UserTasks(ApplicationDbContext db)
		{
			_db = db;
		}

		/// <summary>
		/// Returns a list of all permissions of the <seea cref="User"/> with the given id
		/// </summary>
		public IEnumerable<PermissionTo> GetUserPermissionsById(int userId)
		{
			return GetUserPermissionByIdQuery(userId)
				.ToList();
		}

		/// <summary>
		/// Returns a list of all permissions of the <seea cref="User"/> with the given id
		/// </summary>
		public async Task<IEnumerable<PermissionTo>> GetUserPermissionsByIdAsync(int userId)
		{
			return await GetUserPermissionByIdQuery(userId)
				.ToListAsync();
		}

		private IQueryable<PermissionTo> GetUserPermissionByIdQuery(int userId)
		{
			return _db.Users
				.Include(u => u.UserRoles)
				.ThenInclude(u => u.Role)
				.ThenInclude(r => r.RolePermission)
				.ThenInclude(rp => rp.Permission)
				.Where(u => u.Id == userId)
				.SelectMany(u => u.UserRoles)
				.SelectMany(ur => ur.Role.RolePermission)
				.Select(rp => rp.PermissionId)
				.Distinct();
		}

		public async Task<IEnumerable<RoleBasicDisplay>> GetUserRoles(int userId)
		{
			return await _db.Users
				.Include(u => u.UserRoles)
				.ThenInclude(ur => ur.Role)
				.Where(u => u.Id == userId)
				.SelectMany(u => u.UserRoles)
				.Select(ur => ur.Role)
				.Select(r => new RoleBasicDisplay
				{
					Id = r.Id,
					Name = r.Name,
					Description = r.Description
				})
				.ToListAsync();
		}
	}
}
