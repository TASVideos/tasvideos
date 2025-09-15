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

		AssertResponseCode(response, 200);
		await AssertElementExists("table");
		await AssertDoesNotHaveLink("Submissions/Submit", "permission locked submit");
		await AssertDoesNotHaveLink("Subs-List/user=", "my submission link only for logged in users");
	}

	[TestMethod]
	public async Task SubsListByAuthor()
	{
		AssertEnabled();

		var response = await Navigate("/Subs-List?user=adelikat");
		AssertResponseCode(response, 200);
		await AssertElementExists("table");
		await AssertHasLink("415S", "link to first submission by author");
		await AssertDoesNotHaveLink("Submissions/Submit", "permission locked submit");
		await AssertDoesNotHaveLink("Subs-List/user=", "my submission link only for logged in users");
	}

	[TestMethod]
	public async Task SubsListByYear()
	{
		AssertEnabled();

		var response = await Navigate("/Subs-List?Search.Years=2007");
		AssertResponseCode(response, 200);
		await AssertElementExists("table");
		await AssertHasLink("1412S", "link to first submission in year");
		await AssertDoesNotHaveLink("Submissions/Submit", "permission locked submit");
		await AssertDoesNotHaveLink("Subs-List/user=", "my submission link only for logged in users");
	}

	[TestMethod]
	public async Task SubsListByStatus()
	{
		AssertEnabled();

		var response = await Navigate("/Subs-List?Search.Statuses=7"); // Rejected
		AssertResponseCode(response, 200);
		await AssertElementExists("table");
		await AssertHasLink("38S", "link to a rejected submission");
		await AssertDoesNotHaveLink("Submissions/Submit", "permission locked submit");
		await AssertDoesNotHaveLink("Subs-List/user=", "my submission link only for logged in users");
	}

	[TestMethod]
	public async Task SubmissionsView()
	{
		AssertEnabled();

		var response = await Navigate("/1140S");

		AssertResponseCode(response, 200);
		await AssertElementExists("div.alert.alert-success");
		await AssertHasLink("576M", "link to corresponding publication");
		await AssertHasLink("1140S?handler=Download", "download link");
		await AssertHasLink("Forum/Topics/4140", "link to discussion topic");
		await AssertHasLink("Subs-List", "link back to submission list");
		await AssertHasLink("Subs-List?user=GuanoBowl", "link to submissions by submitter");
		await AssertDoesNotHaveLink("Submissions/Edit/1140", "permission locked edit");
		await AssertDoesNotHaveLink("Submissions/Catalog/1140", "permission locked catalog");
	}

	[TestMethod]
	public async Task SubmissionFile()
	{
		AssertEnabled();

		var (downloadPath, archive) = await DownloadAndValidateZip(
			"9844S?handler=Download",
			"submission_9844S");

		var parseResult = await ParseMovieFile(downloadPath);
		Assert.IsTrue(parseResult.Success, $"Movie parsing failed with errors: {string.Join(", ", parseResult.Errors)}");
		Assert.AreEqual("snes", parseResult.SystemCode);
		Assert.AreEqual("bk2", parseResult.FileExtension);
		Assert.AreEqual(15024, parseResult.RerecordCount);
		Assert.IsFalse(parseResult.Warnings.Any());
		Assert.IsFalse(parseResult.Errors.Any());

		CleanupZipDownload(downloadPath, archive);
	}

	[TestMethod]
	[DataRow("/Submissions/Catalog/1")]
	[DataRow("/Submissions/Edit/1")]
	[DataRow("/Submissions/Delete/1")]
	[DataRow("/Submissions/Publish/1")]
	[DataRow("/Submissions/Submit")]
	public async Task PermissionLockedPages_RedirectToLogin(string path)
	{
		AssertEnabled();

		var response = await Navigate(path);
		AssertRedirectToLogin(response);
	}

	[TestMethod]
	[DataRow("mode=submit")]
	[DataRow("mode=edit&id=1")]
	public async Task LegacyPermissionLockedRoutes_Redirects(string query)
	{
		AssertEnabled();

		var response = await Navigate($"/queue.cgi?{query}");
		AssertRedirectToLogin(response);
	}

	[TestMethod]
	[DataRow("")]
	[DataRow("?mode=list")]
	public async Task LegacyListRoute_Redirects(string query)
	{
		AssertEnabled();

		var response = await Navigate($"/queue.cgi{query}");
		AssertResponseCode(response, 200);
		Assert.IsNotNull(response);
		Assert.IsTrue(response.Url.Contains("Subs-List"));
	}

	[TestMethod]
	[DataRow("id=1", "1S")]
	[DataRow("mode=view&id=1", "1S")]
	public async Task LegacyViewRoutes_Redirect(string query, string expected)
	{
		AssertEnabled();

		var response = await Navigate($"/queue.cgi?{query}");
		AssertResponseCode(response, 200);
		Assert.IsNotNull(response);
		Assert.IsTrue(response.Url.Contains(expected));
	}
}
