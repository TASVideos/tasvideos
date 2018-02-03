using Microsoft.EntityFrameworkCore;
using TASVideos.Legacy.Data.Entity;

namespace TASVideos.Legacy.Data
{
	public class NesVideosSiteContext : DbContext
	{
		public NesVideosSiteContext(DbContextOptions<NesVideosSiteContext> options) : base(options)
		{
		}

		public DbSet<SiteText> SiteText { get; set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<SiteText>().ToTable("site_text");
		}
	}
}