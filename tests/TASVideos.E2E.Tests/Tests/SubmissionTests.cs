using TASVideos.E2E.Tests.Base;

namespace TASVideos.E2E.Tests.Tests;

[TestClass]
public class SubmissionTests : BaseE2ETest
{
	[TestMethod]
	public async Task SubsList()
	{
		AssertEnabled();

		var response = await Navigate("/Subs-List");

		AssertResponseCodeAsync(response, 200);
		await AssertElementExistsAsync("table");
	}

	[TestMethod]
	public async Task SubsListByAuthor()
	{
		AssertEnabled();

		var response = await Navigate("/Subs-List?user=adelikat");
		AssertResponseCodeAsync(response, 200);
		await AssertElementExistsAsync("table");
		await AssertElementExistsAsync("a[href*='415S']", "link to first submission by author");
	}

	[TestMethod]
	public async Task SubsListByYear()
	{
		AssertEnabled();

		var response = await Navigate("/Subs-List?Search.Years=2007");
		AssertResponseCodeAsync(response, 200);
		await AssertElementExistsAsync("table");
		await AssertElementExistsAsync("a[href*='1412S']", "link to first submission in year");
	}

	[TestMethod]
	public async Task SubsListByStatus()
	{
		AssertEnabled();

		var response = await Navigate("/Subs-List?Search.Statuses=7"); // Rejected
		AssertResponseCodeAsync(response, 200);
		await AssertElementExistsAsync("table");
		await AssertElementExistsAsync("a[href*='38S']", "link to a rejected submission");
	}

	[TestMethod]
	public async Task SubmissionsView()
	{
		AssertEnabled();

		var response = await Navigate("/1140S");

		AssertResponseCodeAsync(response, 200);
		await AssertElementExistsAsync("div.alert.alert-success");
		await AssertElementExistsAsync("a[href*='576M']", "link to corresponding publication");
	}

	[TestMethod]
	public async Task SubmissionFile()
	{
		AssertEnabled();

		var (downloadPath, archive) = await DownloadAndValidateZipAsync(
			"9844S?handler=Download",
			"submission_9844S");

		var parseResult = await ParseMovieFile(downloadPath);
		Assert.IsTrue(parseResult.Success, $"Movie parsing failed with errors: {string.Join(", ", parseResult.Errors)}");
		Assert.AreEqual("snes", parseResult.SystemCode);
		Assert.AreEqual("bk2", parseResult.FileExtension);
		Assert.AreEqual(15024, parseResult.RerecordCount);
		Assert.IsFalse(parseResult.Warnings.Any());
		Assert.IsFalse(parseResult.Errors.Any());

		CleanupDownload(downloadPath, archive);
	}
}
