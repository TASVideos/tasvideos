using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Npgsql;
using TASVideos.Data;

namespace TASVideos.Tests.Base;

public abstract class TestDbBase
{
	protected readonly TestDbContext _db;

	private static IDbContextTransaction? _transaction;
	private static bool _isInitialized = false;
	private static string? _connectionString;

	protected TestDbBase()
	{
		_db = Create();
		_transaction = _db.Database.BeginTransaction();
	}

	public static void AssemblyInit(TestContext context)
	{
		var contextConnectionString = context.Properties["PostgresTestsConnection"]?.ToString();
		var builder = new NpgsqlConnectionStringBuilder(contextConnectionString);
		builder.Database += "-" + Assembly.GetCallingAssembly().GetName().Name;
		_connectionString = builder.ToString();
	}

	public static TestDbContext Create()
	{
		AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
		var options = new DbContextOptionsBuilder<ApplicationDbContext>()
			.UseNpgsql(_connectionString)
			.UseSnakeCaseNamingConvention()
			.EnableSensitiveDataLogging()
			.Options;

		var testHttpContext = new TestDbContext.TestHttpContextAccessor();
		var db = new TestDbContext(options, testHttpContext);

		if (!_isInitialized)
		{
			db.Database.EnsureDeleted();
			db.Database.EnsureCreated();

			// We have constant Forum IDs required by parts of our code, but the Test Database doesn't know about this and starts its IDs at 0.
			// This causes us to eventually run into duplicate IDs. As a workaround, we increase the starting ID to 100.
			db.Database.ExecuteSqlRaw("ALTER SEQUENCE forums_id_seq RESTART WITH 100;");

			_isInitialized = true;
		}

		return db;
	}

	[TestCleanup]
	public void Cleanup()
	{
		_transaction?.Dispose();
	}

	public static void AssemblyCleanup()
	{
		if (_isInitialized)
		{
			var db = Create();
			db.Database.EnsureDeleted();
		}
	}
}
