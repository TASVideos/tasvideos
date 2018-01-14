using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TASVideos.Data.Entity;
using TASVideos.Data.Entity.Game;

namespace TASVideos.Data
{
	public class ApplicationDbContext : IdentityDbContext<User, Role, int, UserClaim, UserRole, UserLogin, RoleClaim, UserToken>
	{
		private readonly IHttpContextAccessor _httpContext;

		public ApplicationDbContext(DbContextOptions options, IHttpContextAccessor httpContextAccessor)
			: base(options)
		{
			_httpContext = httpContextAccessor;
		}

		public DbSet<RolePermission> RolePermission { get; set; }
		public DbSet<WikiPage> WikiPages { get; set; }
		public DbSet<WikiPageReferral> WikiReferrals { get; set; }
		public DbSet<RoleLink> RoleLinks { get; set; }
		public DbSet<Submission> Submissions { get; set; }
		public DbSet<SubmissionAuthor> SubmissionAuthors { get; set; }
		public DbSet<SubmissionStatusHistory> SubmissionStatusHistory { get; set; }
		public DbSet<Tier> Tiers { get; set; }

		public DbSet<Publication> Publications { get; set; }
		public DbSet<PublicationAuthor> PublicationAuthors { get; set; }

		// Game tables
		public DbSet<Game> Games { get; set; }
		public DbSet<GameSystem> GameSystems { get; set; }
		public DbSet<GameSystemFrameRate> GameSystemFrameRates { get; set; }
		public DbSet<GameRom> Roms { get; set; }

		public override int SaveChanges(bool acceptAllChangesOnSuccess)
		{
			PerformTrackingUpdates();

			ChangeTracker.AutoDetectChangesEnabled = false;
			var result = base.SaveChanges(acceptAllChangesOnSuccess);
			ChangeTracker.AutoDetectChangesEnabled = true;

			return result;
		}

		public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default(CancellationToken))
		{
			PerformTrackingUpdates();
			return base.SaveChangesAsync(cancellationToken);
		}

		protected override void OnModelCreating(ModelBuilder builder)
		{
			builder.Entity<User>(entity =>
			{
				entity.HasIndex(e => e.NormalizedEmail)
					.HasName("EmailIndex");

				entity.HasIndex(e => e.NormalizedUserName)
					.HasName("UserNameIndex")
					.IsUnique()
					.HasFilter($"([{nameof(User.NormalizedUserName)}] IS NOT NULL)");
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
					.HasName("PageNameIndex")
					.IsUnique()
					.HasFilter($"([{nameof(WikiPage.PageName)}] IS NOT NULL)");
			});

			builder.Entity<GameSystem>(entity =>
			{
				entity.Property(e => e.Id)
					.ValueGeneratedNever()
					.HasAnnotation("DatabaseGenerated", DatabaseGeneratedOption.None);

				entity.HasIndex(e => e.Code)
					.IsUnique()
					.HasFilter($"([{nameof(GameSystem.Code)}] IS NOT NULL)");
			});

			builder.Entity<GameSystemFrameRate>(entity =>
			{
				entity.HasOne(sf => sf.System)
					.WithMany(s => s.SystemFrameRates)
					.OnDelete(DeleteBehavior.Restrict);
			});

			builder.Entity<GameRom>(entity =>
			{
				entity.HasIndex(e => e.Md5)
					.IsUnique()
					.HasFilter($"([{nameof(GameRom.Sha1)}] IS NOT NULL)");

				entity.HasIndex(e => e.Sha1)
					.IsUnique()
					.HasFilter($"([{nameof(GameRom.Sha1)}] IS NOT NULL)");
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
					trackable.CreateTimeStamp = DateTime.UtcNow;
					trackable.LastUpdateTimeStamp = DateTime.UtcNow;
					trackable.LastUpdateUserName = _httpContext?.HttpContext?.User?.Identity?.Name;
					trackable.CreateUserName = _httpContext?.HttpContext?.User?.Identity?.Name;
				}
			}

			foreach (var entry in ChangeTracker.Entries()
				.Where(e => e.State == EntityState.Modified))
			{
				if (entry.Entity is ITrackable trackable)
				{
					trackable.LastUpdateTimeStamp = DateTime.UtcNow;
					trackable.LastUpdateUserName = _httpContext?.HttpContext?.User?.Identity?.Name;
				}
			}
		}
	}
}
