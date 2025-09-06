using System.IO.Compression;
using System.Net;
using System.Text.Json;
using TASVideos.E2E.Tests.Configuration;
using TASVideos.E2E.Tests.Infrastructure;
using TASVideos.MovieParsers;
using TASVideos.MovieParsers.Result;

namespace TASVideos.E2E.Tests.Base;

public class BaseE2ETest : PageTest
{
	private static readonly JsonSerializerOptions SerializerOptions = new()
	{
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase
	};

	private E2ESettings Settings { get; set; } = null!;

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

	protected static void AssertResponseCode(IResponse? response, int expectedStatusCode)
	{
		Assert.IsNotNull(response, "Response should not be null");
		Assert.AreEqual(
			expectedStatusCode,
			response.Status,
			$"Expected status code {expectedStatusCode} but got {response.Status}. URL: {response.Url}");
	}

	protected async Task AssertElementExists(string selector, string? elementDescription = null)
	{
		var element = await Page.QuerySelectorAsync(selector);
		Assert.IsNotNull(
			element,
			$"Element with selector '{selector}' should exist{(elementDescription != null ? $" ({elementDescription})" : "")}");
	}

	protected async Task AssertElementDoesNotExist(string selector, string? elementDescription = null)
	{
		var element = await Page.QuerySelectorAsync(selector);
		Assert.IsNull(
			element,
			$"Element with selector '{selector}' should NOT exist{(elementDescription != null ? $" ({elementDescription})" : "")}");
	}

	protected async Task AssertElementContainsText(string selector, string expectedText, string? elementDescription = null)
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

	protected async Task AssertPageTitle(string expectedText)
	{
		var element = await Page.QuerySelectorAsync("h1.page-title");
		Assert.IsNotNull(
			element,
			"Page Title Element with selector 'h1.page-title' should exist");

		var actualText = await element.TextContentAsync();
		Assert.IsTrue(actualText?.Contains(expectedText), $"Page title should contain {expectedText}");
	}

	protected Task AssertHasLink(string link, string description = "")
		=> AssertElementExists($"a[href*='{link}']", description);

	protected Task AssertDoesNotHaveLink(string link, string description = "")
		=> AssertElementDoesNotExist($"a[href*='{link}']", description);

	protected async Task<(string DownloadPath, string Content)> DownloadAndValidateTextFile(
		string downloadUrl,
		string filePrefix = "download")
	{
		// Set up download handling with extended timeout
		var downloadTask = Page.WaitForDownloadAsync(new PageWaitForDownloadOptions
		{
			Timeout = 30000
		});

		// Get the base URL from settings to construct the full download URL
		var baseUrl = Settings.GetTestUrl().TrimEnd('/');
		var fullDownloadUrl = $"{baseUrl}/{downloadUrl.TrimStart('/')}";

		await Page.EvaluateAsync($"window.location.href = '{fullDownloadUrl}';");

		var download = await downloadTask;
		Assert.IsNotNull(download, "Download should have started");

		var tempDir = Path.GetTempPath();
		var downloadPath = Path.Combine(tempDir, $"{filePrefix}_{Guid.NewGuid()}");

		await download.SaveAsAsync(downloadPath);

		Assert.IsTrue(File.Exists(downloadPath), "Downloaded file should exist");

		var fileInfo = new FileInfo(downloadPath);
		Assert.IsTrue(fileInfo.Length > 0, "Downloaded file should not be empty");

		using var sr = fileInfo.OpenText();
		var str = await sr.ReadToEndAsync();

		Assert.IsNotNull(str, "text should not be null");
		Assert.IsNotNull(str.Length > 0, "text should not be empty");

		return (downloadPath, str);
	}

	protected async Task<(string DownloadPath, ZipArchive Archive)> DownloadAndValidateZip(
		string downloadUrl,
		string filePrefix = "download")
	{
		// Set up download handling with extended timeout
		var downloadTask = Page.WaitForDownloadAsync(new PageWaitForDownloadOptions
		{
			Timeout = 30000
		});

		// Get the base URL from settings to construct the full download URL
		var baseUrl = Settings.GetTestUrl().TrimEnd('/');
		var fullDownloadUrl = $"{baseUrl}/{downloadUrl.TrimStart('/')}";

		await Page.EvaluateAsync($"window.location.href = '{fullDownloadUrl}';");

		var download = await downloadTask;
		Assert.IsNotNull(download, "Download should have started");

		var tempDir = Path.GetTempPath();
		var downloadPath = Path.Combine(tempDir, $"{filePrefix}_{Guid.NewGuid()}.zip");

		await download.SaveAsAsync(downloadPath);

		Assert.IsTrue(File.Exists(downloadPath), "Downloaded file should exist");

		var fileInfo = new FileInfo(downloadPath);
		Assert.IsTrue(fileInfo.Length > 0, "Downloaded file should not be empty");

		var archive = ZipFile.OpenRead(downloadPath);
		Assert.IsTrue(archive.Entries.Count > 0, "ZIP file should contain at least one entry");

		return (downloadPath, archive);
	}

	protected static void CleanupZipDownload(string downloadPath, ZipArchive? archive = null)
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
		catch (Exception ex)
		{
			Assert.Fail($"Error parsing movie file: {ex.Message}");
			return null;
		}
	}

	protected static void AssertRedirectToLogin(IResponse? response)
	{
		Assert.IsNotNull(response);
		AssertResponseCode(response, 200);
		Assert.IsTrue(response.Url.ToLower().Contains("account/login?returnurl="), $"Expected Url Account/Login but got {response.Url}");
	}

	protected static void AssertAccessDenied(IResponse? response)
	{
		Assert.IsNotNull(response);
		AssertResponseCode(response, 200);
		Assert.IsTrue(response.Url.ToLower().Contains("account/accessdenied"), $"Expected Url Account/AccessDenied but got {response.Url}");
	}

	protected async Task<IAPIResponse> ApiGetAsync(string endpoint)
	{
		var baseUrl = Settings.GetTestUrl().TrimEnd('/');
		return await Page.APIRequest.GetAsync($"{baseUrl}/api/v1/{endpoint.TrimStart('/')}");
	}

	protected static void AssertApiOk(IAPIResponse? response)
	{
		Assert.IsNotNull(response);
		Assert.AreEqual((int)HttpStatusCode.OK, response.Status);
	}

	protected static void AssertApiNotFound(IAPIResponse? response)
	{
		Assert.IsNotNull(response);
		Assert.AreEqual((int)HttpStatusCode.NotFound, response.Status);
	}

	protected static void AssertApiBadRequest(IAPIResponse? response)
	{
		Assert.IsNotNull(response);
		Assert.AreEqual((int)HttpStatusCode.BadRequest, response.Status);
	}

	protected static async Task<T> Deserialize<T>(IAPIResponse response)
	{
		var content = await response.TextAsync();
		Assert.IsFalse(string.IsNullOrEmpty(content));

		var obj = JsonSerializer.Deserialize<T>(content, SerializerOptions);
		Assert.IsNotNull(obj);

		return obj;
	}
}
