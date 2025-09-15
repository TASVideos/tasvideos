using TASVideos.Core.Services;
using TASVideos.Core.Services.Wiki;
using TASVideos.Pages.Wiki;
using TASVideos.Services;
using TASVideos.Tests.Base;

namespace TASVideos.RazorPages.Tests.Pages.Wiki;

[TestClass]
public class EditModelTests : TestDbBase
{
	private readonly IWikiPages _wikiPages;
	private readonly IUserManager _userManager;
	private readonly EditModel _model;

	public EditModelTests()
	{
		_wikiPages = Substitute.For<IWikiPages>();
		_userManager = Substitute.For<IUserManager>();
		var publisher = Substitute.For<IExternalMediaPublisher>();
		_model = new EditModel(_wikiPages, _userManager, publisher);
	}

	[TestMethod]
	[DataRow(null)]
	[DataRow("")]
	[DataRow("   ")]
	public async Task OnGet_NullOrEmptyPath_ReturnsNotFound(string? path)
	{
		_model.Path = path;
		var result = await _model.OnGet();
		Assert.IsInstanceOfType<NotFoundResult>(result);
	}

	[TestMethod]
	public async Task OnGet_InvalidWikiPageName_ReturnsNotFound()
	{
		_model.Path = "Invalid/Page.html";
		var result = await _model.OnGet();
		Assert.IsInstanceOfType<NotFoundResult>(result);
	}

	[TestMethod]
	public async Task OnGet_HomePageWithoutExistingUser_ReturnsNotFound()
	{
		_model.Path = "HomePages/NonExistentUser";
		var result = await _model.OnGet();
		Assert.IsInstanceOfType<NotFoundResult>(result);
	}

	[TestMethod]
	public async Task OnGet_HomePageWithExistingUser_ReturnsPage()
	{
		_model.Path = "HomePages/TestUser";
		var wikiPage = new WikiResult { Markup = "Test markup" };
		_wikiPages.Page("HomePages/TestUser").Returns(wikiPage);
		_userManager.GetUserNameByUserName("TestUser").Returns("TestUser");

		var result = await _model.OnGet();

		Assert.IsInstanceOfType<PageResult>(result);
		Assert.AreEqual("Test markup", _model.Markup);
	}

	[TestMethod]
	public async Task OnGet_ExistingWikiPage_LoadsMarkup()
	{
		_model.Path = "TestPage";
		var wikiPage = new WikiResult { Markup = "Existing page markup" };
		_wikiPages.Page("TestPage").Returns(wikiPage);

		var result = await _model.OnGet();

		Assert.IsInstanceOfType<PageResult>(result);
		Assert.AreEqual("Existing page markup", _model.Markup);
	}

	[TestMethod]
	public async Task OnGet_NonExistentWikiPage_EmptyMarkup()
	{
		_model.Path = "NonExistentPage";
		_wikiPages.Page("NonExistentPage").Returns((IWikiPage?)null);

		var result = await _model.OnGet();

		Assert.IsInstanceOfType<PageResult>(result);
		Assert.AreEqual("", _model.Markup);
	}

	[TestMethod]
	public async Task OnGet_PathWithSlashes_TrimsSlashes()
	{
		_model.Path = "/TestPage/";
		var wikiPage = new WikiResult { Markup = "Test content" };
		_wikiPages.Page("TestPage").Returns(wikiPage);

		var result = await _model.OnGet();

		Assert.IsInstanceOfType<PageResult>(result);
		Assert.AreEqual("TestPage", _model.Path);
	}

	[TestMethod]
	[DataRow(null)]
	[DataRow("")]
	[DataRow("   ")]
	public async Task OnPost_NullOrEmptyPath_ReturnsNotFound(string? path)
	{
		_model.Path = path;
		_model.Markup = "Test markup";

		var result = await _model.OnPost();

		Assert.IsInstanceOfType<NotFoundResult>(result);
	}

	[TestMethod]
	public async Task OnPost_InvalidWikiPageName_ReturnsHome()
	{
		_model.Path = "Invalid/PageName.html";
		_model.Markup = "Test markup";

		var result = await _model.OnPost();

		AssertRedirectHome(result);
	}

	[TestMethod]
	public async Task OnPost_HomePageWithoutExistingUser_DoesNotSaveAndReturnsHome()
	{
		_model.Path = "HomePages/NonExistentUser";
		_model.Markup = "Test markup";

		var result = await _model.OnPost();

		AssertRedirectHome(result);
		await _wikiPages.DidNotReceive().Add(Arg.Any<WikiCreateRequest>());
	}

	[TestMethod]
	public async Task OnPost_HomePageWithExistingUser_NormalizesUserName()
	{
		_model.Path = "HomePages/testuser"; // lowercase
		_model.Markup = "Test markup";
		_model.EditComments = "Test edit";

		var authenticatedUser = _db.AddUser("AuthenticatedUser").Entity;
		AddAuthenticatedUser(_model, authenticatedUser, [PermissionTo.EditHomePage]);
		_userManager.GetUserNameByUserName("testuser").Returns("TestUser");

		var wikiResult = new WikiResult
		{
			PageName = "HomePages/TestUser",
			Revision = 1
		};
		_wikiPages.Add(Arg.Any<WikiCreateRequest>()).Returns(wikiResult);

		var result = await _model.OnPost();

		Assert.IsInstanceOfType<RedirectResult>(result);
		Assert.AreEqual("HomePages/TestUser", _model.Path); // Should be normalized
	}

