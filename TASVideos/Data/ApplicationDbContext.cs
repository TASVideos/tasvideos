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

		protected override void OnModelCreating(ModelBuilder builder)
		{
			builder.Entity<RolePermission>()
				.HasKey(rp => new {rp.RoleId, rp.PermissionId});

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

		public DbSet<Publication> Publications { get; set; }
		public DbSet<Permission> Permissions { get; set; }
		public DbSet<RolePermission> RolePermission { get; set; }
	}
}
