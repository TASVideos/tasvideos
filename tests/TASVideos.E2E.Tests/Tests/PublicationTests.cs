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
		await AssertElementExistsAsync("a[href*='Publications/Filter']", "link to the filters page");
		await AssertElementContainsTextAsync("label", "Showing items [1 - ", "a label showing a page of records");

		AssertResponseCodeAsync(response, 200);
	}

	[TestMethod]
	public async Task PublicationsView()
	{
		AssertEnabled();

		var response = await Navigate("/1007M");

		AssertResponseCodeAsync(response, 200);
		await AssertElementExistsAsync("a[href*='1007M?handler=Download']", "link to download the movie file");
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

		var (downloadPath, archive) = await DownloadAndValidateZipAsync(
			$"{id}M?handler=Download",
			$"publication_{id}M");

		var parseResult = await ParseMovieFile(downloadPath);
		Assert.IsTrue(parseResult.Success, $"Movie parsing failed with errors: {string.Join(", ", parseResult.Errors)}");
		Assert.AreEqual(fileExt, parseResult.FileExtension);
		Assert.AreEqual(code, parseResult.SystemCode);
		Assert.IsFalse(parseResult.Warnings.Any());
		Assert.IsFalse(parseResult.Errors.Any());

		CleanupDownload(downloadPath, archive);
	}
}
