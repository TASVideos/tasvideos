using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using TASVideos.Data.Entity;
using TASVideos.Data.Entity.Awards;
using TASVideos.Data.Entity.Forum;
using TASVideos.Data.Entity.Game;

namespace TASVideos.Data;

public class ApplicationDbContext : IdentityDbContext<User, Role, int, UserClaim, UserRole, UserLogin, RoleClaim, UserToken>
{
	private readonly IHttpContextAccessor? _httpContext;

	internal const string SystemUser = "admin@tasvideos.org";

	public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
		: base(options)
	{
	}

	public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IHttpContextAccessor? httpContextAccessor)
		: base(options)
	{
		_httpContext = httpContextAccessor;
	}

	public DbSet<RolePermission> RolePermission { get; set; } = null!;
	public DbSet<WikiPage> WikiPages { get; set; } = null!;
	public DbSet<WikiPageReferral> WikiReferrals { get; set; } = null!;
	public DbSet<RoleLink> RoleLinks { get; set; } = null!;
	public DbSet<Submission> Submissions { get; set; } = null!;
	public DbSet<SubmissionAuthor> SubmissionAuthors { get; set; } = null!;
	public DbSet<SubmissionStatusHistory> SubmissionStatusHistory { get; set; } = null!;
	public DbSet<SubmissionRejectionReason> SubmissionRejectionReasons { get; set; } = null!;

	public DbSet<PublicationClass> PublicationClasses { get; set; } = null!;

	public DbSet<Publication> Publications { get; set; } = null!;
	public DbSet<PublicationAuthor> PublicationAuthors { get; set; } = null!;
	public DbSet<PublicationFile> PublicationFiles { get; set; } = null!;
	public DbSet<PublicationTag> PublicationTags { get; set; } = null!;
	public DbSet<PublicationRating> PublicationRatings { get; set; } = null!;
	public DbSet<PublicationFlag> PublicationFlags { get; set; } = null!;
	public DbSet<PublicationUrl> PublicationUrls { get; set; } = null!;
	public DbSet<PublicationMaintenanceLog> PublicationMaintenanceLogs { get; set; } = null!;

	public DbSet<Tag> Tags { get; set; } = null!;
	public DbSet<Flag> Flags { get; set; } = null!;

	public DbSet<Award> Awards { get; set; } = null!;
	public DbSet<PublicationAward> PublicationAwards { get; set; } = null!;
	public DbSet<UserAward> UserAwards { get; set; } = null!;

	// Game tables
	public DbSet<Game> Games { get; set; } = null!;
	public DbSet<GameGenre> GameGenres { get; set; } = null!;
	public DbSet<Genre> Genres { get; set; } = null!;
	public DbSet<GameSystem> GameSystems { get; set; } = null!;
	public DbSet<GameSystemFrameRate> GameSystemFrameRates { get; set; } = null!;
	public DbSet<GameRom> GameRoms { get; set; } = null!;
	public DbSet<GameGroup> GameGroups { get; set; } = null!;
	public DbSet<GameGameGroup> GameGameGroups { get; set; } = null!;
	public DbSet<GameRamAddressDomain> GameRamAddressDomains { get; set; } = null!;
	public DbSet<GameRamAddress> GameRamAddresses { get; set; } = null!;

	// Forum tables
	public DbSet<ForumCategory> ForumCategories { get; set; } = null!;
	public DbSet<Forum> Forums { get; set; } = null!;
	public DbSet<ForumTopic> ForumTopics { get; set; } = null!;
	public DbSet<ForumPost> ForumPosts { get; set; } = null!;
	public DbSet<ForumPoll> ForumPolls { get; set; } = null!;
	public DbSet<ForumPollOption> ForumPollOptions { get; set; } = null!;
	public DbSet<ForumPollOptionVote> ForumPollOptionVotes { get; set; } = null!;
	public DbSet<ForumTopicWatch> ForumTopicWatches { get; set; } = null!;

	public DbSet<PrivateMessage> PrivateMessages { get; set; } = null!;

	// Userfiles
	public DbSet<UserFile> UserFiles { get; set; } = null!;
	public DbSet<UserFileComment> UserFileComments { get; set; } = null!;
	public DbSet<UserDisallow> UserDisallows { get; set; } = null!;

	public DbSet<MediaPost> MediaPosts { get; set; } = null!;

	public DbSet<IpBan> IpBans { get; set; } = null!;

	public DbSet<UserMaintenanceLog> UserMaintenanceLogs { get; set; } = null!;

	public DbSet<DeprecatedMovieFormat> DeprecatedMovieFormats { get; set; } = null!;

	public override int SaveChanges(bool acceptAllChangesOnSuccess)
	{
		PerformTrackingUpdates();

		ChangeTracker.AutoDetectChangesEnabled = false;
		var result = base.SaveChanges(acceptAllChangesOnSuccess);
		ChangeTracker.AutoDetectChangesEnabled = true;

		return result;
	}

	public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
	{
		PerformTrackingUpdates();
		return base.SaveChangesAsync(cancellationToken);
	}

	/// <summary>
	/// Attempts to save changes, but if a <see cref="DbUpdateConcurrencyException"/> occurs,
	/// it will be caught no changes will be saved.  Only to be used if discarding the data is
	/// an acceptable handling.
	/// </summary>
	public async Task<int> TrySaveChangesAsync(CancellationToken cancellationToken = default)
	{
		try
		{
			return await SaveChangesAsync(cancellationToken);
		}
		catch (DbUpdateConcurrencyException)
		{
			return 0;
		}
	}

	protected override void OnModelCreating(ModelBuilder builder)
	{
		if (Database.IsNpgsql())
		{
			foreach (var entity in builder.Model.GetEntityTypes())
			{
				foreach (var prop in entity.GetDeclaredProperties().Where(p => p.ClrType == typeof(string)))
				{
					prop.AddAnnotation("Relational:ColumnType", "citext");
				}
			}

			builder.HasPostgresExtension("citext");
			builder.HasPostgresExtension("pg_trgm");
		}

		builder.Entity<User>(entity =>
		{
			entity.HasIndex(e => e.NormalizedUserName).IsUnique();

			entity.HasMany(e => e.SentPrivateMessages)
				.WithOne(e => e.FromUser!)
				.OnDelete(DeleteBehavior.Restrict);

			entity.HasMany(e => e.ReceivedPrivateMessages)
				.WithOne(e => e.ToUser!)
				.OnDelete(DeleteBehavior.Restrict);

			entity.HasMany(e => e.UserFiles)
				.WithOne(e => e.Author!)
				.OnDelete(DeleteBehavior.Restrict);

			entity.HasMany(e => e.UserFileComments)
				.WithOne(e => e.User!)
				.OnDelete(DeleteBehavior.Restrict);

			entity.HasMany(e => e.ForumTopicWatches)
				.WithOne(e => e.User!)
				.OnDelete(DeleteBehavior.Restrict);
		});

		builder.Entity<PublicationClass>(entity =>
		{
			entity.Property(e => e.Id)
				.ValueGeneratedNever()
				.HasAnnotation("DatabaseGenerated", DatabaseGeneratedOption.None);

			entity.HasIndex(e => e.Name).IsUnique();
		});

		builder.Entity<UserLogin>(entity =>
		{
			entity.HasKey(e => new { e.LoginProvider, e.ProviderKey });
			entity.HasIndex(e => e.UserId);
		});

		builder.Entity<RoleClaim>(entity =>
		{
			entity.HasIndex(e => e.RoleId);
		});

		builder.Entity<UserClaim>(entity =>
		{
			entity.HasIndex(e => e.UserId);
		});

		builder.Entity<UserRole>(entity =>
		{
			entity.HasKey(e => new { e.UserId, e.RoleId });
			entity.HasIndex(e => e.RoleId);
		});

		builder.Entity<UserToken>(entity =>
		{
			entity.HasKey(e => new { e.UserId, e.LoginProvider, e.Name });
		});

		builder.Entity<RolePermission>()
			.HasKey(rp => new { rp.RoleId, rp.PermissionId });

		builder.Entity<RolePermission>()
			.HasOne(pt => pt.Role)
			.WithMany(p => p.RolePermission)
			.HasForeignKey(pt => pt.RoleId);

		builder.Entity<WikiPage>(entity =>
		{
			entity.HasIndex(e => new { e.PageName, e.Revision })
				.IsUnique();

			if (Database.IsNpgsql())
			{
				entity.HasIndex(e => e.Markup)
					.HasMethod("gin")
					.HasOperators("gin_trgm_ops");
				entity
					.HasGeneratedTsVectorColumn(
						p => p.SearchVector,
						"english",  // Text search config
						p => new { p.PageName, p.Markup })
					.HasIndex(p => p.SearchVector)
					.HasMethod("GIN");
			}
			else
			{
				entity.Ignore(e => e.SearchVector);
			}
		});

		builder.Entity<GameSystem>(entity =>
		{
			entity.Property(e => e.Id)
				.ValueGeneratedNever()
				.HasAnnotation("DatabaseGenerated", DatabaseGeneratedOption.None);

			entity.HasIndex(e => e.Code).IsUnique();
		});

		builder.Entity<GameSystemFrameRate>(entity =>
		{
			entity.HasOne(sf => sf.System)
				.WithMany(s => s.SystemFrameRates)
				.OnDelete(DeleteBehavior.Restrict);

			entity.Property(e => e.Obsolete).HasDefaultValue(false);
		});

		builder.Entity<GameRom>(entity =>
		{
			entity.HasIndex(e => e.Md5).IsUnique();
			entity.HasIndex(e => e.Sha1).IsUnique();
		});

		builder.Entity<SubmissionAuthor>(entity =>
		{
			entity.HasKey(e => new { e.UserId, e.SubmissionId });
			entity.HasIndex(e => e.SubmissionId);
		});

		builder.Entity<PublicationAuthor>(entity =>
		{
			entity.HasKey(e => new { e.UserId, e.PublicationId });
			entity.HasIndex(e => e.PublicationId);
		});

		builder.Entity<Publication>(entity =>
		{
			entity.HasOne(p => p.System)
				.WithMany(s => s.Publications)
				.OnDelete(DeleteBehavior.Restrict);

			entity.HasOne(p => p.Rom)
				.WithMany(r => r.Publications)
				.OnDelete(DeleteBehavior.Restrict);

			entity.HasMany(p => p.ObsoletedMovies)
				.WithOne(p => p.ObsoletedBy!)
				.OnDelete(DeleteBehavior.Restrict);
		});

		builder.Entity<GameGenre>(entity =>
		{
			entity.HasKey(e => new { e.GameId, e.GenreId });
			entity.HasIndex(e => e.GameId);
		});

		builder.Entity<Genre>(entity =>
		{
			entity.Property(e => e.Id)
				.ValueGeneratedNever()
				.HasAnnotation("DatabaseGenerated", DatabaseGeneratedOption.None);
		});

		builder.Entity<Flag>(entity =>
		{
			entity.Property(e => e.Id)
				.ValueGeneratedNever()
				.HasAnnotation("DatabaseGenerated", DatabaseGeneratedOption.None);

			entity.HasIndex(e => e.Token).IsUnique();
		});

		builder.Entity<PublicationTag>(entity =>
		{
			entity.HasKey(e => new { e.PublicationId, e.TagId });
			entity.HasIndex(e => e.PublicationId);
		});

		builder.Entity<PublicationFlag>(entity =>
		{
			entity.HasKey(e => new { e.PublicationId, e.FlagId });
			entity.HasIndex(e => e.PublicationId);
		});

		builder.Entity<PublicationRating>(entity =>
		{
			entity.HasKey(e => new { e.UserId, e.PublicationId, e.Type });
			entity.HasIndex(e => e.PublicationId);
			entity.HasIndex(e => new { e.UserId, e.PublicationId, e.Type })
				.IsUnique();
		});

		builder.Entity<Tag>(entity =>
		{
			entity.HasIndex(e => e.Code).IsUnique();
		});

		builder.Entity<ForumPoll>(entity =>
		{
			entity.HasOne(p => p.Topic)
				.WithOne(t => t.Poll!)
				.HasForeignKey<ForumTopic>(t => t.PollId);
		});

		builder.Entity<Submission>(entity =>
		{
			entity.HasIndex(e => e.Status);
			entity.Property(e => e.ImportedTime).HasDefaultValue(0);
			entity.Property(e => e.LegacyTime).HasDefaultValue(0);
			entity.Property(e => e.ImportedTime).HasColumnType("decimal(16, 4)");
			entity.Property(e => e.LegacyTime).HasColumnType("decimal(16, 4)");

			entity.HasOne(p => p.Topic)
				.WithOne(t => t.Submission!)
				.HasForeignKey<ForumTopic>(t => t.SubmissionId);
		});

		builder.Entity<UserFile>(entity =>
		{
			entity.Property(e => e.Id)
				.ValueGeneratedNever()
				.HasAnnotation("DatabaseGenerated", DatabaseGeneratedOption.None);

			entity.HasIndex(e => e.Hidden);
			entity.Property(e => e.Length).HasColumnType("decimal(10, 3)");
		});

		builder.Entity<PrivateMessage>(entity =>
		{
			entity.HasIndex(e => new { e.ToUserId, e.ReadOn, e.DeletedForToUser });
		});

		builder.Entity<ForumTopicWatch>(entity =>
		{
			entity.HasKey(e => new { e.UserId, e.ForumTopicId });
			entity.HasIndex(e => e.ForumTopicId);
		});

		builder.Entity<SubmissionRejectionReason>(entity =>
		{
			entity.Property(e => e.Id)
				.ValueGeneratedNever()
				.HasAnnotation("DatabaseGenerated", DatabaseGeneratedOption.None);

			entity.HasIndex(e => e.DisplayName).IsUnique();
		});

		builder.Entity<GameGameGroup>(entity =>
		{
			entity.HasKey(e => new { e.GameId, e.GameGroupId });
			entity.HasIndex(e => e.GameId);
		});

		builder.Entity<GameGroup>(entity =>
		{
			entity.HasIndex(e => e.Name)
				.IsUnique();
		});

		builder.Entity<ForumPost>(entity =>
		{
			if (Database.IsNpgsql())
			{
				entity.HasIndex(e => e.Text)
					.HasMethod("gin")
					.HasOperators("gin_trgm_ops");
				entity
					.HasGeneratedTsVectorColumn(
						p => p.SearchVector,
						"english",  // Text search config
						p => p.Text)
					.HasIndex(p => p.SearchVector)
					.HasMethod("GIN");
			}
			else
			{
				entity.Ignore(e => e.SearchVector);
			}
		});

		builder.Entity<PublicationUrl>(entity =>
		{
			entity.HasIndex(e => e.Type);
		});

		builder.Entity<UserDisallow>(entity =>
		{
			entity.HasIndex(e => e.RegexPattern).IsUnique();
		});

		builder.Entity<IpBan>(entity =>
		{
			entity.HasIndex(e => e.Mask).IsUnique();
		});

		builder.Entity<UserMaintenanceLog>(entity =>
		{
			entity.HasOne(e => e.Editor);
			entity.HasOne(e => e.User).WithMany(e => e.UserMaintenanceLogs);
		});

		builder.Entity<DeprecatedMovieFormat>(entity =>
		{
			entity.HasIndex(e => e.FileExtension).IsUnique();
		});
	}

	private void PerformTrackingUpdates()
	{
		ChangeTracker.DetectChanges();

		foreach (var entry in ChangeTracker.Entries()
			.Where(e => e.State == EntityState.Added))
		{
			if (entry.Entity is ITrackable trackable)
			{
				// Don't set if already set
				if (trackable.CreateTimestamp.Year == 1)
				{
					trackable.CreateTimestamp = DateTime.UtcNow;
				}

				if (string.IsNullOrWhiteSpace(trackable.CreateUserName))
				{
					trackable.CreateUserName = GetUser();
				}

				if (trackable.LastUpdateTimestamp.Year == 1)
				{
					trackable.LastUpdateTimestamp = trackable.CreateTimestamp;
				}

				if (string.IsNullOrWhiteSpace(trackable.LastUpdateUserName))
				{
					trackable.LastUpdateUserName = GetUser();
				}
			}
		}

		foreach (var entry in ChangeTracker.Entries()
			.Where(e => e.State == EntityState.Modified))
		{
			if (entry.Entity is ITrackable trackable)
			{
				if (!IsModified(entry, nameof(ITrackable.LastUpdateTimestamp)))
				{
					trackable.LastUpdateTimestamp = DateTime.UtcNow;
				}

				if (!IsModified(entry, nameof(ITrackable.LastUpdateUserName)))
				{
					trackable.LastUpdateUserName = GetUser();
				}
			}
		}
	}

	private string GetUser() => _httpContext?.HttpContext?.User.Identity?.Name ?? SystemUser;

	private static bool IsModified(EntityEntry entry, string propertyName)
		=> entry.Properties
			.Any(prop => prop.Metadata.Name == propertyName && prop.IsModified);
}
