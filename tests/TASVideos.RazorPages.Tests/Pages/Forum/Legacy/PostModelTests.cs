using TASVideos.Core.Services;
using TASVideos.Pages.Forum.Posts;

namespace TASVideos.RazorPages.Tests.Pages.Forum.Legacy;

[TestClass]
public class PostModelTests : BasePageModelTests
{
	private readonly IForumService _forumService;
	private readonly IndexModel _model;

	public PostModelTests()
	{
		_forumService = Substitute.For<IForumService>();
		_model = new IndexModel(_forumService)
		{
			PageContext = TestPageContext()
		};
	}

	[TestMethod]
	public async Task LegacyPost_PostNotFound_ReturnsNotFound()
	{
		_model.Id = 999;
		_forumService.GetPostPosition(999, Arg.Any<bool>()).Returns((PostPosition?)null);

		var result = await _model.OnGet();

		AssertForumNotFound(result);
	}

	[TestMethod]
	public async Task LegacyPost_ValidPostId_RedirectsToCorrectTopic()
	{
		const int postId = 12345;
		const int topicId = 67890;
		const int pageNumber = 3;

		_model.Id = postId;
		var postPosition = new PostPosition(Page: pageNumber, TopicId: topicId);
		_forumService.GetPostPosition(postId, Arg.Any<bool>()).Returns(postPosition);

		var result = await _model.OnGet();

		AssertRedirect(result, "/Forum/Topics/Index");
		var redirectResult = (RedirectToPageResult)result;
		var routeValues = redirectResult.RouteValues;
		Assert.IsNotNull(routeValues);
		Assert.AreEqual(topicId, routeValues["Id"]);
		Assert.AreEqual(pageNumber, routeValues["CurrentPage"]);
		Assert.AreEqual(postId, routeValues["Highlight"]);
		Assert.AreEqual(postId.ToString(), redirectResult.Fragment);
	}

	[TestMethod]
	public async Task LegacyPost_PostOnFirstPage_RedirectsWithCorrectParameters()
	{
		const int postId = 555;
		const int topicId = 777;

		_model.Id = postId;
		var postPosition = new PostPosition(Page: 1, TopicId: topicId);
		_forumService.GetPostPosition(postId, Arg.Any<bool>()).Returns(postPosition);

		var result = await _model.OnGet();

		var redirectResult = (RedirectToPageResult)result;
		Assert.AreEqual(1, redirectResult.RouteValues!["CurrentPage"]);
		Assert.AreEqual(postId.ToString(), redirectResult.Fragment);
	}

	[TestMethod]
	public async Task LegacyPost_PostOnLastPage_RedirectsWithCorrectPageNumber()
	{
		const int postId = 888;
		const int topicId = 999;
		const int lastPage = 50;

		_model.Id = postId;
		var postPosition = new PostPosition(Page: lastPage, TopicId: topicId);
		_forumService.GetPostPosition(postId, Arg.Any<bool>()).Returns(postPosition);

		var result = await _model.OnGet();

		var redirectResult = (RedirectToPageResult)result;
		Assert.AreEqual(lastPage, redirectResult.RouteValues!["CurrentPage"]);
		Assert.AreEqual(topicId, redirectResult.RouteValues["Id"]);
		Assert.AreEqual(postId, redirectResult.RouteValues["Highlight"]);
	}

	[TestMethod]
	public async Task LegacyPost_WithRestrictedForumPermission_PassesTrueToService()
	{
		const int postId = 123;
		_model.Id = postId;

		var user = _db.AddUserWithRole("AdminUser").Entity;
		await _db.SaveChangesAsync();

		AddAuthenticatedUser(_model, user, [PermissionTo.SeeRestrictedForums]);
		var postPosition = new PostPosition(Page: 1, TopicId: 456);
		_forumService.GetPostPosition(postId, true).Returns(postPosition);

		await _model.OnGet();

		await _forumService.Received(1).GetPostPosition(postId, true);
	}

	[TestMethod]
	public async Task LegacyPost_WithoutRestrictedForumPermission_PassesFalseToService()
	{
		const int postId = 789;
		_model.Id = postId;

		var user = _db.AddUserWithRole("RegularUser").Entity;
		await _db.SaveChangesAsync();

		AddAuthenticatedUser(_model, user, []);
		var postPosition = new PostPosition(Page: 1, TopicId: 101112);
		_forumService.GetPostPosition(postId, false).Returns(postPosition);

		await _model.OnGet();

		await _forumService.Received(1).GetPostPosition(postId, false);
	}
}
