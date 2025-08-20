using TASVideos.Pages.Forum.Legacy;

namespace TASVideos.RazorPages.Tests.Pages.Forum.Legacy;

[TestClass]
public class ForumModelTests : BasePageModelTests
{
	private readonly ForumModel _model = new()
	{
		PageContext = TestPageContext()
	};

	[TestMethod]
	public void OnGet_BothFAndIdNull_ReturnsNotFound()
	{
		_model.F = null;
		_model.Id = null;

		var result = _model.OnGet();

		Assert.IsInstanceOfType(result, typeof(NotFoundResult));
	}

	[TestMethod]
	public void OnGet_FHasValue_RedirectsToSubforumWithFValue()
	{
		const int forumId = 42;
		_model.F = forumId;
		_model.Id = null;

		var result = _model.OnGet();

		Assert.IsInstanceOfType(result, typeof(RedirectToPageResult));
		var redirectResult = (RedirectToPageResult)result;
		Assert.AreEqual("/Forum/Subforum/Index", redirectResult.PageName);
		Assert.IsNotNull(redirectResult.RouteValues);
		Assert.AreEqual(forumId, redirectResult.RouteValues["Id"]);
	}

	[TestMethod]
	public void OnGet_IdHasValue_RedirectsToSubforumWithIdValue()
	{
		const int forumId = 123;
		_model.F = null;
		_model.Id = forumId;

		var result = _model.OnGet();

		Assert.IsInstanceOfType(result, typeof(RedirectToPageResult));
		var redirectResult = (RedirectToPageResult)result;
		Assert.AreEqual("/Forum/Subforum/Index", redirectResult.PageName);
		Assert.IsNotNull(redirectResult.RouteValues);
		Assert.AreEqual(forumId, redirectResult.RouteValues["Id"]);
	}

	[TestMethod]
	public void OnGet_BothFAndIdHaveValues_PrioritizesFOverId()
	{
		const int fValue = 99;
		const int idValue = 88;
		_model.F = fValue;
		_model.Id = idValue;

		var result = _model.OnGet();

		Assert.IsInstanceOfType(result, typeof(RedirectToPageResult));
		var redirectResult = (RedirectToPageResult)result;
		Assert.AreEqual("/Forum/Subforum/Index", redirectResult.PageName);
		Assert.IsNotNull(redirectResult.RouteValues);
		Assert.AreEqual(fValue, redirectResult.RouteValues["Id"]);
	}

	[TestMethod]
	public void OnGet_FIsZero_RedirectsToSubforumWithZero()
	{
		_model.F = 0;
		_model.Id = null;

		var result = _model.OnGet();

		Assert.IsInstanceOfType(result, typeof(RedirectToPageResult));
		var redirectResult = (RedirectToPageResult)result;
		Assert.AreEqual("/Forum/Subforum/Index", redirectResult.PageName);
		Assert.IsNotNull(redirectResult.RouteValues);
		Assert.AreEqual(0, redirectResult.RouteValues["Id"]);
	}

	[TestMethod]
	public void OnGet_IdIsZero_RedirectsToSubforumWithZero()
	{
		_model.F = null;
		_model.Id = 0;

		var result = _model.OnGet();

		Assert.IsInstanceOfType(result, typeof(RedirectToPageResult));
		var redirectResult = (RedirectToPageResult)result;
		Assert.AreEqual("/Forum/Subforum/Index", redirectResult.PageName);
		Assert.IsNotNull(redirectResult.RouteValues);
		Assert.AreEqual(0, redirectResult.RouteValues["Id"]);
	}
}