	[TestMethod]
	public async Task OnPost_InvalidModelState_ReturnsPage()
	{
		_model.Path = "TestPage";
		_model.Markup = "Test markup";
		_model.ModelState.AddModelError("test", "Test error");

		var result = await _model.OnPost();

		Assert.IsInstanceOfType<PageResult>(result);
		Assert.IsFalse(_model.ModelState.IsValid);
	}

	[TestMethod]
	public async Task OnPost_ValidEdit_CreatesWikiPageAndRedirects()
	{
		var user = _db.AddUser("TestUser").Entity;
		AddAuthenticatedUser(_model, user, [PermissionTo.EditWikiPages]);
		_model.Path = "TestPage";
		_model.Markup = "Test markup content";
		_model.EditComments = "Initial creation";
		_model.EditStart = DateTime.UtcNow.AddMinutes(-5);
		_wikiPages.Add(Arg.Any<WikiCreateRequest>()).Returns(new WikiResult
		{
			PageName = "TestPage",
			Revision = 1
		});

		var result = await _model.OnPost();

		Assert.IsInstanceOfType<RedirectResult>(result);
		var redirect = (RedirectResult)result;
		Assert.AreEqual("/TestPage", redirect.Url);

		await _wikiPages.Received(1).Add(Arg.Is<WikiCreateRequest>(req =>
			req.PageName == _model.Path
				&& req.Markup == _model.Markup
				&& req.RevisionMessage == _model.EditComments
				&& req.AuthorId == user.Id
				&& req.CreateTimestamp == _model.EditStart));
	}

	[TestMethod]
	public async Task OnPost_WikiPageAddFails_AddsModelErrorAndReturnsPage()
	{
		var user = _db.AddUser("TestUser").Entity;
		AddAuthenticatedUser(_model, user, [PermissionTo.EditWikiPages]);
		_model.Path = "TestPage";
		_model.Markup = "Test markup";
		_wikiPages.Add(Arg.Any<WikiCreateRequest>()).Returns((IWikiPage?)null);

		var result = await _model.OnPost();

		Assert.IsInstanceOfType<PageResult>(result);
		Assert.IsFalse(_model.ModelState.IsValid);
		Assert.IsNotNull(_model.ModelState[""]);
		Assert.IsTrue(_model.ModelState[""]!.Errors.Any(e => e.ErrorMessage.Contains("Unable to save")));
	}

	[TestMethod]
	[DataRow(null)]
	[DataRow("")]
	[DataRow(" ")]
	public async Task OnPostRollbackLatest_NoPath_ReturnsNotFound(string path)
	{
		_model.Path = path;
		var result = await _model.OnPostRollbackLatest();

		Assert.IsInstanceOfType<NotFoundResult>(result);
	}

	[TestMethod]
	public async Task OnPostRollbackLatest_FirstRevision_ReturnsBadRequest()
	{
		var user = _db.AddUser("TestUser").Entity;
		AddAuthenticatedUser(_model, user, [PermissionTo.EditWikiPages]);
		_model.Path = "TestPage";
		_wikiPages.RollbackLatest(_model.Path, user.Id).Returns((IWikiPage?)null);
		_wikiPages.Page(_model.Path).Returns(new WikiResult { Revision = 1 });

		var result = await _model.OnPostRollbackLatest();

		Assert.IsInstanceOfType<BadRequestObjectResult>(result);
		var badRequest = (BadRequestObjectResult)result;
		Assert.IsTrue(badRequest.Value!.ToString()!.Contains("Cannot rollback"));
	}

	[TestMethod]
	public async Task OnPostRollbackLatest_UnableToRollback_ReturnsNotFound()
	{
		var user = _db.AddUser("TestUser").Entity;
		AddAuthenticatedUser(_model, user, [PermissionTo.EditWikiPages]);
		_model.Path = "TestPage";
		_wikiPages.RollbackLatest(_model.Path, user.Id).Returns((IWikiPage?)null);

		var result = await _model.OnPostRollbackLatest();

		Assert.IsInstanceOfType<NotFoundResult>(result);
	}

	[TestMethod]
	public async Task OnPostRollbackLatest_ValidRollback_CreatesRollbackRevisionAndRedirects()
	{
		var user = _db.AddUser("TestUser").Entity;
		AddAuthenticatedUser(_model, user, [PermissionTo.EditWikiPages]);
		_model.Path = "TestPage";
		_wikiPages.RollbackLatest(_model.Path, user.Id).Returns(new WikiResult());

		var result = await _model.OnPostRollbackLatest();

		Assert.IsInstanceOfType<RedirectToPageResult>(result);
		var redirect = (RedirectToPageResult)result;
		Assert.AreEqual("PageHistory", redirect.PageName);
		Assert.AreEqual("TestPage", redirect.RouteValues!["Path"]);
		Assert.IsTrue((bool?)redirect.RouteValues["Latest"]);
	}
}
