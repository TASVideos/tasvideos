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
		public DbSet<Forums> Forums { get; set; }
		public DbSet<Topics> Topics { get; set; }
		public DbSet<Posts> Posts { get; set; }
		public DbSet<PostsText> PostsText { get; set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<Users>().ToTable("users");
			modelBuilder.Entity<Categories>().ToTable("categories");
			modelBuilder.Entity<Forums>().ToTable("forums");
			modelBuilder.Entity<Topics>().ToTable("topics");
			modelBuilder.Entity<Posts>().ToTable("posts");
			modelBuilder.Entity<PostsText>().ToTable("posts_text");
		}
	}
}
