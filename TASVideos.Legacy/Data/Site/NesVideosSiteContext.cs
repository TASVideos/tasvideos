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

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<SiteText>().ToTable("site_text");
			modelBuilder.Entity<User>().ToTable("users");
		}
	}
}