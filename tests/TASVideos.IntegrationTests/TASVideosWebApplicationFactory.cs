using Microsoft.EntityFrameworkCore;
using TASVideos.Data;

namespace TASVideos.IntegrationTests;

internal class TASVideosWebApplicationFactory : WebApplicationFactory<Program>
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

			// Use in-memory database for testing
			services.AddDbContext<ApplicationDbContext>(options =>
			{
				options.UseInMemoryDatabase($"IntegrationTestDb_{Guid.NewGuid()}");
			});

			// Disable logging for cleaner test output
			services.AddLogging(builder => builder.SetMinimumLevel(LogLevel.Warning));
		});

		builder.UseEnvironment("Testing");
	}

	/// <summary>
	/// Creates a new test client with a fresh database
	/// </summary>
	public HttpClient CreateClientWithFreshDatabase()
	{
		var client = CreateClient();

		using var scope = Services.CreateScope();
		var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
		context.Database.EnsureDeleted();
		context.Database.EnsureCreated();

		return client;
	}

	/// <summary>
	/// Creates a new test client with a fresh database that does not follow redirects
	/// </summary>
	public HttpClient CreateClientWithFreshDatabaseNoRedirects()
	{
		var client = CreateClient(new WebApplicationFactoryClientOptions
		{
			AllowAutoRedirect = false
		});

		using var scope = Services.CreateScope();
		var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
		context.Database.EnsureDeleted();
		context.Database.EnsureCreated();

		return client;
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
