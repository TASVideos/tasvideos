using Microsoft.EntityFrameworkCore;
using TASVideos.Legacy.Data.Site.Entity;

namespace TASVideos.Legacy.Data.Site
{
	public class NesVideosSiteContext : DbContext
	{
		public NesVideosSiteContext(DbContextOptions<NesVideosSiteContext> options)
			: base(options)
		{
		}

		public DbSet<AwardClass> AwardClasses { get; set; } = null!;
		public DbSet<Awards> Awards { get; set; } = null!;
		public DbSet<SiteText> SiteText { get; set; } = null!;
		public DbSet<User> Users { get; set; } = null!;

		// ReSharper disable once UnusedMember.Global
		public DbSet<UserRole> UserRoles { get; set; } = null!;
		public DbSet<Role> Roles { get; set; } = null!;
		public DbSet<Submission> Submissions { get; set; } = null!;
		public DbSet<Movie> Movies { get; set; } = null!;

		// ReSharper disable once UnusedMember.Global
		public DbSet<MovieFile> MovieFiles { get; set; } = null!;
		public DbSet<MovieFileStorage> MovieFileStorage { get; set; } = null!;
		public DbSet<GameName> GameNames { get; set; } = null!;
		public DbSet<GameNameGroup> GameNameGroups { get; set; } = null!;
		public DbSet<GameNameGroupName> GameNameGroupNames { get; set; } = null!;
		public DbSet<Rom> Roms { get; set; } = null!;

		// ReSharper disable once UnusedMember.Global
		public DbSet<Player> Players { get; set; } = null!;
		public DbSet<UserPlayer> UserPlayers { get; set; } = null!;
		public DbSet<ClassType> ClassTypes { get; set; } = null!;
		public DbSet<MovieClass> MovieClass { get; set; } = null!;
		public DbSet<MovieRating> MovieRating { get; set; } = null!;
		public DbSet<MovieFlag> MovieFlags { get; set; } = null!;
		public DbSet<MovieMaintenanceLog> MovieMaintenanceLog { get; set; } = null!;
		public DbSet<UserFile> UserFiles { get; set; } = null!;
		public DbSet<UserFileComment> UserFileComments { get; set; } = null!;
		public DbSet<SubmissionRejections> SubmissionRejections { get; set; } = null!;

		public DbSet<RamAddressDomain> RamAddressDomains { get; set; } = null!;
		public DbSet<RamAddress> RamAddresses { get; set; } = null!;
		public DbSet<RamAddressSet> RamAddressSets { get; set; } = null!;
		public DbSet<UserMaintenanceLogs> UserMaintenanceLogs { get; set; } = null!;

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
			modelBuilder.Entity<Movie>().ToTable("movie").HasOne(e => e.Submission);

			modelBuilder.Entity<MovieFile>(entity =>
			{
				entity.HasOne(e => e.Storage).WithOne(ee => ee!.MovieFile!).IsRequired(false);
				entity.ToTable("movie_file");
			});

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
			modelBuilder.Entity<MovieMaintenanceLog>().ToTable("movie_maintenancelog");
			modelBuilder.Entity<UserFile>().ToTable("user_files");
			modelBuilder.Entity<UserFileComment>().ToTable("user_files_comments");
			modelBuilder.Entity<SubmissionRejections>().ToTable("rejections");

			modelBuilder.Entity<GameNameGroupName>().ToTable("gamename_groupname");
			modelBuilder.Entity<GameNameGroup>(entity =>
			{
				entity.HasKey(e => new { e.GnId, e.GroupId });
				entity.ToTable("gamename_group");
			});

			modelBuilder.Entity<RamAddressDomain>().ToTable("ramaddresses_domains");
			modelBuilder.Entity<RamAddress>().ToTable("ramaddresses");
			modelBuilder.Entity<RamAddressSet>().ToTable("ramaddresses_sets");
			modelBuilder.Entity<UserMaintenanceLogs>().ToTable("user_maintenancelog");
		}
	}
}
