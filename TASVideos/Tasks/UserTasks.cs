using System.Collections.Generic;
using System.Linq;
using TASVideos.Data;
using TASVideos.Data.Entity;

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
			return (from user in _db.Users
					join userRole in _db.UserRoles on user.Id equals userRole.UserId
					join role in _db.Roles on userRole.RoleId equals role.Id
					join rolePermission in _db.RolePermission on role.Id equals rolePermission.RoleId
					join permission in _db.Permissions on rolePermission.PermissionId equals permission.Id
					where user.Id == userId
					select permission.Id)
				.Distinct()
				.ToList();
		}
	}
}
