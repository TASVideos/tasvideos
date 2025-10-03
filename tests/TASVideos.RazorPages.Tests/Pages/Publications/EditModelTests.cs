using TASVideos.Core.Services;
using TASVideos.Core.Services.Wiki;
using TASVideos.Pages.Publications;
using TASVideos.Services;
using TASVideos.Tests.Base;

namespace TASVideos.RazorPages.Tests.Pages.Publications;

[TestClass]
public class EditModelTests : TestDbBase
{
	private readonly IPublications _publications;
	private readonly IWikiPages _wikiPages;
	private readonly EditModel _page;

	public EditModelTests()
	{
		_publications = Substitute.For<IPublications>();
		var publisher = Substitute.For<IExternalMediaPublisher>();
		_wikiPages = Substitute.For<IWikiPages>();
		var maintenanceLogger = Substitute.For<IPublicationMaintenanceLogger>();
		_page = new EditModel(_db, _publications, publisher, _wikiPages, maintenanceLogger);
	}

	[TestMethod]
	public async Task OnGet_NoPublication_ReturnsNotFound()
	{
		_page.Id = 999;
		var actual = await _page.OnGet();
		Assert.IsInstanceOfType<NotFoundResult>(actual);
	}

	[TestMethod]
	public async Task OnGet_ValidPublication_PopulatesData()
	{
		var pub = _db.AddPublication("TestAuthor").Entity;
		_db.AttachTag(pub, "test");
		_db.AttachFlag(pub, "test");

		await _db.SaveChangesAsync();

		_page.Id = pub.Id;

		const string markup = "Test markup";
		var wikiPage = new WikiResult { Markup = markup };
		_wikiPages.Page($"InternalSystem/PublicationContent/M{pub.Id}").Returns(wikiPage);

		var actual = await _page.OnGet();

		Assert.IsInstanceOfType<PageResult>(actual);
		Assert.AreEqual(pub.Title, _page.Publication.Title);
		Assert.AreEqual(pub.MovieFileName, _page.Publication.MovieFileName);
		Assert.AreEqual(markup, _page.Publication.Markup);
		Assert.IsTrue(_page.Publication.Authors.Count >= 1);
		Assert.IsTrue(_page.Publication.Authors.Contains("TestAuthor"));
		Assert.AreEqual(1, _page.Publication.SelectedTags.Count);
		Assert.AreEqual(1, _page.Publication.SelectedFlags.Count);
	}

	[TestMethod]
	public async Task OnPost_InvalidModelState_ReturnsPage()
	{
		_page.ModelState.AddModelError("Title", "Invalid title");

		var actual = await _page.OnPost();
		Assert.IsFalse(_page.ModelState.IsValid);
		Assert.IsInstanceOfType<PageResult>(actual);
	}

	[TestMethod]
	public async Task OnPost_ObsoletedByNonexistentPublication_ValidationError()
	{
		var pub = _db.AddPublication().Entity;
		_page.Id = pub.Id;
		_page.Publication = new EditModel.PublicationEdit
		{
			ObsoletedBy = 999, // Non-existent publication
			Authors = ["TestUser"]
		};

		var actual = await _page.OnPost();

		Assert.IsInstanceOfType<PageResult>(actual);
		Assert.IsFalse(_page.ModelState.IsValid);
		Assert.IsTrue(_page.ModelState.ContainsKey("Publication.ObsoletedBy"));
	}

	[TestMethod]
	public async Task OnPost_ValidEdit_UpdatesPublicationAndRedirects()
	{
		var pub = _db.AddPublication().Entity;
		_db.AddUser("UpdatedAuthor");
		await _db.SaveChangesAsync();

		_page.Id = pub.Id;
		_page.Publication = new EditModel.PublicationEdit
		{
			EmulatorVersion = "Updated Version",
			ExternalAuthors = "External Author",
			Authors = ["UpdatedAuthor"],
			Markup = "Updated markup",
			RevisionMessage = "Test update"
		};

		var user = _db.AddUser("Editor").Entity;
		AddAuthenticatedUser(_page, user, [PermissionTo.EditPublicationMetaData]);

		var existingWikiPage = new WikiResult { Markup = "Old markup" };
		var newWikiPage = new WikiResult { Markup = "Updated markup" };
		_wikiPages.Page($"InternalSystem/PublicationContent/M{pub.Id}").Returns(existingWikiPage);
		_wikiPages.Add(Arg.Any<WikiCreateRequest>()).Returns(newWikiPage);

		_publications.UpdatePublication(pub.Id, Arg.Any<UpdatePublicationRequest>())
			.Returns(new UpdatePublicationResult(true, [], "Test Publication"));

		var result = await _page.OnPost();

		AssertRedirect(result, "View", pub.Id);
		await _publications.Received(1).UpdatePublication(pub.Id, Arg.Any<UpdatePublicationRequest>());
	}

