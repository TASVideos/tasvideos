using Microsoft.EntityFrameworkCore;
using TASVideos.Legacy.Data.Site.Entity;

namespace TASVideos.Legacy.Data.Site
{
	public class NesVideosSiteContext : DbContext
	{
		public NesVideosSiteContext(DbContextOptions<NesVideosSiteContext> options) : base(options)
		{
		}

		public DbSet<AwardClass> AwardClasses { get; set; }
		public DbSet<Awards> Awards { get; set; }
		public DbSet<SiteText> SiteText { get; set; }
		public DbSet<User> Users { get; set; }
		// ReSharper disable once UnusedMember.Global
		public DbSet<UserRole> UserRoles { get; set; }
		public DbSet<Role> Roles { get; set; }
		public DbSet<Submission> Submissions { get; set; }
		public DbSet<Movie> Movies { get; set; }
		// ReSharper disable once UnusedMember.Global
		public DbSet<MovieFile> MovieFiles { get; set; }
		public DbSet<MovieFileStorage> MovieFileStorage { get; set; }
		public DbSet<GameName> GameNames { get; set; }
		public DbSet<Rom> Roms { get; set; }
		// ReSharper disable once UnusedMember.Global
		public DbSet<Player> Players { get; set; }
		public DbSet<UserPlayer> UserPlayers { get; set; }
		public DbSet<ClassType> ClassTypes { get; set; }
		public DbSet<MovieClass> MovieClass { get; set; }
		public DbSet<MovieRating> MovieRating { get; set; }
		public DbSet<MovieFlag> MovieFlags { get; set; }
		public DbSet<UserFile> UserFiles { get; set; }
		public DbSet<UserFileComment> UserFileComments { get; set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<AwardClass>().ToTable("awards_classes");
			modelBuilder.Entity<Awards>(entity =>
			{
				entity.HasKey(e => new { e.UserId, e.MovieId, e.AwardId, e.Year });
				entity.ToTable("awards");
			});

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
			modelBuilder.Entity<ClassType>().ToTable("classtype");
			modelBuilder.Entity<MovieClass>(entity =>
			{
				entity.HasKey(e => new { e.MovieId, e.ClassId });
				entity.ToTable("movie_class");
			});
			modelBuilder.Entity<MovieRating>(entity =>
			{
				entity.HasKey(e => new { e.UserId, e.MovieId });
				entity.ToTable("movie_rating");
			});
			modelBuilder.Entity<MovieFlag>(entity =>
			{
				entity.HasKey(e => new { e.MovieId, e.FlagId });
				entity.ToTable("movie_flag");
			});
			modelBuilder.Entity<UserFile>().ToTable("user_files");
			modelBuilder.Entity<UserFileComment>().ToTable("user_files_comments");
		}
	}
}