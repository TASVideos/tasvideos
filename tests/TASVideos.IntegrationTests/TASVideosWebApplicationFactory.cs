using Microsoft.EntityFrameworkCore;
using TASVideos.Data;

namespace TASVideos.IntegrationTests;

internal class TASVideosWebApplicationFactory(bool usePostgreSql = false) : WebApplicationFactory<Program>
{
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

	private static string GetPostgreSqlConnectionString()
	{
		// Check for environment variable first
		var connectionString = Environment.GetEnvironmentVariable("INTEGRATION_TEST_DB_CONNECTION");
		if (!string.IsNullOrEmpty(connectionString))
		{
			return connectionString;
		}

		// Default test database connection string
		var testDbName = $"tasvideos_integrationtest_{Guid.NewGuid():N}";
		return $"Host=localhost;Database={testDbName};Username=postgres;Password=postgres";
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

		if (usePostgreSql)
		{
			// For PostgreSQL, delete and recreate the database
			context.Database.EnsureDeleted();

			// Check if there are migrations to apply
			var pendingMigrations = context.Database.GetPendingMigrations().ToList();
			if (pendingMigrations.Count != 0)
			{
				// Apply migrations to create the database with proper schema
				context.Database.Migrate();
			}
			else
			{
				// No migrations, just create the database
				context.Database.EnsureCreated();
			}
		}
		else
		{
			// For in-memory database, keep existing behavior
			context.Database.EnsureDeleted();
			context.Database.EnsureCreated();
		}
	}

	/// <summary>
	/// Seeds the database with test data
	/// </summary>
	public void SeedDatabase(Action<ApplicationDbContext> seedAction)
	{
		using var scope = Services.CreateScope();
		var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
		seedAction(context);
		context.SaveChanges();
	}
}
