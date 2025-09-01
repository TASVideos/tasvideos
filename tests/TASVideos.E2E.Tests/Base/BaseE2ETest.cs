using System.IO.Compression;
using TASVideos.E2E.Tests.Configuration;
using TASVideos.E2E.Tests.Infrastructure;
using TASVideos.MovieParsers;
using TASVideos.MovieParsers.Result;

namespace TASVideos.E2E.Tests.Base;

public class BaseE2ETest : PageTest
{
	protected E2ESettings Settings { get; set; } = null!;

	[TestInitialize]
	public virtual async Task SetupAsync()
	{
		var configuration = new ConfigurationBuilder()
			.SetBasePath(Directory.GetCurrentDirectory())
			.AddJsonFile("appsettings.json", optional: false)
			.AddUserSecrets<BaseE2ETest>()
			.AddEnvironmentVariables()
			.Build();

		Settings = configuration.GetSection("E2ESettings").Get<E2ESettings>() ?? new E2ESettings();

		Context.SetDefaultTimeout(Settings.RequestTimeoutMs);

		await ThrottleManager.WaitIfNeededAsync(Settings);
	}

	protected void AssertEnabled()
	{
		if (!Settings.IsEnabled())
		{
			Assert.Inconclusive("E2E tests not enabled");
		}
	}

	protected async Task<IResponse?> Navigate(string path = "")
	{
		var fullUrl = Path.Combine(Settings.GetTestUrl(), path).Replace('\\', '/');
		if (!fullUrl.StartsWith("http"))
		{
			fullUrl = Settings.GetTestUrl() + "/" + path.TrimStart('/');
		}

		return await ThrottleManager.ExecuteWithThrottleAsync(
			Settings, async () => await Page.GotoAsync(fullUrl, new PageGotoOptions
			{
				WaitUntil = WaitUntilState.NetworkIdle,
				Timeout = Settings.RequestTimeoutMs
			}));
	}

	protected void AssertResponseCodeAsync(IResponse? response, int expectedStatusCode)
	{
		Assert.IsNotNull(response, "Response should not be null");
		Assert.AreEqual(
			expectedStatusCode,
			response.Status,
			$"Expected status code {expectedStatusCode} but got {response.Status}. URL: {response.Url}");
	}

	protected async Task AssertElementExistsAsync(string selector, string? elementDescription = null)
	{
		var element = await Page.QuerySelectorAsync(selector);
		Assert.IsNotNull(
			element,
			$"Element with selector '{selector}' should exist{(elementDescription != null ? $" ({elementDescription})" : "")}");
	}

	protected async Task AssertElementContainsTextAsync(string selector, string expectedText, string? elementDescription = null)
	{
		var element = await Page.QuerySelectorAsync(selector);
		Assert.IsNotNull(
			element,
			$"Element with selector '{selector}' should exist{(elementDescription != null ? $" ({elementDescription})" : "")}");

		var actualText = await element.TextContentAsync();
		Assert.IsTrue(
			actualText?.Contains(expectedText),
			$"Element with selector '{selector}' should contain text '{expectedText}' but contains '{actualText}'{(elementDescription != null ? $" ({elementDescription})" : "")}");
	}

	protected async Task WaitForLoadStateAsync(LoadState loadState = LoadState.NetworkIdle)
	{
		await Page.WaitForLoadStateAsync(loadState, new PageWaitForLoadStateOptions
		{
			Timeout = Settings.RequestTimeoutMs
		});
	}

	protected async Task<(string DownloadPath, ZipArchive Archive)> DownloadAndValidateZipAsync(
		string downloadUrl,
		string filePrefix = "download",
		int timeoutMs = 30000)
	{
		// Set up download handling with extended timeout
		var downloadTask = Page.WaitForDownloadAsync(new PageWaitForDownloadOptions
		{
			Timeout = timeoutMs
		});

		// Get the base URL from settings to construct the full download URL
		var baseUrl = Settings.GetTestUrl().TrimEnd('/');
		var fullDownloadUrl = $"{baseUrl}/{downloadUrl.TrimStart('/')}";

		// Trigger the download by navigating to the download URL
		await Page.EvaluateAsync($"window.location.href = '{fullDownloadUrl}';");

		// Wait for the download to complete
		var download = await downloadTask;
		Assert.IsNotNull(download, "Download should have started");

		// Save to temporary file
		var tempDir = Path.GetTempPath();
		var downloadPath = Path.Combine(tempDir, $"{filePrefix}_{Guid.NewGuid()}.zip");

		await download.SaveAsAsync(downloadPath);

		// Verify file exists and is not empty
		Assert.IsTrue(File.Exists(downloadPath), "Downloaded file should exist");
		var fileInfo = new FileInfo(downloadPath);
		Assert.IsTrue(fileInfo.Length > 0, "Downloaded file should not be empty");

		// Verify it's a valid ZIP file by attempting to open it
		var archive = ZipFile.OpenRead(downloadPath);
		Assert.IsTrue(archive.Entries.Count > 0, "ZIP file should contain at least one entry");

		// Log the contents for verification
		Console.WriteLine($"{filePrefix} ZIP file contains {archive.Entries.Count} entries:");
		foreach (var entry in archive.Entries)
		{
			Console.WriteLine($"  - {entry.FullName} ({entry.Length} bytes)");
		}

		return (downloadPath, archive);
	}

	protected static void CleanupDownload(string downloadPath, ZipArchive? archive = null)
	{
		archive?.Dispose();
		if (File.Exists(downloadPath))
		{
			File.Delete(downloadPath);
		}
	}

	protected static async Task<IParseResult> ParseMovieFile(string downloadPath)
	{
		try
		{
			var movieParser = new MovieParser();
			await using var fileStream = File.OpenRead(downloadPath);
			return await movieParser.ParseZip(fileStream);
		}
		catch(Exception ex)
		{
			Assert.Fail($"Error parsing movie file: {ex.Message}");
			return null;
		}
	}
}
