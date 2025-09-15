using TASVideos.Core.Services;
using TASVideos.Pages.Forum.Posts;

namespace TASVideos.RazorPages.Tests.Pages.Forum.Posts;

[TestClass]
public class IndexModelTests : BasePageModelTests
{
	private readonly IForumService _forumService;
	private readonly IndexModel _model;

	public IndexModelTests()
	{
		_forumService = Substitute.For<IForumService>();
		_model = new IndexModel(_forumService)
		{
			PageContext = TestPageContext()
		};
	}

	[TestMethod]
	public async Task OnGet_PostDoesNotExist_ReturnsNotFound()
	{
		_model.Id = 999;
		_forumService.GetPostPosition(999, Arg.Any<bool>()).Returns((PostPosition?)null);

		var result = await _model.OnGet();

		AssertForumNotFound(result);
	}

	[TestMethod]
	public async Task OnGet_PostExists_RedirectsToTopicWithCorrectParameters()
	{
		_model.Id = 123;
		var postPosition = new PostPosition(Page: 5, TopicId: 456);
		_forumService.GetPostPosition(123, Arg.Any<bool>()).Returns(postPosition);

		var result = await _model.OnGet();

		var redirectResult = (RedirectToPageResult)result;
		Assert.IsNull(redirectResult.PageHandler);

		var routeValues = redirectResult.RouteValues;
		Assert.IsNotNull(routeValues);
		Assert.AreEqual(456, routeValues["Id"]);
		Assert.AreEqual(5, routeValues["CurrentPage"]);
		Assert.AreEqual(123, routeValues["Highlight"]);

		Assert.AreEqual("123", redirectResult.Fragment);
	}

	[TestMethod]
	public async Task OnGet_UserWithRestrictedPermission_PassesTrueToForumService()
	{
		_model.Id = 123;
		var user = _db.AddUserWithRole("RestrictedUser").Entity;
		await _db.SaveChangesAsync();

		AddAuthenticatedUser(_model, user, [PermissionTo.SeeRestrictedForums]);
		var postPosition = new PostPosition(Page: 1, TopicId: 789);
		_forumService.GetPostPosition(123, true).Returns(postPosition);

		var result = await _model.OnGet();

		await _forumService.Received(1).GetPostPosition(123, true);
		AssertRedirect(result, "/Forum/Topics/Index");
	}

	[TestMethod]
	public async Task OnGet_UserWithoutRestrictedPermission_PassesFalseToForumService()
	{
		_model.Id = 123;
		var user = _db.AddUserWithRole("RegularUser").Entity;
		await _db.SaveChangesAsync();

		AddAuthenticatedUser(_model, user, []);
		var postPosition = new PostPosition(Page: 1, TopicId: 789);
		_forumService.GetPostPosition(123, false).Returns(postPosition);

		var result = await _model.OnGet();

		await _forumService.Received(1).GetPostPosition(123, false);
		AssertRedirect(result, "/Forum/Topics/Index");
	}

	[TestMethod]
	public async Task OnGet_AnonymousUser_PassesFalseToForumService()
	{
		_model.Id = 123;
		var postPosition = new PostPosition(Page: 1, TopicId: 789);
		_forumService.GetPostPosition(123, false).Returns(postPosition);

		var result = await _model.OnGet();

		await _forumService.Received(1).GetPostPosition(123, false);
		AssertRedirect(result, "/Forum/Topics/Index");
	}

	[TestMethod]
	public async Task OnGet_PostAtFirstPage_RedirectsWithPageOne()
	{
		_model.Id = 456;
		var postPosition = new PostPosition(Page: 1, TopicId: 999);
		_forumService.GetPostPosition(456, Arg.Any<bool>()).Returns(postPosition);

		var result = await _model.OnGet();

		var redirectResult = (RedirectToPageResult)result;
		Assert.AreEqual(1, redirectResult.RouteValues!["CurrentPage"]);
		Assert.AreEqual(999, redirectResult.RouteValues["Id"]);
		Assert.AreEqual(456, redirectResult.RouteValues["Highlight"]);
	}

	[TestMethod]
	public async Task OnGet_PostAtLastPage_RedirectsWithCorrectPageNumber()
	{
		_model.Id = 789;
		var postPosition = new PostPosition(Page: 25, TopicId: 111);
		_forumService.GetPostPosition(789, Arg.Any<bool>()).Returns(postPosition);

		var result = await _model.OnGet();

		var redirectResult = (RedirectToPageResult)result;
		Assert.AreEqual(25, redirectResult.RouteValues!["CurrentPage"]);
		Assert.AreEqual(111, redirectResult.RouteValues["Id"]);
		Assert.AreEqual(789, redirectResult.RouteValues["Highlight"]);
	}

	[TestMethod]
	public async Task OnGet_DifferentPostIds_MaintainsCorrectIdInFragmentAndHighlight()
	{
		const int postId = 987654;
		_model.Id = postId;
		var postPosition = new PostPosition(Page: 10, TopicId: 555);
		_forumService.GetPostPosition(postId, Arg.Any<bool>()).Returns(postPosition);

		var result = await _model.OnGet();

		var redirectResult = (RedirectToPageResult)result;
		Assert.AreEqual(postId.ToString(), redirectResult.Fragment);
		Assert.AreEqual(postId, redirectResult.RouteValues!["Highlight"]);
	}

	[TestMethod]
	public void AllowsAnonymousAttribute() => AssertAllowsAnonymousUsers(typeof(IndexModel));
}
