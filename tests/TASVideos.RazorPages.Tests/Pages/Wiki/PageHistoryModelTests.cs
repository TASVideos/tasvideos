using TASVideos.Pages.Wiki;

namespace TASVideos.RazorPages.Tests.Pages.Wiki;

[TestClass]
public class PageHistoryModelTests : BasePageModelTests
{
	private readonly PageHistoryModel _model;

	public PageHistoryModelTests()
	{
		_model = new PageHistoryModel(_db)
		{
			PageContext = TestPageContext()
		};
	}

	[TestMethod]
	public async Task OnGet_NoPath_SetsEmptyPageName()
	{
		_model.Path = null;

		await _model.OnGet();

		Assert.AreEqual("", _model.PageName);
		Assert.AreEqual(0, _model.Revisions.Count);
	}

	[TestMethod]
	public async Task OnGet_PathWithSlashes_TrimsSlashes()
	{
		_model.Path = "/TestPage/";
		await _model.OnGet();
		Assert.AreEqual("TestPage", _model.PageName);
	}

	[TestMethod]
	public async Task OnGet_PageDoesNotExist_ReturnsEmptyRevisions()
	{
		_model.Path = "NonExistentPage";

		await _model.OnGet();

		Assert.AreEqual("NonExistentPage", _model.PageName);
		Assert.AreEqual(0, _model.Revisions.Count);
	}

	[TestMethod]
	public async Task OnGet_PageHasRevisions_ReturnsOrderedRevisions()
	{
		const string pageName = "TestPage";
		var author1 = _db.AddUser("Author1").Entity;
		var author2 = _db.AddUser("Author2").Entity;

		_db.WikiPages.AddRange(
			new WikiPage
			{
				Id = 1,
				PageName = pageName,
				Markup = "First revision",
				Revision = 1,
				IsDeleted = false,
				AuthorId = author1.Id,
				Author = author1,
				ChildId = 2,
				MinorEdit = false,
				RevisionMessage = "Initial creation"
			},
			new WikiPage
			{
				Id = 2,
				PageName = pageName,
				Markup = "Second revision",
				Revision = 2,
				IsDeleted = false,
				AuthorId = author2.Id,
				Author = author2,
				ChildId = null,
				MinorEdit = true,
				RevisionMessage = "Minor update"
			});
		await _db.SaveChangesAsync();

		_model.Path = pageName;

		await _model.OnGet();

		Assert.AreEqual(pageName, _model.PageName);
		Assert.AreEqual(2, _model.Revisions.Count);

		// Should be ordered by revision number
		Assert.AreEqual(1, _model.Revisions[0].Revision);
		Assert.AreEqual(2, _model.Revisions[1].Revision);
		Assert.AreEqual("Author1", _model.Revisions[0].CreateUserName);
		Assert.AreEqual("Author2", _model.Revisions[1].CreateUserName);
		Assert.IsFalse(_model.Revisions[0].MinorEdit);
		Assert.IsTrue(_model.Revisions[1].MinorEdit);
		Assert.AreEqual("Initial creation", _model.Revisions[0].RevisionMessage);
		Assert.AreEqual("Minor update", _model.Revisions[1].RevisionMessage);
	}

	[TestMethod]
	public async Task OnGet_PageHasDeletedRevisions_ExcludesDeleted()
	{
		const string pageName = "TestPage";
		var author = _db.AddUser("TestUser").Entity;

		_db.WikiPages.AddRange(
			new WikiPage
			{
				Id = 1,
				PageName = pageName,
				Markup = "Active revision",
				Revision = 1,
				IsDeleted = false,
				AuthorId = author.Id,
				Author = author,
				ChildId = null
			},
			new WikiPage
			{
				Id = 2,
				PageName = pageName,
				Markup = "Deleted revision",
				Revision = 2,
				IsDeleted = true,
				AuthorId = author.Id,
				Author = author,
				ChildId = null
			});
		await _db.SaveChangesAsync();

		_model.Path = pageName;

		await _model.OnGet();

		Assert.AreEqual(1, _model.Revisions.Count);
		Assert.AreEqual(1, _model.Revisions[0].Revision);
	}

	[TestMethod]
	public async Task OnGet_WithFromAndToRevisions_CreatesDiff()
	{
		const string pageName = "TestPage";
		var author = _db.AddUser("TestUser").Entity;

		_db.WikiPages.AddRange(
			new WikiPage
			{
				Id = 1,
				PageName = pageName,
				Markup = "Original content",
				Revision = 1,
				IsDeleted = false,
				AuthorId = author.Id,
				Author = author,
				ChildId = 2
			},
			new WikiPage
			{
				Id = 2,
				PageName = pageName,
				Markup = "Updated content",
				Revision = 2,
				IsDeleted = false,
				AuthorId = author.Id,
				Author = author,
				ChildId = null
			});
		await _db.SaveChangesAsync();

		_model.Path = pageName;
		_model.FromRevision = 1;
		_model.ToRevision = 2;

		await _model.OnGet();

		Assert.AreEqual("Original content", _model.Diff.LeftMarkup);
		Assert.AreEqual("Updated content", _model.Diff.RightMarkup);
	}

	[TestMethod]
	public async Task OnGet_WithSameFromAndToRevision_CreatesSameDiff()
	{
		const string pageName = "TestPage";
		var author = _db.AddUser("TestUser").Entity;

		_db.WikiPages.Add(new WikiPage
		{
			Id = 1,
			PageName = pageName,
			Markup = "Test content",
			Revision = 1,
			IsDeleted = false,
			AuthorId = author.Id,
			Author = author,
			ChildId = null
		});
		await _db.SaveChangesAsync();

		_model.Path = pageName;
		_model.FromRevision = 1;
		_model.ToRevision = 1;

		await _model.OnGet();

		Assert.AreEqual("Test content", _model.Diff.LeftMarkup);
		Assert.AreEqual("Test content", _model.Diff.RightMarkup);
	}

