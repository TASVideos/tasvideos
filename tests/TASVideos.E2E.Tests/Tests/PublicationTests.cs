using TASVideos.E2E.Tests.Base;

namespace TASVideos.E2E.Tests.Tests;

// TODO:
// Movies list
// by id
// by various other filters
[TestClass]
public class PublicationTests : BaseE2ETest
{
	[TestMethod]
	[DataRow("nes")]
	[DataRow("standard")]
	[DataRow("Y2007")]
	[DataRow("rpg")]
	[DataRow("stars")]
	[DataRow("author782")]
	[DataRow("2p")]
	[DataRow("group8")]
	public async Task MoviesBySystem(string filters)
	{
		AssertEnabled();

		var response = await Navigate($"/Movies-{filters}");

		AssertResponseCode(response, 200);
		await AssertHasLink("Publications/Filter");
		await AssertElementContainsText("label", "Showing items [1 - ", "a label showing a page of records");
		await AssertDoesNotHaveLink("Publications/Edit/3650", "permission locked edit");
		await AssertDoesNotHaveLink("Publications/Catalog/3650", "permission locked catalog");
		await AssertDoesNotHaveLink("Publications/Catalog/3650", "permission locked catalog");
		await AssertDoesNotHaveLink("Publications/Unpublish/3650", "permission unpublish");
	}

	[TestMethod]
	public async Task PublicationsView()
	{
		AssertEnabled();

		var response = await Navigate("/1007M");

		AssertResponseCode(response, 200);
		await AssertHasLink("1007M?handler=Download");
		await AssertDoesNotHaveLink("Publications/Edit/3650", "permission locked edit");
		await AssertDoesNotHaveLink("Publications/Catalog/3650", "permission locked catalog");
		await AssertDoesNotHaveLink("Publications/Catalog/3650", "permission locked catalog");
		await AssertDoesNotHaveLink("Publications/Unpublish/3650", "permission unpublish");
	}

	[TestMethod]
	[DataRow(4987, "bk2", "nes")]
	[DataRow(5924, "ctm", "3ds")]
	[DataRow(2929, "dsm", "ds")]
	[DataRow(2679, "dtm", "wii")]
	[DataRow(1613, "fbm", "arcade")]
	[DataRow(1698, "fm2", "nes")]
	[DataRow(3582, "jrsr", "dos")]
	[DataRow(3099, "lsmv", "snes")]
	[DataRow(6473, "ltm", "dos")]
	[DataRow(1007, "m64", "n64")]
	[DataRow(2318, "omr", "msx")]
	[DataRow(1226, "vbm", "gb")]
	[DataRow(2484, "wtf", "windows")]
	public async Task PublicationFile(int id, string fileExt, string code)
	{
		AssertEnabled();

		var (downloadPath, archive) = await DownloadAndValidateZip(
			$"{id}M?handler=Download",
			$"publication_{id}M");

		var parseResult = await ParseMovieFile(downloadPath);
		Assert.IsTrue(parseResult.Success, $"Movie parsing failed with errors: {string.Join(", ", parseResult.Errors)}");
		Assert.AreEqual(fileExt, parseResult.FileExtension);
		Assert.AreEqual(code, parseResult.SystemCode);
		Assert.IsFalse(parseResult.Warnings.Any());
		Assert.IsFalse(parseResult.Errors.Any());

		CleanupZipDownload(downloadPath, archive);
	}

	[TestMethod]
	[DataRow("/Publications/AdditionalMovies/1")]
	[DataRow("/Publications/Catalog/1")]
	[DataRow("/Publications/Edit/1")]
	[DataRow("/Publications/EditClass/1")]
	[DataRow("/Publications/EditFiles/1")]
	[DataRow("/Publications/PrimaryMovie/1")]
	[DataRow("/Publications/Rate/1")]
	[DataRow("/Publications/Unpublish/1")]
	public async Task PermissionLockedPages_RedirectToLogin(string path)
	{
		AssertEnabled();

		var response = await Navigate(path);
		AssertRedirectToLogin(response);
	}

	[TestMethod]
	[DataRow("name=smb", "1G")]
	[DataRow("id=1", "1M")]
	[DataRow("id=1,2,3", "1M-2M-3M")]
	[DataRow("rec=y", "NewcomerRec")]
	[DataRow("rec=anything", "NewcomerRec")]
	public async Task LegacyRoute_Redirects(string query, string expected)
	{
		AssertEnabled();

		var response = await Navigate($"/movies.cgi?{query}");
		AssertResponseCode(response, 200);
		Assert.IsNotNull(response);
		Assert.IsTrue(response.Url.Contains($"Movies-{expected}"));
	}
}
