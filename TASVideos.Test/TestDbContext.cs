using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Update;
using TASVideos.Data;

namespace TASVideos.Test
{
	/// <summary>
	/// Creates a context optimized for unit testing
	/// Database is in memory, and provides mechanisms for mocking
	/// database conflicts
	/// </summary>
	internal class TestDbContext : ApplicationDbContext
	{
		private bool _dbConcurrentUpdateConflict;
		private bool _dbUpdateConflict;

		private TestDbContext(DbContextOptions<ApplicationDbContext> options, IHttpContextAccessor httpContextAccessor)
			: base(options, httpContextAccessor)
		{
		}

		public static TestDbContext Create()
		{
			var options = new DbContextOptionsBuilder<ApplicationDbContext>()
				.UseInMemoryDatabase("TestDb")
				.Options;

			var db = new TestDbContext(options, null);
			db.Database.EnsureDeleted();
			return db;
		}

		public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default(CancellationToken))
		{
			if (_dbUpdateConflict)
			{
				throw new DbUpdateException("Mock update conflict scenario", new Exception());
			}

			if (_dbConcurrentUpdateConflict)
			{
				throw new DbUpdateConcurrencyException("Mock concurrency conflict scenario", new IUpdateEntry[] { new TestUpdateEntry() });
			}

			return base.SaveChangesAsync(cancellationToken);
		}

		/// <summary>
		/// Simulates a scenario that will throw a <seealso cref="DbUpdateException"/>
		/// These scenarios include constraint violations and model validation errors
		/// </summary>
		public void CreateUpdateConflict()
		{
			_dbUpdateConflict = true;
		}

		/// <summary>
		/// Simulates a scenario that will throw a <seealso cref="DbUpdateConcurrencyException"/>
		/// This happens when an optimistic concurrency check fails
		/// </summary>
		public void CreateConcurrentUpdateConflict()
		{
			_dbConcurrentUpdateConflict = true;
		}
	}

	internal class TestUpdateEntry : IUpdateEntry
	{
		public bool IsModified(IProperty property)
		{
			return false;
		}

		public bool HasTemporaryValue(IProperty property)
		{
			return false;
		}

		public bool IsStoreGenerated(IProperty property)
		{
			return false;
		}

		public object GetCurrentValue(IPropertyBase propertyBase)
		{
			return new object();
		}

		public object GetOriginalValue(IPropertyBase propertyBase)
		{
			return new object();
		}

		public TProperty GetCurrentValue<TProperty>(IPropertyBase propertyBase)
		{
			return default(TProperty);
		}

		public TProperty GetOriginalValue<TProperty>(IProperty property)
		{
			return default(TProperty);
		}

		public void SetCurrentValue(IPropertyBase propertyBase, object value)
		{
		}

		public EntityEntry ToEntityEntry()
		{
			return null;
		}

		// ReSharper disable once UnassignedGetOnlyAutoProperty
		public IEntityType EntityType { get; }

		// ReSharper disable once UnassignedGetOnlyAutoProperty
		public EntityState EntityState { get; }

		// ReSharper disable once UnassignedGetOnlyAutoProperty
		public IUpdateEntry SharedIdentityEntry { get; }
	}
}