	[TestMethod]
	public async Task OnGet_WithInvalidRevisions_DoesNotCreateDiff()
	{
		const string pageName = "TestPage";
		var author = _db.AddUser("TestUser").Entity;

		_db.WikiPages.Add(new WikiPage
		{
			Id = 1,
			PageName = pageName,
			Markup = "Test content",
			Revision = 1,
			IsDeleted = false,
			AuthorId = author.Id,
			Author = author,
			ChildId = null
		});
		await _db.SaveChangesAsync();

		_model.Path = pageName;
		_model.FromRevision = 1;
		_model.ToRevision = 999; // Non-existent revision

		await _model.OnGet();

		Assert.AreEqual("", _model.Diff.LeftMarkup);
		Assert.AreEqual("", _model.Diff.RightMarkup);
	}

	[TestMethod]
	public async Task OnGet_WithLatestTrue_SetsFromAndToRevisions()
	{
		const string pageName = "TestPage";
		var author = _db.AddUser("TestUser").Entity;

		_db.WikiPages.AddRange(
			new WikiPage
			{
				Id = 1,
				PageName = pageName,
				Markup = "First revision",
				Revision = 1,
				IsDeleted = false,
				AuthorId = author.Id,
				Author = author,
				ChildId = 2
			},
			new WikiPage
			{
				Id = 2,
				PageName = pageName,
				Markup = "Second revision",
				Revision = 2,
				IsDeleted = false,
				AuthorId = author.Id,
				Author = author,
				ChildId = 3
			},
			new WikiPage
			{
				Id = 3,
				PageName = pageName,
				Markup = "Third revision",
				Revision = 3,
				IsDeleted = false,
				AuthorId = author.Id,
				Author = author,
				ChildId = null
			});
		await _db.SaveChangesAsync();

		_model.Path = pageName;
		_model.Latest = true;

		await _model.OnGet();

		// Should compare the two latest revisions (3 and 2)
		Assert.AreEqual(2, _model.FromRevision);
		Assert.AreEqual(3, _model.ToRevision);
		Assert.AreEqual("Second revision", _model.Diff.LeftMarkup);
		Assert.AreEqual("Third revision", _model.Diff.RightMarkup);
	}

	[TestMethod]
	public async Task OnGet_WithLatestTrueOnlyOneRevision_ComparesAgainstSelf()
	{
		const string pageName = "TestPage";
		var author = _db.AddUser("TestUser").Entity;

		_db.WikiPages.Add(new WikiPage
		{
			Id = 1,
			PageName = pageName,
			Markup = "Only revision",
			Revision = 1,
			IsDeleted = false,
			AuthorId = author.Id,
			Author = author
		});
		await _db.SaveChangesAsync();

		_model.Path = pageName;
		_model.Latest = true;

		await _model.OnGet();

		// Should compare revision 1 against itself
		Assert.AreEqual(1, _model.FromRevision);
		Assert.AreEqual(1, _model.ToRevision);
		Assert.AreEqual("Only revision", _model.Diff.LeftMarkup);
		Assert.AreEqual("Only revision", _model.Diff.RightMarkup);
	}

	[TestMethod]
	public async Task OnGet_WithLatestTrueNoRevisions_DoesNotSetRevisions()
	{
		_model.Path = "NonExistentPage";
		_model.Latest = true;

		await _model.OnGet();

		Assert.IsNull(_model.FromRevision);
		Assert.IsNull(_model.ToRevision);
		Assert.AreEqual("", _model.Diff.LeftMarkup);
		Assert.AreEqual("", _model.Diff.RightMarkup);
	}

	[TestMethod]
	public async Task OnGet_RevisionMessageAndMinorEditPreserved()
	{
		const string pageName = "TestPage";
		var author = _db.AddUser("TestUser").Entity;

		_db.WikiPages.AddRange(
			new WikiPage
			{
				Id = 1,
				PageName = pageName,
				Markup = "Content with message",
				Revision = 1,
				IsDeleted = false,
				AuthorId = author.Id,
				Author = author,
				ChildId = 2,
				MinorEdit = false,
				RevisionMessage = "Added important content"
			},
			new WikiPage
			{
				Id = 2,
				PageName = pageName,
				Markup = "Content minor change",
				Revision = 2,
				IsDeleted = false,
				AuthorId = author.Id,
				Author = author,
				ChildId = null,
				MinorEdit = true,
				RevisionMessage = null
			});
		await _db.SaveChangesAsync();

		_model.Path = pageName;

		await _model.OnGet();

		Assert.AreEqual(2, _model.Revisions.Count);
		Assert.AreEqual("Added important content", _model.Revisions[0].RevisionMessage);
		Assert.IsNull(_model.Revisions[1].RevisionMessage);
		Assert.IsFalse(_model.Revisions[0].MinorEdit);
		Assert.IsTrue(_model.Revisions[1].MinorEdit);
	}

	[TestMethod]
	public async Task OnGet_AuthorUserNameCorrectlyMapped()
	{
		const string pageName = "TestPage";
		const string authorName = "TestAuthor";
		var author = _db.AddUser(authorName).Entity;

		_db.WikiPages.Add(new WikiPage
		{
			Id = 1,
			PageName = pageName,
			Markup = "Test content",
			Revision = 1,
			IsDeleted = false,
			AuthorId = author.Id,
			Author = author
		});
		await _db.SaveChangesAsync();

		_model.Path = pageName;

		await _model.OnGet();

		Assert.AreEqual(1, _model.Revisions.Count);
		Assert.AreEqual(authorName, _model.Revisions[0].CreateUserName);
	}
}
