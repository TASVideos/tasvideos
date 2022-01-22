using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using TASVideos.Core.Services;
using TASVideos.Core.Settings;
using TASVideos.Data;
using SharpCompress.Compressors;

namespace TASVideos.Core.Data
{
	public static class DbInitializer
	{
		public static void InitializeDatabase(IServiceProvider services)
		{
			var settings = services.GetRequiredService<IOptions<AppSettings>>();
			switch (settings.Value.GetStartupStrategy())
			{
				case StartupStrategy.Minimal:
					MinimalStrategy(services);
					break;
				case StartupStrategy.Sample:
					SampleStrategy(services);
					break;
				case StartupStrategy.Migrate:
					MigrationStrategy(services);
					break;
			}
		}

		private static void MinimalStrategy(IServiceProvider services)
		{
			var context = services.GetRequiredService<ApplicationDbContext>();
			context.Database.EnsureCreated();
			var wikiPages = services.GetRequiredService<IWikiPages>();
			wikiPages.PrePopulateCache();
		}

		private static void SampleStrategy(IServiceProvider services)
		{
			var context = services.GetRequiredService<ApplicationDbContext>();
			context.Database.EnsureDeleted();
			context.Database.EnsureCreated();

			// Note: We specifically do not want to run seed data
			// This data is already baked into the sample data file
			GenerateDevSampleData(context).Wait();
		}

		private static void MigrationStrategy(IServiceProvider services)
		{
			var context = services.GetRequiredService<ApplicationDbContext>();
			context.Database.Migrate();
		}

		/// <summary>
		/// Adds optional sample data
		/// Unlike seed data, sample data is arbitrary data for testing purposes and would not be apart of a production release.
		/// </summary>
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
}
