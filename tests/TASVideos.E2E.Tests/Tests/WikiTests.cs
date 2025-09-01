using TASVideos.E2E.Tests.Base;

namespace TASVideos.E2E.Tests.Tests;

[TestClass]
public class WikiTests : BaseE2ETest
{
	[TestMethod]
	public async Task WikiPage()
	{
		AssertEnabled();

		var response = await Navigate("/EmulatorResources");
		AssertResponseCodeAsync(response, 200);
		await AssertElementExistsAsync("article");
	}

	[TestMethod]
	public async Task PageHistory()
	{
		AssertEnabled();
		var response = await Navigate("/Wiki/PageHistory?path=EmulatorResources");
		AssertResponseCodeAsync(response, 200);
	}

	[TestMethod]
	public async Task LatestDiff()
	{
		AssertEnabled();
		var response = await Navigate("/Wiki/PageHistory?path=EmulatorResources&latest=true");
		AssertResponseCodeAsync(response, 200);
	}

	[TestMethod]
	public async Task Referrers()
	{
		AssertEnabled();
		var response = await Navigate("/Wiki/Referrers?path=EmulatorResources");
		AssertResponseCodeAsync(response, 200);
	}

	[TestMethod]
	public async Task ViewSource()
	{
		AssertEnabled();
		var response = await Navigate("/Wiki/ViewSource?path=EmulatorResources");
		AssertResponseCodeAsync(response, 200);
	}

	[TestMethod]
	public async Task Preview()
	{
		AssertEnabled();
		var response = await Navigate("/Wiki/Preview?Id=EmulatorResources");
		AssertResponseCodeAsync(response, 200);
	}

	[TestMethod]
	public async Task DoesNotExist_ReturnsStandardErrorPage()
	{
		AssertEnabled();

		var response = await Navigate("/DoesNotExist");
		AssertResponseCodeAsync(response, 404);
		var content = await Page.TextContentAsync("body");
		Assert.IsNotNull(content);
		Assert.IsTrue(content.Contains("The page you were looking for does not yet exist"));
	}
}
