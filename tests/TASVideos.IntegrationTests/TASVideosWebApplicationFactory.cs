using Microsoft.EntityFrameworkCore;
using TASVideos.Data;

namespace TASVideos.IntegrationTests;

internal class TASVideosWebApplicationFactory(bool usePostgreSql = false) : WebApplicationFactory<Program>
{
	private string? _testDatabaseName;
	protected override void ConfigureWebHost(IWebHostBuilder builder)
	{
		builder.ConfigureServices(services =>
		{
			// Remove the existing DbContext registration
			var descriptor = services.SingleOrDefault(
				d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
			if (descriptor != null)
			{
				services.Remove(descriptor);
			}

			if (usePostgreSql)
			{
				var connString = GetPostgreSqlConnectionString();
				services.AddDbContext<ApplicationDbContext>(options =>
				{
					options.UseNpgsql(connString)
						.UseSnakeCaseNamingConvention();
				});
			}
			else
			{
				services.AddDbContext<ApplicationDbContext>(options =>
				{
					options.UseInMemoryDatabase($"IntegrationTestDb_{Guid.NewGuid()}");
				});
			}

			// Disable logging for cleaner test output
			services.AddLogging(b => b.SetMinimumLevel(LogLevel.Warning));
		});

		builder.UseEnvironment("Testing");
	}

	private string GetPostgreSqlConnectionString()
	{
		// TODO: get from configuration
		_testDatabaseName = $"tasvideos_integrationtest_{Guid.NewGuid():N}";
		return $"Host=localhost;Database={_testDatabaseName};Username=postgres;Password=postgres";
	}

	/// <summary>
	/// Creates a new test client with a fresh database and does follow redirects
	/// </summary>
	public HttpClient CreateClientWithFollowRedirects()
	{
		var client = CreateClient();
		InitializeDatabase();
		return client;
	}

	/// <summary>
	/// Creates a new test client with a fresh database that does not follow redirects
	/// </summary>
	public HttpClient CreateClientWithNoFollowRedirects()
	{
		var client = CreateClient(new WebApplicationFactoryClientOptions
		{
			AllowAutoRedirect = false
		});
		InitializeDatabase();
		return client;
	}

	private void InitializeDatabase()
	{
		using var scope = Services.CreateScope();
		var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

		context.Database.EnsureDeleted();
		if (usePostgreSql)
		{
			var pendingMigrations = context.Database.GetPendingMigrations().ToList();
			if (pendingMigrations.Count > 0)
			{
				context.Database.Migrate();
			}
			else
			{
				context.Database.EnsureCreated();
			}
		}
		else
		{
			context.Database.EnsureCreated();
		}
	}

	public void SeedDatabase(Action<ApplicationDbContext> seedAction)
	{
		using var scope = Services.CreateScope();
		var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
		seedAction(context);
		context.SaveChanges();
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing && usePostgreSql && !string.IsNullOrEmpty(_testDatabaseName))
		{
			CleanupPostgreSqlDatabase();
		}

		base.Dispose(disposing);
	}

	private void CleanupPostgreSqlDatabase()
	{
		try
		{
			var masterConnectionString = "Host=localhost;Database=postgres;Username=postgres;Password=postgres";
			using var connection = new Npgsql.NpgsqlConnection(masterConnectionString);
			connection.Open();

			// Terminate existing connections to the test database
			var terminateConnections = @"
				SELECT pg_terminate_backend(pg_stat_activity.pid)
				FROM pg_stat_activity
				WHERE pg_stat_activity.datname = $1
				  AND pid <> pg_backend_pid();";

			using var terminateCommand = new Npgsql.NpgsqlCommand(terminateConnections, connection);
			terminateCommand.Parameters.AddWithValue(_testDatabaseName!);
			terminateCommand.ExecuteNonQuery();

			// Drop the database - Note: Database names cannot be parameterized in PostgreSQL
			// But since _testDatabaseName is generated internally with a GUID, it's safe
			var dropCommand = $"DROP DATABASE IF EXISTS \"{_testDatabaseName}\";";
			using var command = new Npgsql.NpgsqlCommand(dropCommand, connection);
			command.ExecuteNonQuery();
		}
		catch (Exception ex)
		{
			// Log the error but don't fail the test cleanup
			Console.WriteLine($"Warning: Failed to cleanup test database '{_testDatabaseName}': {ex.Message}");
		}
	}
}
