using TASVideos.Core.Services.Wiki;
using TASVideos.Pages.Wiki;
using TASVideos.Services;
using TASVideos.Tests.Base;

namespace TASVideos.RazorPages.Tests.Pages.Wiki;

[TestClass]
public class MoveModelTests : TestDbBase
{
	private readonly IWikiPages _wikiPages;
	private readonly MoveModel _model;

	public MoveModelTests()
	{
		_wikiPages = Substitute.For<IWikiPages>();
		var publisher = Substitute.For<IExternalMediaPublisher>();
		_model = new MoveModel(_wikiPages, publisher);
	}

	#region OnGet Tests

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
	public async Task OnGet_PathDoesNotExist_ReturnsNotFound()
	{
		_model.Path = "NonExistentPage";
		_wikiPages.Exists("NonExistentPage").Returns(false);

		var result = await _model.OnGet();

		Assert.IsInstanceOfType<NotFoundResult>(result);
	}

	[TestMethod]
	public async Task OnGet_ValidPath_SetsOriginalAndDestinationPageName()
	{
		_model.Path = "TestPage";
		_wikiPages.Exists("TestPage").Returns(true);

		var result = await _model.OnGet();

		Assert.IsInstanceOfType<PageResult>(result);
		Assert.AreEqual("TestPage", _model.OriginalPageName);
		Assert.AreEqual("TestPage", _model.DestinationPageName);
	}

	[TestMethod]
	public async Task OnGet_PathWithSlashes_TrimsSlashesAndSetsProperties()
	{
		_model.Path = "/TestPage/";
		_wikiPages.Exists("TestPage").Returns(true);

		var result = await _model.OnGet();

		Assert.IsInstanceOfType<PageResult>(result);
		Assert.AreEqual("TestPage", _model.OriginalPageName);
		Assert.AreEqual("TestPage", _model.DestinationPageName);
	}

	#endregion

	#region OnPost Tests

	[TestMethod]
	public async Task OnPost_InvalidModelState_ReturnsPage()
	{
		_model.OriginalPageName = "TestPage";
		_model.DestinationPageName = "NewPage";
		_model.ModelState.AddModelError("test", "Test error");

		var result = await _model.OnPost();

		Assert.IsInstanceOfType<PageResult>(result);
		Assert.IsFalse(_model.ModelState.IsValid);
	}

	[TestMethod]
	public async Task OnPost_DestinationPageExists_AddsModelErrorAndReturnsPage()
	{
		_model.OriginalPageName = "TestPage";
		_model.DestinationPageName = "ExistingPage";
		_wikiPages.Exists("ExistingPage", includeDeleted: true).Returns(true);

		var result = await _model.OnPost();

		Assert.IsInstanceOfType<PageResult>(result);
		Assert.IsFalse(_model.ModelState.IsValid);
		Assert.IsNotNull(_model.ModelState["DestinationPageName"]);
		Assert.IsTrue(_model.ModelState["DestinationPageName"]!.Errors.Any(e => e.ErrorMessage.Contains("already exists")));
		await _wikiPages.DidNotReceive().Move(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>());
	}

	[TestMethod]
	public async Task OnPost_PageNamesWithSlashes_TrimsSlashesBeforeProcessing()
	{
		var user = _db.AddUser("TestUser").Entity;
		await _db.SaveChangesAsync();
		AddAuthenticatedUser(_model, user, [PermissionTo.MoveWikiPages]);
		_model.OriginalPageName = "/TestPage/";
		_model.DestinationPageName = "/NewPage/";
		_wikiPages.Exists("NewPage", includeDeleted: true).Returns(false);
		_wikiPages.Move("TestPage", "NewPage", user.Id).Returns(true);

		var result = await _model.OnPost();

		Assert.IsInstanceOfType<RedirectResult>(result);
		await _wikiPages.Received(1).Move("TestPage", "NewPage", user.Id);
	}

	[TestMethod]
	public async Task OnPost_MoveSucceeds_CallsWikiMoveAndRedirectsToDestination()
	{
		var user = _db.AddUser("TestUser").Entity;
		await _db.SaveChangesAsync();
		AddAuthenticatedUser(_model, user, [PermissionTo.MoveWikiPages]);
		_model.OriginalPageName = "OriginalPage";
		_model.DestinationPageName = "DestinationPage";
		_wikiPages.Exists("DestinationPage", includeDeleted: true).Returns(false);
		_wikiPages.Move("OriginalPage", "DestinationPage", user.Id).Returns(true);

		var result = await _model.OnPost();

		Assert.IsInstanceOfType<RedirectResult>(result);
		var redirect = (RedirectResult)result;
		Assert.AreEqual("/DestinationPage", redirect.Url);

		await _wikiPages.Received(1).Move("OriginalPage", "DestinationPage", user.Id);
	}

	[TestMethod]
	public async Task OnPost_MoveFails_AddsModelErrorAndReturnsPage()
	{
		var user = _db.AddUser("TestUser").Entity;
		await _db.SaveChangesAsync();
		AddAuthenticatedUser(_model, user, [PermissionTo.MoveWikiPages]);
		_model.OriginalPageName = "TestPage";
		_model.DestinationPageName = "NewPage";
		_wikiPages.Exists("NewPage", includeDeleted: true).Returns(false);
		_wikiPages.Move("TestPage", "NewPage", user.Id).Returns(false);

		var result = await _model.OnPost();

		Assert.IsInstanceOfType<PageResult>(result);
		Assert.IsFalse(_model.ModelState.IsValid);
		Assert.IsNotNull(_model.ModelState[""]);
		Assert.IsTrue(_model.ModelState[""]!.Errors.Any(e => e.ErrorMessage.Contains("Unable to move page")));
	}

	[TestMethod]
	public void RequiresPermission() => AssertHasPermission(typeof(MoveModel), PermissionTo.MoveWikiPages);

	#endregion
}
