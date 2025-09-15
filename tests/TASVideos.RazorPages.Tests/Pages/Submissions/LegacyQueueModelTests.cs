using TASVideos.Pages.Submissions;
using TASVideos.Tests.Base;

namespace TASVideos.RazorPages.Tests.Pages.Submissions;

[TestClass]
public class LegacyQueueModelTests : TestDbBase
{
	private readonly LegacyQueueModel _page = new();

	[TestMethod]
	public void OnGet_ModeSubmit_RedirectsToSubmissionsSubmit()
	{
		_page.Mode = "submit";

		var result = _page.OnGet();

		Assert.IsInstanceOfType<RedirectToPageResult>(result);
		var redirect = (RedirectToPageResult)result;
		Assert.AreEqual("/Submissions/Submit", redirect.PageName);
	}

	[TestMethod]
	public void OnGet_ModeSubmitCaseInsensitive_RedirectsToSubmissionsSubmit()
	{
		_page.Mode = "SUBMIT";

		var result = _page.OnGet();

		Assert.IsInstanceOfType<RedirectToPageResult>(result);
		var redirect = (RedirectToPageResult)result;
		Assert.AreEqual("/Submissions/Submit", redirect.PageName);
	}

	[TestMethod]
	public void OnGet_ModeList_RedirectsToSubmissionsIndex()
	{
		_page.Mode = "list";

		var result = _page.OnGet();

		Assert.IsInstanceOfType<RedirectToPageResult>(result);
		var redirect = (RedirectToPageResult)result;
		Assert.AreEqual("/Submissions/Index", redirect.PageName);
		Assert.IsNull(redirect.RouteValues!["User"]);
	}

	[TestMethod]
	public void OnGet_ModeListTypeOwn_WithLoggedInUser_RedirectsToSubmissionsIndexWithUser()
	{
		_page.Mode = "list";
		_page.Type = "own";

		var user = _db.AddUser("TestUser").Entity;
		AddAuthenticatedUser(_page, user, []);

		var result = _page.OnGet();

		Assert.IsInstanceOfType<RedirectToPageResult>(result);
		var redirect = (RedirectToPageResult)result;
		Assert.AreEqual("/Submissions/Index", redirect.PageName);
		Assert.AreEqual("TestUser", redirect.RouteValues!["User"]);
	}

	[TestMethod]
	public void OnGet_ModeListTypeOwn_WithoutLoggedInUser_RedirectsToSubmissionsIndexWithoutUser()
	{
		_page.Mode = "list";
		_page.Type = "own";

		var result = _page.OnGet();

		Assert.IsInstanceOfType<RedirectToPageResult>(result);
		var redirect = (RedirectToPageResult)result;
		Assert.AreEqual("/Submissions/Index", redirect.PageName);
		Assert.IsNull(redirect.RouteValues!["User"]);
	}

	[TestMethod]
	public void OnGet_ModeListCaseInsensitive_RedirectsToSubmissionsIndex()
	{
		_page.Mode = "LIST";

		var result = _page.OnGet();

		Assert.IsInstanceOfType<RedirectToPageResult>(result);
		var redirect = (RedirectToPageResult)result;
		Assert.AreEqual("/Submissions/Index", redirect.PageName);
	}

	[TestMethod]
	public void OnGet_ModeEditWithId_RedirectsToSubmissionsEdit()
	{
		_page.Mode = "edit";
		_page.Id = 123;

		var result = _page.OnGet();

		Assert.IsInstanceOfType<RedirectToPageResult>(result);
		var redirect = (RedirectToPageResult)result;
		Assert.AreEqual("/Submissions/Edit", redirect.PageName);
		Assert.AreEqual(123, redirect.RouteValues!["Id"]);
	}

	[TestMethod]
	public void OnGet_ModeEditWithoutId_RedirectsToSubmissionsIndex()
	{
		_page.Mode = "edit";
		_page.Id = null;

		var result = _page.OnGet();

		Assert.IsInstanceOfType<RedirectToPageResult>(result);
		var redirect = (RedirectToPageResult)result;
		Assert.AreEqual("/Submissions/Index", redirect.PageName);
	}

