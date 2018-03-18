using Microsoft.EntityFrameworkCore;
using TASVideos.Legacy.Data.Forum.Entity;

namespace TASVideos.Legacy.Data.Forum
{
	public class NesVideosForumContext : DbContext
	{
		public NesVideosForumContext(DbContextOptions<NesVideosForumContext> options) : base(options)
		{
		}

		public DbSet<Users> Users { get; set; }

		public DbSet<Categories> Categories { get; set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<Users>().ToTable("users");
			modelBuilder.Entity<Categories>().ToTable("categories");
		}
	}
}
