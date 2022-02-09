using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Npgsql;
using TASVideos.Core.Settings;
using TASVideos.Data;
using SharpCompress.Compressors;

namespace TASVideos.Core.Data;

public static class DbInitializer
{
	public static async Task InitializeDatabase(IServiceProvider services)
	{
		var settings = services.GetRequiredService<IOptions<AppSettings>>();
		var context = services.GetRequiredService<ApplicationDbContext>();
		switch (settings.Value.GetStartupStrategy())
		{
			case StartupStrategy.Minimal:
				await context.Database.EnsureCreatedAsync();
				break;
			case StartupStrategy.Migrate:
				await context.Database.MigrateAsync();
				break;
			case StartupStrategy.Sample:
				await SampleStrategy(context);
				break;
		}
	}

	private static async Task SampleStrategy(DbContext context)
	{
		await context.Database.EnsureDeletedAsync();
		await context.Database.MigrateAsync();

		await GenerateDevSampleData(context);

		// https://github.com/npgsql/npgsql/issues/2366
		// For NpgSql specifically, if we drop and create SCHEMA while running the application
		// we must reload types, as the connector will not do itself and will get an error when querying data
		await using var conn = context.Database.GetDbConnection();
		if (conn is NpgsqlConnection nConn)
		{
			await nConn.OpenAsync();
			nConn.ReloadTypes();
			await nConn.CloseAsync();
		}
	}

	// Adds optional sample data for testing purposes (would not be apart of a production release)
	private static async Task GenerateDevSampleData(DbContext context)
	{
		var sql = await GetSampleDataScript();
		await using (await context.Database.BeginTransactionAsync())
		{
			var commands = new[] { sql };

			foreach (var c in commands)
			{
				// EF needs this BS for some reason
				var escaped = c
					.Replace("{", "{{")
					.Replace("}", "}}");

				await context.Database.ExecuteSqlRawAsync(escaped);
			}

			await context.Database.CommitTransactionAsync();
		}
	}

	private static async Task<string> GetSampleDataScript()
	{
		var bytes = await GetSampleDataFile();
		await using var ms = new MemoryStream(bytes);
		await using var gz = new SharpCompress.Compressors.Deflate.GZipStream(ms, CompressionMode.Decompress);
		using var unzip = new StreamReader(gz);
		return await unzip.ReadToEndAsync();
	}

	private static async Task<byte[]> GetSampleDataFile()
	{
		byte[] bytes;
		string sampleDataPath = Path.Combine(Path.GetTempPath(), "sample-data.sql.gz");
		if (!File.Exists(sampleDataPath))
		{
			bytes = await DownloadSampleDataFile();
			await File.WriteAllBytesAsync(sampleDataPath, bytes);
		}
		else
		{
			var createDate = File.GetLastWriteTimeUtc(sampleDataPath);
			if (createDate < DateTime.UtcNow.AddDays(-1))
			{
				bytes = await DownloadSampleDataFile();
				await File.WriteAllBytesAsync(sampleDataPath, bytes);
			}
			else
			{
				bytes = await File.ReadAllBytesAsync(sampleDataPath);
			}
		}

		return bytes;
	}

	private static async Task<byte[]> DownloadSampleDataFile()
	{
		const string url = "https://tasvideos.org/sample-data/sample-data.sql.gz";
		using var client = new HttpClient();
		using var result = await client.GetAsync(url);
		result.EnsureSuccessStatusCode();
		return await result.Content.ReadAsByteArrayAsync();
	}
}
