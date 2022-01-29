using System;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Update;
using TASVideos.Data;

namespace TASVideos.Tests.Base;

/// <summary>
/// Creates a context optimized for unit testing.
/// Database is in memory, and provides mechanisms for mocking database conflicts.
/// </summary>
public class TestDbContext : ApplicationDbContext
{
	private readonly IHttpContextAccessor _testHttpContext;

	private bool _dbConcurrentUpdateConflict;
	private bool _dbUpdateConflict;

	private TestDbContext(DbContextOptions<ApplicationDbContext> options, TestHttpContextAccessor httpContextAccessor)
		: base(options, httpContextAccessor)
	{
		_testHttpContext = httpContextAccessor;
	}

	public static TestDbContext Create()
	{
		var options = new DbContextOptionsBuilder<ApplicationDbContext>()
			.UseInMemoryDatabase("TestDb")
			.ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
			.Options;

		var testHttpContext = new TestHttpContextAccessor();
		var db = new TestDbContext(options, testHttpContext);
		db.Database.EnsureDeleted();
		return db;
	}

	/// <summary>
	/// Simulates a user having logged in.
	/// </summary>
	public void LogInUser(string userName)
	{
		var identity = new GenericIdentity(userName);
		string[] roles = { "TestRole" };
		var principal = new GenericPrincipal(identity, roles);
		_testHttpContext.HttpContext!.User = principal;
	}

	public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
	{
		if (_dbUpdateConflict)
		{
			// If we need to test other types of conflicts, we can have more flags, with different exception messages
			throw new DbUpdateException("Mock update conflict scenario", new Exception("unique constraint"));
		}

		if (_dbConcurrentUpdateConflict)
		{
			throw new DbUpdateConcurrencyException("Mock concurrency conflict scenario", new IUpdateEntry[] { new TestUpdateEntry() });
		}

		return base.SaveChangesAsync(cancellationToken);
	}

	/// <summary>
	/// Simulates a scenario that will throw a <seealso cref="DbUpdateException"/>.
	/// These scenarios include constraint violations and model validation errors.
	/// </summary>
	public void CreateUpdateConflict()
	{
		_dbUpdateConflict = true;
	}

	/// <summary>
	/// Simulates a scenario that will throw a <seealso cref="DbUpdateConcurrencyException"/>.
	/// This happens when an optimistic concurrency check fails.
	/// </summary>
	public void CreateConcurrentUpdateConflict()
	{
		_dbConcurrentUpdateConflict = true;
	}

	private class TestHttpContextAccessor : IHttpContextAccessor
	{
		public HttpContext? HttpContext { get; set; } = new DefaultHttpContext();
	}
}

internal class TestUpdateEntry : IUpdateEntry
{
	public void SetOriginalValue(IProperty property, object? value)
	{
	}

	public void SetPropertyModified(IProperty property)
	{
	}

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

	public void SetStoreGeneratedValue(IProperty property, object? value)
	{
	}

	public EntityEntry ToEntityEntry()
	{
		return null!;
	}

	public object GetRelationshipSnapshotValue(IPropertyBase propertyBase)
	{
		throw new NotImplementedException();
	}

	public object GetPreStoreGeneratedCurrentValue(IPropertyBase propertyBase)
	{
		throw new NotImplementedException();
	}

	public bool IsConceptualNull(IProperty property)
	{
		throw new NotImplementedException();
	}

	// ReSharper disable once UnassignedGetOnlyAutoProperty
	public IEntityType EntityType => null!;
	public EntityState EntityState { get; set; }

	// ReSharper disable once UnassignedGetOnlyAutoProperty
	public IUpdateEntry SharedIdentityEntry => null!;
}
