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

		AssertResponseCode(response, 200);

		await AssertHasLink("Games");
		await AssertHasLink("Forum");

		var pageTitle = await Page.TitleAsync();
		Assert.IsTrue(
			pageTitle.Contains("TASVideos", StringComparison.OrdinalIgnoreCase),
			$"Page title should contain 'TASVideos' but was '{pageTitle}'");
	}
}
