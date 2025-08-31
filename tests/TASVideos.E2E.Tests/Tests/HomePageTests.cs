using TASVideos.E2E.Tests.Base;

namespace TASVideos.E2E.Tests.Tests;

[TestClass]
public class HomePageTests : BaseE2ETest
{
	[TestMethod]
	public async Task HomePage()
	{
		AssertEnabled();

		var response = await Navigate("/");

		AssertResponseCodeAsync(response, 200);

		await AssertElementExistsAsync("a[href*='Games'], a[href*='games']", "Games link");
		await AssertElementExistsAsync("a[href*='Forum'], a[href*='forum']", "Forum link");

		var pageTitle = await Page.TitleAsync();
		Assert.IsTrue(
			pageTitle.Contains("TASVideos", StringComparison.OrdinalIgnoreCase),
			$"Page title should contain 'TASVideos' but was '{pageTitle}'");
	}
}
