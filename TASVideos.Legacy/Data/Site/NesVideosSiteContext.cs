using Microsoft.EntityFrameworkCore;
using TASVideos.Legacy.Data.Site.Entity;

namespace TASVideos.Legacy.Data.Site
{
	public class NesVideosSiteContext : DbContext
	{
		public NesVideosSiteContext(DbContextOptions<NesVideosSiteContext> options) : base(options)
		{
		}

		public DbSet<SiteText> SiteText { get; set; }
		public DbSet<User> Users { get; set; }
		public DbSet<UserRole> UserRoles { get; set; }
		public DbSet<Role> Roles { get; set; }
		public DbSet<Submission> Submissions { get; set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<SiteText>().ToTable("site_text");
			modelBuilder.Entity<User>().ToTable("users");
			modelBuilder.Entity<UserRole>(entity =>
			{
				entity.HasKey(e => new { e.UserId, e.RoleId });
				entity.ToTable("user_role");
			});
			modelBuilder.Entity<Role>().ToTable("roles");
			modelBuilder.Entity<Submission>().ToTable("submission");
		}
	}
}