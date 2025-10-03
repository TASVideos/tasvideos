using TASVideos.Pages.Wiki;

namespace TASVideos.RazorPages.Tests.Pages.Wiki;

[TestClass]
public class RenderDeletedModelTests : BasePageModelTests
{
	private readonly RenderDeletedModel _model;

	public RenderDeletedModelTests()
	{
		_model = new RenderDeletedModel(_db)
		{
			PageContext = TestPageContext()
		};
	}

	[TestMethod]
	[DataRow(null)]
	[DataRow("")]
	[DataRow(" ")]
	public async Task OnGet_NullOrEmptyUrl_ReturnsNotFound(string? url)
	{
		var result = await _model.OnGet(url);

		Assert.IsNotNull(result);
		Assert.IsInstanceOfType(result, typeof(NotFoundResult));
	}

	[TestMethod]
	public async Task OnGet_NoDeletedPagesFound_ReturnsNotFound()
	{
		const string url = "NonExistentPage";

		var result = await _model.OnGet(url);

		Assert.IsNotNull(result);
		Assert.IsInstanceOfType(result, typeof(NotFoundResult));
		Assert.AreEqual(0, _model.WikiPages.Count);
	}

	[TestMethod]
	public async Task OnGet_DeletedPageExists_ReturnsPageResult()
	{
		const string pageName = "TestPage";
		var author = _db.AddUser("TestUser").Entity;
		_db.WikiPages.Add(new WikiPage
		{
			Id = 1,
			PageName = pageName,
			Markup = "Test content",
			Revision = 1,
			IsDeleted = true,
			Author = author
		});
		await _db.SaveChangesAsync();

		var result = await _model.OnGet(pageName);

		Assert.IsNotNull(result);
		Assert.IsInstanceOfType(result, typeof(PageResult));
		Assert.AreEqual(1, _model.WikiPages.Count);
		Assert.AreEqual(pageName, _model.WikiPages[0].PageName);
		Assert.IsTrue(_model.WikiPages[0].IsDeleted);
	}

	[TestMethod]
	public async Task OnGet_SpecificRevision_ReturnsCorrectRevision()
	{
		const string pageName = "TestPage";
		const int targetRevision = 2;
		var author = _db.AddUser("TestUser").Entity;

		var deletedPage1 = new WikiPage
		{
			Id = 1,
			PageName = pageName,
			Markup = "First revision",
			Revision = 1,
			IsDeleted = true,
			AuthorId = author.Id,
			Author = author,
			ChildId = 2
		};

		var deletedPage2 = new WikiPage
		{
			Id = 2,
			PageName = pageName,
			Markup = "Second revision",
			Revision = targetRevision,
			IsDeleted = true,
			AuthorId = author.Id,
			Author = author,
			ChildId = null
		};

		_db.WikiPages.AddRange(deletedPage1, deletedPage2);
		await _db.SaveChangesAsync();

		var result = await _model.OnGet(pageName, targetRevision);

		Assert.IsNotNull(result);
		Assert.IsInstanceOfType(result, typeof(PageResult));
		Assert.AreEqual(1, _model.WikiPages.Count);
		Assert.AreEqual(targetRevision, _model.WikiPages[0].Revision);
		Assert.AreEqual("Second revision", _model.WikiPages[0].Markup);
	}

	[TestMethod]
	public async Task OnGet_NoRevisionSpecified_ReturnsCurrentDeletedRevision()
	{
		const string pageName = "TestPage";
		var author = _db.AddUser("TestUser").Entity;

		var olderDeletedPage = new WikiPage
		{
			Id = 1,
			PageName = pageName,
			Markup = "Older revision",
			Revision = 1,
			IsDeleted = true,
			AuthorId = author.Id,
			Author = author,
			ChildId = 2
		};

		var currentDeletedPage = new WikiPage
		{
			Id = 2,
			PageName = pageName,
			Markup = "Current revision",
			Revision = 2,
			IsDeleted = true,
			AuthorId = author.Id,
			Author = author,
			ChildId = null
		};

		_db.WikiPages.AddRange(olderDeletedPage, currentDeletedPage);
		await _db.SaveChangesAsync();

		var result = await _model.OnGet(pageName);

		Assert.IsNotNull(result);
		Assert.IsInstanceOfType(result, typeof(PageResult));
		Assert.AreEqual(1, _model.WikiPages.Count);
		Assert.AreEqual(2, _model.WikiPages[0].Revision);
		Assert.AreEqual("Current revision", _model.WikiPages[0].Markup);
	}

	[TestMethod]
	public async Task OnGet_NonDeletedPage_ReturnsNotFound()
	{
		const string pageName = "ActivePage";
		var author = _db.AddUser("TestUser").Entity;
		_db.WikiPages.Add(new WikiPage
		{
			Id = 1,
			PageName = pageName,
			Markup = "Active content",
			Revision = 1,
			IsDeleted = false,
			AuthorId = author.Id,
			Author = author
		});
		await _db.SaveChangesAsync();

		var result = await _model.OnGet(pageName);

		Assert.IsNotNull(result);
		Assert.IsInstanceOfType(result, typeof(NotFoundResult));
		Assert.AreEqual(0, _model.WikiPages.Count);
	}

	[TestMethod]
	public async Task OnGet_SpecificRevisionNotFound_ReturnsNotFound()
	{
		const string pageName = "TestPage";
		const int nonExistentRevision = 999;
		var author = _db.AddUser("TestUser").Entity;
		_db.WikiPages.Add(new WikiPage
		{
			Id = 1,
			PageName = pageName,
			Markup = "Test content",
			Revision = 1,
			IsDeleted = true,
			AuthorId = author.Id,
			Author = author
		});
		await _db.SaveChangesAsync();

		var result = await _model.OnGet(pageName, nonExistentRevision);

		Assert.IsNotNull(result);
		Assert.IsInstanceOfType(result, typeof(NotFoundResult));
		Assert.AreEqual(0, _model.WikiPages.Count);
	}

	[TestMethod]
	public async Task OnGet_AuthorIncluded_LoadsAuthorInformation()
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
			IsDeleted = true,
			AuthorId = author.Id,
			Author = author
		});
		await _db.SaveChangesAsync();

		var result = await _model.OnGet(pageName);

		Assert.IsNotNull(result);
		Assert.IsInstanceOfType(result, typeof(PageResult));
		Assert.AreEqual(1, _model.WikiPages.Count);
		Assert.IsNotNull(_model.WikiPages[0].Author);
		Assert.AreEqual(authorName, _model.WikiPages[0].Author!.UserName);
	}
}