	[TestMethod]
	public async Task OnPost_YouTubeUrlUpdated_SyncsWithYouTube()
	{
		var pub = _db.AddPublication().Entity;
		_db.AddStreamingUrl(pub, "https://www.youtube.com/watch?v=123");
		await _db.SaveChangesAsync();

		_page.Id = pub.Id;
		_page.Publication = new EditModel.PublicationEdit
		{
			Authors = ["TestUser"],
			Markup = "Test markup"
		};

		var user = _db.AddUser("Editor").Entity;
		AddAuthenticatedUser(_page, user, [PermissionTo.EditPublicationMetaData]);

		var existingWikiPage = new WikiResult { Markup = "Old markup" };
		var newWikiPage = new WikiResult { Markup = "Test markup" };
		_wikiPages.Page($"InternalSystem/PublicationContent/M{pub.Id}").Returns(existingWikiPage);
		_wikiPages.Add(Arg.Any<WikiCreateRequest>()).Returns(newWikiPage);

		_publications.UpdatePublication(pub.Id, Arg.Any<UpdatePublicationRequest>())
			.Returns(new UpdatePublicationResult(true, [], "Test Publication"));

		await _page.OnPost();

		await _publications.Received(1).UpdatePublication(pub.Id, Arg.Any<UpdatePublicationRequest>());
	}

	[TestMethod]
	public async Task OnGetTitle_ValidPublicationId_ReturnsTitle()
	{
		const int id = 123;
		const string title = "Test Title";
		_publications.GetTitle(id).Returns(title);

		var actual = await _page.OnGetTitle(id);

		Assert.IsInstanceOfType<ContentResult>(actual);
		var contentResult = (ContentResult)actual;
		Assert.AreEqual(title, contentResult.Content);
	}

	[TestMethod]
	public async Task OnGetTitle_InvalidPublicationId_ReturnsEmpty()
	{
		var actual = await _page.OnGetTitle(999);

		Assert.IsInstanceOfType<ContentResult>(actual);
		var contentResult = (ContentResult)actual;
		Assert.AreEqual("", contentResult.Content);
	}

	[TestMethod]
	public async Task OnPost_EmptyAuthorsRemoved()
	{
		var pub = _db.AddPublication().Entity;
		_db.AddUser("ValidAuthor");
		await _db.SaveChangesAsync();

		_page.Id = pub.Id;
		_page.Publication = new EditModel.PublicationEdit
		{
			Authors = ["ValidAuthor", "", "  "], // Mix of valid and empty authors
			Markup = "Test markup"
		};

		var user = _db.AddUser("Editor").Entity;
		AddAuthenticatedUser(_page, user, [PermissionTo.EditPublicationMetaData]);

		var existingWikiPage = new WikiResult { Markup = "Test markup" };
		_wikiPages.Page($"InternalSystem/PublicationContent/M{pub.Id}").Returns(existingWikiPage);

		_publications.UpdatePublication(pub.Id, Arg.Any<UpdatePublicationRequest>())
			.Returns(new UpdatePublicationResult(true, [], "Test Publication"));

		await _page.OnPost();

		// Verify only valid author remains (logic now handled in page before calling service)
		Assert.AreEqual(1, _page.Publication.Authors.Count);
		Assert.AreEqual("ValidAuthor", _page.Publication.Authors.First());
		await _publications.Received(1).UpdatePublication(pub.Id, Arg.Any<UpdatePublicationRequest>());
	}

	[TestMethod]
	public void RequiresPermission() => AssertHasPermission(typeof(EditModel), PermissionTo.EditPublicationMetaData);
}
