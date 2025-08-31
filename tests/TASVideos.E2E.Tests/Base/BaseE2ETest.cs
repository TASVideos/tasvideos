using TASVideos.E2E.Tests.Configuration;
using TASVideos.E2E.Tests.Infrastructure;

namespace TASVideos.E2E.Tests.Base;

public class BaseE2ETest : PageTest
{
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
}
