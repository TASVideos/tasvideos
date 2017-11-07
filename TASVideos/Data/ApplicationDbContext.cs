using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TASVideos.Data.Entity;

namespace TASVideos.Data
{
    public class ApplicationDbContext : IdentityDbContext<User, Role, int>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

		public DbSet<Permission> Permissions { get; set; }
		public DbSet<RolePermission> RolePermission { get; set; }

		public DbSet<Publication> Publications { get; set; }

		public IEnumerable<PermissionTo> GetUserPermissionsById(int userId)
		{
			return (from user in Users
					join userRole in UserRoles on user.Id equals userRole.UserId
					join role in Roles on userRole.RoleId equals role.Id
					join rolePermission in RolePermission on role.Id equals rolePermission.RoleId
					join permission in Permissions on rolePermission.PermissionId equals permission.Id
					where user.Id == userId
					select permission.Id)
				.Distinct()
				.ToList();
		}

		protected override void OnModelCreating(ModelBuilder builder)
		{
			builder.Entity<RolePermission>()
				.HasKey(rp => new { rp.RoleId, rp.PermissionId });

			builder.Entity<RolePermission>()
				.HasOne(pt => pt.Role)
				.WithMany(p => p.RolePermission)
				.HasForeignKey(pt => pt.RoleId);

			builder.Entity<RolePermission>()
				.HasOne(pt => pt.Permission)
				.WithMany(t => t.RolePermission)
				.HasForeignKey(pt => pt.PermissionId);

			base.OnModelCreating(builder);
		}
	}
}
