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
		await AssertNoErrorIndicators();

		await AssertElementExistsAsync("a[href*='Games'], a[href*='games']", "Games link");
		await AssertElementExistsAsync("a[href*='Forum'], a[href*='forum']", "Forum link");

		var pageTitle = await Page.TitleAsync();
		Assert.IsTrue(
			pageTitle.Contains("TASVideos", StringComparison.OrdinalIgnoreCase),
			$"Page title should contain 'TASVideos' but was '{pageTitle}'");
	}
}
