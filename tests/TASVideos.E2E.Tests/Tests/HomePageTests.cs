using TASVideos.E2E.Tests.Base;

namespace TASVideos.E2E.Tests.Tests;

[TestClass]
public class HomePageTests : BaseE2ETest
{
	[TestMethod]
	public async Task HomePage_ShouldLoad_Successfully()
	{
		var response = await NavigateWithThrottleAsync("/");

		AssertResponseCodeAsync(response, 200);
	}

	[TestMethod]
	public async Task HomePage_ShouldContain_ExpectedElements()
	{
		var response = await NavigateWithThrottleAsync("/");

		AssertResponseCodeAsync(response, 200);
		await AssertElementExistsAsync("title", "Page title");
		await AssertElementExistsAsync("nav", "Main navigation");
		await AssertElementExistsAsync("header", "Page header");
		await AssertElementExistsAsync("main, .main, #main", "Main content area");
		await AssertElementExistsAsync("footer", "Page footer");
	}

	[TestMethod]
	public async Task HomePage_ShouldContain_TASVideosTitle()
	{
		var response = await NavigateWithThrottleAsync("/");

		AssertResponseCodeAsync(response, 200);

		var pageTitle = await Page.TitleAsync();
		Assert.IsTrue(
			pageTitle.Contains("TASVideos", StringComparison.OrdinalIgnoreCase),
			$"Page title should contain 'TASVideos' but was '{pageTitle}'");
	}

	[TestMethod]
	public async Task HomePage_ShouldContain_NavigationLinks()
	{
		var response = await NavigateWithThrottleAsync("/");

		AssertResponseCodeAsync(response, 200);
		await AssertElementExistsAsync("a[href*='Publications'], a[href*='publications']", "Publications link");
		await AssertElementExistsAsync("a[href*='Games'], a[href*='games']", "Games link");
		await AssertElementExistsAsync("a[href*='Forum'], a[href*='forum']", "Forum link");
	}

	[TestMethod]
	public async Task HomePage_ShouldLoad_WithinTimeout()
	{
		var startTime = DateTime.UtcNow;

		var response = await NavigateWithThrottleAsync("/");

		var loadTime = DateTime.UtcNow - startTime;
		AssertResponseCodeAsync(response, 200);
		Assert.IsTrue(
			loadTime.TotalMilliseconds < Settings.RequestTimeoutMs,
			$"Page should load within {Settings.RequestTimeoutMs}ms but took {loadTime.TotalMilliseconds}ms");
	}

	[TestMethod]
	public async Task HomePage_ShouldNotContain_ErrorMessages()
	{
		var response = await NavigateWithThrottleAsync("/");

		AssertResponseCodeAsync(response, 200);

		var content = await Page.TextContentAsync("body");

		var errorIndicators = new[] { "error", "exception", "500", "404", "not found" };

		foreach (var errorIndicator in errorIndicators)
		{
			Assert.IsFalse(
				content?.Contains(errorIndicator, StringComparison.OrdinalIgnoreCase),
				$"Page should not contain error indicator '{errorIndicator}' but page content does");
		}
	}
}
