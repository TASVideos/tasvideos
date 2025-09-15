using TASVideos.E2E.Tests.Base;

namespace TASVideos.E2E.Tests.Tests;

[TestClass]
public class ForumTests : BaseE2ETest
{
	[TestMethod]
	public async Task Forum()
	{
		AssertEnabled();

		var response = await Navigate("/Forum");

		AssertResponseCode(response, 200);
		await AssertPageTitle("Forum");
		await AssertHasLink("Forum/Posts/Unanswered");
		await AssertHasLink("Forum/Subforum", "link to a subforum");
		await AssertElementExists("button[id='mark-all-posts']");
		await AssertDoesNotHaveLink("Forum/Subforum/Create", "permission locked subforum create");
		await AssertDoesNotHaveLink("Forum/Edit", "permission locked subforum edit");
		await AssertDoesNotHaveLink("Forum/Subforum/28", "permission locked subforum");
	}

	[TestMethod]
	public async Task Subforum()
	{
		AssertEnabled();

		var response = await Navigate("/Forum/Subforum/2");
		AssertResponseCode(response, 200);
		await AssertPageTitle("General");
		await AssertElementExists("table");
		await AssertElementExists("button[id='mark-all-posts']");
		await AssertDoesNotHaveLink("Forum/Topics/Create/2", "permission locked topic creation");
		await AssertDoesNotHaveLink("Forum/Subforum/Edit/2", "permission locked edit");
	}

	[TestMethod]
	public async Task Topic()
	{
		AssertEnabled();

		var response = await Navigate("/Forum/Topics/19741");
		AssertResponseCode(response, 200);
		await AssertPageTitle("Ask a Judge");
		await AssertElementContainsText("label", "Showing items [1 - ", "a label showing a page of records");
		await AssertDoesNotHaveLink("Forum/Topics/19741?handler=Watch", "watch topic only for logged in users");
		await AssertDoesNotHaveLink("Forum/Topics/SetType/19741", "permission locked setType");
		await AssertDoesNotHaveLink("Forum/Topics/Move/19741", "permission locked move");
		await AssertDoesNotHaveLink("Forum/Topics/Split/19741", "permission locked split");
		await AssertDoesNotHaveLink("Forum/Topics/Merge/19741", "permission locked merge");
		await AssertElementDoesNotExist("button[type='submit'].btn-warning", "lock forum btn");
		await AssertDoesNotHaveLink("Forum/Topics/AddEditPoll/19741", "permission locked poll create");
		await AssertDoesNotHaveLink("Forum/Posts/Create/19741", "permission locked create post btn");
	}

	[TestMethod]
	public async Task Post()
	{
		AssertEnabled();

		var response = await Navigate("/Forum/Posts/284055");
		AssertResponseCode(response, 200);
	}

	[TestMethod]
	public async Task LatestPosts()
	{
		AssertEnabled();

		var response = await Navigate("/Forum/Posts/Latest");
		AssertResponseCode(response, 200);
		await AssertPageTitle("Latest Forum Posts");
		await AssertElementExists("table");
		await AssertHasLink("Forum/Subforum");
		await AssertHasLink("Forum/Topics");
		await AssertHasLink("Forum/Posts");
	}

	[TestMethod]
	public async Task UnansweredPosts()
	{
		AssertEnabled();

		var response = await Navigate("/Forum/Posts/Unanswered");
		AssertResponseCode(response, 200);
		await AssertPageTitle("Unanswered Posts");
		await AssertElementExists("table");

		// Note: it is theoretically possible to have no unanswered posts, but it has never been the case in several decades
		await AssertHasLink("Forum/Subforum");
		await AssertHasLink("Forum/Topics");
		await AssertHasLink("Forum/Posts");
	}

	[TestMethod]
	public async Task RestrictedSubforum_NotLoggedIn_ReturnsNotFound()
	{
		AssertEnabled();

		var response = await Navigate("/Forum/Subforum/28");
		AssertResponseCode(response, 404);
	}

	[TestMethod]
	public async Task RestrictedTopic_NotLoggedIn_ReturnsNotFound()
	{
		AssertEnabled();

		var response = await Navigate("/Forum/Topics/9862");
		AssertResponseCode(response, 404);
	}

	[TestMethod]
	public async Task RestrictedPost_NotLoggedIn_ReturnsNotFound()
	{
		AssertEnabled();

		var response = await Navigate("/Forum/Posts/241608");
		AssertResponseCode(response, 404);
	}

	[TestMethod]
	[DataRow("/Forum/Edit/1")]
	[DataRow("/Forum/Subforum/Edit/2")]
	[DataRow("/Forum/Subforum/Create")]
	[DataRow("/Forum/Topics/AddEditPoll/19741")]
	[DataRow("/Forum/Topics/Catalog/19741")]
	[DataRow("/Forum/Topics/Create/2")]
	[DataRow("/Forum/Topics/Merge/19741")]
	[DataRow("/Forum/Topics/SetType/19741")]
	[DataRow("/Forum/Topics/Split/19741")]
	[DataRow("/Forum/Posts/Create/1")]
	[DataRow("/Forum/Posts/Edit/1")]

	public async Task PermissionLockedPages_RedirectToLogin(string path)
	{
		AssertEnabled();

		var response = await Navigate(path);
		AssertRedirectToLogin(response);
	}

	[TestMethod]
	[DataRow("forum/viewforum.php?f=2")]
	[DataRow("forum/f/2")]
	public async Task LegacySubforum_Redirects(string path)
	{
		AssertEnabled();

		var response = await Navigate(path);

		AssertResponseCode(response, 200);
		Assert.IsNotNull(response);
		Assert.IsTrue(response.Url.Contains("Forum/Subforum"));
	}

	[TestMethod]
	[DataRow("forum/t/19741")]
	[DataRow("forum/viewtopic.php?t=19741")]
	[DataRow("forum/p/462730")]
	public async Task LegacyTopic_Redirects(string path)
	{
		AssertEnabled();

		var response = await Navigate(path);

		AssertResponseCode(response, 200);
		Assert.IsNotNull(response);
		Assert.IsTrue(response.Url.Contains("Forum/Topics"));
	}

	[TestMethod]
	public async Task LegacyMoodReport_Redirects()
	{
		AssertEnabled();

		var response = await Navigate("/forum/moodreport.php");
		AssertResponseCode(response, 200);
		Assert.IsNotNull(response);
		Assert.IsTrue(response.Url.Contains("Forum/MoodReport"));
	}
}
