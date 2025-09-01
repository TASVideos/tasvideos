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
		AssertResponseCode(response, 200);
		await AssertElementExists("article");
		await AssertHasLink("Wiki/PageHistory?path=EmulatorResources");
		await AssertHasLink("Wiki/Referrers?path=EmulatorResources");
		await AssertHasLink("Wiki/ViewSource?path=EmulatorResources");
		await AssertDoesNotHaveLink("Wiki/Edit", "permission locked edit");
		await AssertDoesNotHaveLink("Wiki/Move", "permission locked move");
		await AssertDoesNotHaveLink("Wiki/DeletedPages/DeletePage", "permission locked delete");
	}

	[TestMethod]
	public async Task PageHistory()
	{
		AssertEnabled();
		var response = await Navigate("/Wiki/PageHistory?path=EmulatorResources");
		AssertResponseCode(response, 200);
		await AssertDoesNotHaveLink("Wiki/DeletedPages/DeleteRevision", "permission locked delete");
		await AssertDoesNotHaveLink("Wiki/Edit/RollbackLatest", "permission locked rollback");
	}

	[TestMethod]
	public async Task LatestDiff()
	{
		AssertEnabled();
		var response = await Navigate("/Wiki/PageHistory?path=EmulatorResources&latest=true");
		AssertResponseCode(response, 200);
		await AssertDoesNotHaveLink("Wiki/DeletedPages/DeleteRevision", "permission locked delete");
		await AssertDoesNotHaveLink("Wiki/Edit/RollbackLatest", "permission locked rollback");
	}

	[TestMethod]
	public async Task Referrers()
	{
		AssertEnabled();
		var response = await Navigate("/Wiki/Referrers?path=EmulatorResources");
		AssertResponseCode(response, 200);
	}

	[TestMethod]
	public async Task ViewSource()
	{
		AssertEnabled();
		var response = await Navigate("/Wiki/ViewSource?path=EmulatorResources");
		AssertResponseCode(response, 200);
	}

	[TestMethod]
	public async Task Preview()
	{
		AssertEnabled();
		var response = await Navigate("/Wiki/Preview?Id=EmulatorResources");
		AssertResponseCode(response, 200);
	}

	[TestMethod]
	public async Task DoesNotExist_ReturnsStandardErrorPage()
	{
		AssertEnabled();

		var response = await Navigate("/DoesNotExist");
		AssertResponseCode(response, 404);
		var content = await Page.TextContentAsync("body");
		Assert.IsNotNull(content);
		Assert.IsTrue(content.Contains("The page you were looking for does not yet exist"));
	}

	[TestMethod]
	public async Task SystemWikiPages_MustExist()
	{
		AssertEnabled();

		foreach (var page in SystemWiki.Pages)
		{
			var response = await Navigate(page);
			AssertResponseCode(response, 200);
			await AssertElementExists("article");
			await AssertElementContainsText("h3", "This page is a system resource");
		}
	}

	[TestMethod]
	[DataRow("/Wiki/Edit?path=EmulatorResources")]
	[DataRow("/Wiki/Move?path=EmulatorResources")]
	[DataRow("/Wiki/DeletedPages")]
	public async Task PermissionLockedPages_RedirectToLogin(string path)
	{
		AssertEnabled();

		var response = await Navigate(path);
		AssertRedirectToLogin(response);
	}

	[TestMethod]
	public async Task LegacyPrivileges_Redirects()
	{
		AssertEnabled();

		var response = await Navigate("privileges");
		AssertRedirectToLogin(response);
	}

	[TestMethod]
	[DataRow("privileges")]
	[DataRow("submitmovie")]
	public async Task Legacy_Redirects(string legacy)
	{
		AssertEnabled();

		var response = await Navigate(legacy);
		AssertRedirectToLogin(response);
	}
}
