using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Npgsql;
using TASVideos.Data;

namespace TASVideos.Tests.Base;

[TestClass]
public class TestDbBase
{
	protected TestDbContext _db;

	private static IDbContextTransaction? _transaction;
	private static bool isInitialized = false;
	private static string? connectionString;

	public TestDbBase()
	{
		_db = Create();
		_transaction = _db.Database.BeginTransaction();
	}

	public static void AssemblyInit(TestContext context)
	{
		var contextConnectionString = context.Properties["PostgresTestsConnection"]?.ToString();
		var builder = new NpgsqlConnectionStringBuilder(contextConnectionString);
		builder.Database += "-" + Assembly.GetCallingAssembly().GetName().Name;
		connectionString = builder.ToString();
	}

	public static TestDbContext Create()
	{
		AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
		var options = new DbContextOptionsBuilder<ApplicationDbContext>()
			.UseNpgsql(connectionString)
			.UseSnakeCaseNamingConvention()
			.EnableSensitiveDataLogging()
			.Options;

		var testHttpContext = new TestDbContext.TestHttpContextAccessor();
		var db = new TestDbContext(options, testHttpContext);

		if (!isInitialized)
		{
			db.Database.EnsureDeleted();
			db.Database.EnsureCreated();
			isInitialized = true;
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
		if (isInitialized)
		{
			var db = Create();
			db.Database.EnsureDeleted();
		}
	}
}
