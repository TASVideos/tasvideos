using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TASVideos.Data.Entity;

namespace TASVideos.Data
{
	public class ApplicationDbContext : IdentityDbContext<User, Role, int, UserClaim, UserRole, UserLogin, RoleClaim, UserToken>
	{
		private readonly IHttpContextAccessor _httpContext;

		public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IHttpContextAccessor httpContextAccessor)
			: base(options)
		{
			_httpContext = httpContextAccessor;
		}

		public DbSet<RolePermission> RolePermission { get; set; }
		public DbSet<WikiPage> WikiPages { get; set; }
		public DbSet<WikiPageReferral> WikiReferrals { get; set; }

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
		}
	}
}