	[TestMethod]
	public void OnGet_ModeEditCaseInsensitive_RedirectsToSubmissionsEdit()
	{
		_page.Mode = "EDIT";
		_page.Id = 456;

		var result = _page.OnGet();

		Assert.IsInstanceOfType<RedirectToPageResult>(result);
		var redirect = (RedirectToPageResult)result;
		Assert.AreEqual("/Submissions/Edit", redirect.PageName);
		Assert.AreEqual(456, redirect.RouteValues!["Id"]);
	}

	[TestMethod]
	public void OnGet_ModeViewWithId_RedirectsToSubmissionsView()
	{
		_page.Mode = "view";
		_page.Id = 789;

		var result = _page.OnGet();

		Assert.IsInstanceOfType<RedirectToPageResult>(result);
		var redirect = (RedirectToPageResult)result;
		Assert.AreEqual("/Submissions/View", redirect.PageName);
		Assert.AreEqual(789, redirect.RouteValues!["Id"]);
	}

	[TestMethod]
	public void OnGet_ModeViewWithoutId_RedirectsToSubmissionsIndex()
	{
		_page.Mode = "view";
		_page.Id = null;

		var result = _page.OnGet();

		Assert.IsInstanceOfType<RedirectToPageResult>(result);
		var redirect = (RedirectToPageResult)result;
		Assert.AreEqual("/Submissions/Index", redirect.PageName);
	}

	[TestMethod]
	public void OnGet_ModeViewCaseInsensitive_RedirectsToSubmissionsView()
	{
		_page.Mode = "VIEW";
		_page.Id = 321;

		var result = _page.OnGet();

		Assert.IsInstanceOfType<RedirectToPageResult>(result);
		var redirect = (RedirectToPageResult)result;
		Assert.AreEqual("/Submissions/View", redirect.PageName);
		Assert.AreEqual(321, redirect.RouteValues!["Id"]);
	}

	[TestMethod]
	public void OnGet_NoModeWithId_RedirectsToSubmissionsView()
	{
		_page.Mode = null;
		_page.Id = 654;

		var result = _page.OnGet();

		Assert.IsInstanceOfType<RedirectToPageResult>(result);
		var redirect = (RedirectToPageResult)result;
		Assert.AreEqual("/Submissions/View", redirect.PageName);
		Assert.AreEqual(654, redirect.RouteValues!["Id"]);
	}

	[TestMethod]
	public void OnGet_EmptyModeWithId_RedirectsToSubmissionsView()
	{
		_page.Mode = "";
		_page.Id = 987;

		var result = _page.OnGet();

		Assert.IsInstanceOfType<RedirectToPageResult>(result);
		var redirect = (RedirectToPageResult)result;
		Assert.AreEqual("/Submissions/View", redirect.PageName);
		Assert.AreEqual(987, redirect.RouteValues!["Id"]);
	}

	[TestMethod]
	public void OnGet_WhitespaceModeWithId_RedirectsToSubmissionsView()
	{
		_page.Mode = "   ";
		_page.Id = 111;

		var result = _page.OnGet();

		Assert.IsInstanceOfType<RedirectToPageResult>(result);
		var redirect = (RedirectToPageResult)result;
		Assert.AreEqual("/Submissions/View", redirect.PageName);
		Assert.AreEqual(111, redirect.RouteValues!["Id"]);
	}

	[TestMethod]
	public void OnGet_NoModeNoId_RedirectsToSubmissionsIndex()
	{
		_page.Mode = null;
		_page.Id = null;

		var result = _page.OnGet();

		Assert.IsInstanceOfType<RedirectToPageResult>(result);
		var redirect = (RedirectToPageResult)result;
		Assert.AreEqual("/Submissions/Index", redirect.PageName);
	}

	[TestMethod]
	public void OnGet_UnknownMode_RedirectsToSubmissionsIndex()
	{
		_page.Mode = "unknown";
		_page.Id = 222;

		var result = _page.OnGet();

		Assert.IsInstanceOfType<RedirectToPageResult>(result);
		var redirect = (RedirectToPageResult)result;
		Assert.AreEqual("/Submissions/Index", redirect.PageName);
	}

	[TestMethod]
	public void OnGet_UnknownModeNoId_RedirectsToSubmissionsIndex()
	{
		_page.Mode = "unknown";
		_page.Id = null;

		var result = _page.OnGet();

		Assert.IsInstanceOfType<RedirectToPageResult>(result);
		var redirect = (RedirectToPageResult)result;
		Assert.AreEqual("/Submissions/Index", redirect.PageName);
	}
}
