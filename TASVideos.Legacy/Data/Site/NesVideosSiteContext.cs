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
		public DbSet<Movie> Movies { get; set; }
		public DbSet<MovieFile> MovieFiles { get; set; }
		public DbSet<MovieFileStorage> MovieFileStorage { get; set; }
		public DbSet<GameName> GameNames { get; set; }
		public DbSet<Rom> Roms { get; set; }
		public DbSet<Player> Players { get; set; }
		public DbSet<UserPlayer> UserPlayers { get; set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<SiteText>().ToTable("site_text");
			modelBuilder.Entity<User>().ToTable("users");
			modelBuilder.Entity<UserRole>(entity =>
			{
				entity.HasKey(e => new { e.UserId, e.RoleId });
				entity.ToTable("user_role");
			});
			modelBuilder.Entity<UserPlayer>(entity =>
			{
				entity.HasKey(e => new { e.UserId, e.PlayerId });
				entity.ToTable("user_player");
			});
			modelBuilder.Entity<Role>().ToTable("roles");
			modelBuilder.Entity<Submission>().ToTable("submission");
			modelBuilder.Entity<Movie>().ToTable("movie");
			modelBuilder.Entity<MovieFile>().ToTable("movie_file");
			modelBuilder.Entity<MovieFileStorage>().ToTable("movie_file_storage");
			modelBuilder.Entity<GameName>().ToTable("gamename");
			modelBuilder.Entity<Rom>().ToTable("roms");
			modelBuilder.Entity<Player>().ToTable("player");

		}
	}
}