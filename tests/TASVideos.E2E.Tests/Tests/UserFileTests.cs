using TASVideos.E2E.Tests.Base;

namespace TASVideos.E2E.Tests.Tests;

[TestClass]
public class UserFileTests : BaseE2ETest
{
	[TestMethod]
	public async Task UserFiles()
	{
		AssertEnabled();

		var response = await Navigate("/UserFiles");

		AssertResponseCode(response, 200);
		await AssertElementContainsText("div.card-header", "Users with movies", "card that lists movies by user");
	}

	[TestMethod]
	public async Task UserFilesByAuthor()
	{
		AssertEnabled();

		var response = await Navigate("/UserFiles/ForUser/adelikat");
		AssertResponseCode(response, 200);
		await AssertPageTitle("adelikat");
	}

	[TestMethod]
	public async Task UserFilesByGame()
	{
		AssertEnabled();

		var response = await Navigate("/UserFiles/Game/1");
		AssertResponseCode(response, 200);
		await AssertPageTitle("Super Mario Bros");
	}

	[TestMethod]
	public async Task UserFilesInfo()
	{
		AssertEnabled();

		const string fileId = "638328100328567940";
		var response = await Navigate($"/UserFiles/Info/{fileId}");
		AssertResponseCode(response, 200);
		await AssertPageTitle(fileId);
		await AssertHasLink($"UserFiles/Info/{fileId}?handler=Download");
		await AssertElementExists("pre.language-lua", "preview");
		await AssertDoesNotHaveLink("UserFiles/Edit/638328100328567940", "permission locked edit");
		await AssertDoesNotHaveLink("UserFiles?handler=Delete0", "permission locked delete");
	}

	[TestMethod]
	public async Task UserFileDownload()
	{
		AssertEnabled();

		const string fileId = "638328100328567940";
		var (downloadPath, contentStr) = await DownloadAndValidateTextFile(
			$"UserFiles/Info/{fileId}?handler=Download",
			$"userfile_{fileId}");

		Assert.IsTrue(contentStr.Contains("while true do"));

		CleanupZipDownload(downloadPath);
	}

	[TestMethod]
	[DataRow("/UserFiles/Catalog/638328100328567940")]
	[DataRow("/UserFiles/Upload")]

	public async Task PermissionLockedPages_RedirectToLogin(string path)
	{
		AssertEnabled();

		var response = await Navigate(path);
		AssertRedirectToLogin(response);
	}

	[TestMethod]
	[DataRow("/UserFiles/Edit/638328100328567940")]
	public async Task AccessDenied(string path)
	{
		AssertEnabled();

		var response = await Navigate(path);
		AssertAccessDenied(response);
	}
}
