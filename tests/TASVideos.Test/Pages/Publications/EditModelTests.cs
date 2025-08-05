using TASVideos.Common;
using TASVideos.Core.Services;
using TASVideos.Core.Services.Wiki;
using TASVideos.Core.Services.Youtube;
using TASVideos.Data.Entity;
using TASVideos.Pages.Publications;
using TASVideos.Services;
using TASVideos.Tests.Base;
using static TASVideos.RazorPages.Tests.RazorTestHelpers;

namespace TASVideos.RazorPages.Tests.Pages.Publications;

[TestClass]
public class EditModelTests : TestDbBase
{
	private readonly IWikiPages _wikiPages;
	private readonly ITagService _tagService;
	private readonly IFlagService _flagService;
	private readonly IYoutubeSync _youtubeSync;
	private readonly EditModel _page;

	public EditModelTests()
	{
		var publisher = Substitute.For<IExternalMediaPublisher>();
		_wikiPages = Substitute.For<IWikiPages>();
		_tagService = Substitute.For<ITagService>();
		_flagService = Substitute.For<IFlagService>();
		var maintenanceLogger = Substitute.For<IPublicationMaintenanceLogger>();
		_youtubeSync = Substitute.For<IYoutubeSync>();
		_page = new EditModel(_db, publisher, _wikiPages, _tagService, _flagService, maintenanceLogger, _youtubeSync);
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
		var pub = _db.AddPublication().Entity;
		var author = _db.AddUser("TestAuthor").Entity;

		_db.PublicationAuthors.Add(new PublicationAuthor
		{
			Publication = pub,
			Author = author,
			Ordinal = 1
		});

		var tag = _db.Tags.Add(new Tag { Id = 1, Code = "test", DisplayName = "Test Tag" }).Entity;
		_db.PublicationTags.Add(new PublicationTag { Publication = pub, Tag = tag });

		var flag = _db.Flags.Add(new Flag { Id = 1, Token = "test", Name = "Test Flag" }).Entity;
		_db.PublicationFlags.Add(new PublicationFlag { Publication = pub, Flag = flag });

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
		_ = _db.AddUser("UpdatedAuthor").Entity;
		_ = _db.Tags.Add(new Tag { Id = 1, Code = "NewTag", DisplayName = "New Tag" }).Entity;
		await _db.SaveChangesAsync();

		_page.Id = pub.Id;
		_page.Publication = new EditModel.PublicationEdit
		{
			EmulatorVersion = "Updated Version",
			ExternalAuthors = "External Author",
			Authors = ["UpdatedAuthor"],
			SelectedTags = [1],
			SelectedFlags = [],
			Markup = "Updated markup",
			RevisionMessage = "Test update"
		};

		var user = _db.AddUser("Editor").Entity;
		AddAuthenticatedUser(_page, user, [PermissionTo.EditPublicationMetaData]);

		var existingWikiPage = new WikiResult { Markup = "Old markup" };
		var newWikiPage = new WikiResult { Markup = "Updated markup" };
		_wikiPages.Page($"InternalSystem/PublicationContent/M{pub.Id}").Returns(existingWikiPage);
		_wikiPages.Add(Arg.Any<WikiCreateRequest>()).Returns(newWikiPage);

		_tagService.GetDiff(Arg.Any<IEnumerable<int>>(), Arg.Any<IEnumerable<int>>())
			.Returns(new ListDiff([], []));
		_flagService.GetDiff(Arg.Any<IEnumerable<int>>(), Arg.Any<IEnumerable<int>>())
			.Returns(new ListDiff([], []));

		var actual = await _page.OnPost();

		Assert.IsInstanceOfType<RedirectToPageResult>(actual);
		var redirectResult = (RedirectToPageResult)actual;
		Assert.AreEqual("View", redirectResult.PageName);
		Assert.AreEqual(pub.Id, redirectResult.RouteValues!["Id"]);
	}

	[TestMethod]
	public async Task OnPost_YouTubeUrlUpdated_SyncsWithYouTube()
	{
		var pub = _db.AddPublication().Entity;
		const string youtubeUrl = "https://www.youtube.com/watch?v=123";

		_db.PublicationUrls.Add(new PublicationUrl
		{
			Publication = pub,
			Url = youtubeUrl,
			Type = PublicationUrlType.Streaming
		});
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

		_youtubeSync.IsYoutubeUrl(youtubeUrl).Returns(true);
		_tagService.GetDiff(Arg.Any<IEnumerable<int>>(), Arg.Any<IEnumerable<int>>())
			.Returns(new ListDiff([], []));
		_flagService.GetDiff(Arg.Any<IEnumerable<int>>(), Arg.Any<IEnumerable<int>>())
			.Returns(new ListDiff([], []));

		await _page.OnPost();

		await _youtubeSync.Received(1).SyncYouTubeVideo(Arg.Is<YoutubeVideo>(v =>
			v.Id == pub.Id &&
			v.Url == youtubeUrl));
	}

	[TestMethod]
	public async Task OnGetTitle_ValidPublicationId_ReturnsTitle()
	{
		var pub = _db.AddPublication().Entity;

		var actual = await _page.OnGetTitle(pub.Id);

		Assert.IsInstanceOfType<ContentResult>(actual);
		var contentResult = (ContentResult)actual;
		Assert.AreEqual(pub.Title, contentResult.Content);
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
		_ = _db.AddUser("ValidAuthor").Entity;
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

		_tagService.GetDiff(Arg.Any<IEnumerable<int>>(), Arg.Any<IEnumerable<int>>())
			.Returns(new ListDiff([], []));
		_flagService.GetDiff(Arg.Any<IEnumerable<int>>(), Arg.Any<IEnumerable<int>>())
			.Returns(new ListDiff([], []));

		await _page.OnPost();

		// Verify only valid author remains
		Assert.AreEqual(1, _page.Publication.Authors.Count);
		Assert.AreEqual("ValidAuthor", _page.Publication.Authors.First());
	}
}