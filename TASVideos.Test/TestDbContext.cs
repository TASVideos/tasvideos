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
				throw new DbUpdateConcurrencyException("Mock concurrency conflict scenario", new IUpdateEntry[] { new TestUpdateEntry() });
			}

			return base.SaveChangesAsync(cancellationToken);
		}

		public void CreateUpdateConflict()
		{
			_dbUpdateConflict = true;
		}
	}

	internal class TestUpdateEntry : IUpdateEntry
	{
		public bool IsModified(IProperty property)
		{
			throw new NotImplementedException();
		}

		public bool HasTemporaryValue(IProperty property)
		{
			throw new NotImplementedException();
		}

		public bool IsStoreGenerated(IProperty property)
		{
			throw new NotImplementedException();
		}

		public object GetCurrentValue(IPropertyBase propertyBase)
		{
			throw new NotImplementedException();
		}

		public object GetOriginalValue(IPropertyBase propertyBase)
		{
			throw new NotImplementedException();
		}

		public TProperty GetCurrentValue<TProperty>(IPropertyBase propertyBase)
		{
			throw new NotImplementedException();
		}

		public TProperty GetOriginalValue<TProperty>(IProperty property)
		{
			throw new NotImplementedException();
		}

		public void SetCurrentValue(IPropertyBase propertyBase, object value)
		{
			throw new NotImplementedException();
		}

		public EntityEntry ToEntityEntry()
		{
			throw new NotImplementedException();
		}

		public IEntityType EntityType { get; }
		public EntityState EntityState { get; }
		public IUpdateEntry SharedIdentityEntry { get; }
	}
}
