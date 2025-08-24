using TASVideos.Core.Services;
using TASVideos.Pages.Forum.Legacy;

namespace TASVideos.RazorPages.Tests.Pages.Forum.Legacy;

[TestClass]
public class TopicModelTests : BasePageModelTests
{
	private readonly IForumService _forumService;
	private readonly TopicModel _model;

	public TopicModelTests()
	{
		_forumService = Substitute.For<IForumService>();
		_model = new TopicModel(_forumService)
		{
			PageContext = TestPageContext()
		};
	}

	[TestMethod]
	public async Task OnGet_AllParametersNull_ReturnsNotFound()
	{
		_model.P = null;
		_model.T = null;
		_model.Id = null;

		var result = await _model.OnGet();

		AssertForumNotFound(result);
	}

	[TestMethod]
	public async Task OnGet_PParameterWithValidPost_RedirectsToTopicWithHighlight()
	{
		const int postId = 123;
		const int topicId = 456;
		const int pageNumber = 5;

		_model.P = postId;
		_model.T = null;
		_model.Id = null;

		var postPosition = new PostPosition(Page: pageNumber, TopicId: topicId);
		_forumService.GetPostPosition(postId, Arg.Any<bool>()).Returns(postPosition);

		var result = await _model.OnGet();

		AssertRedirect(result, "/Forum/Topics/Index");
		var routeValues = ((RedirectToPageResult)result).RouteValues;
		Assert.IsNotNull(routeValues);
		Assert.AreEqual(topicId, routeValues["Id"]);
		Assert.AreEqual(pageNumber, routeValues["CurrentPage"]);
		Assert.AreEqual(postId, routeValues["Highlight"]);
	}

	[TestMethod]
	public async Task OnGet_PParameterWithInvalidPost_ReturnsNotFound()
	{
		const int postId = 999;
		_model.P = postId;
		_model.T = 123;
		_model.Id = 456;

		_forumService.GetPostPosition(postId, Arg.Any<bool>()).Returns((PostPosition?)null);

		var result = await _model.OnGet();

		AssertForumNotFound(result);
	}

	[TestMethod]
	public async Task OnGet_TParameterOnly_RedirectsToTopicWithTValue()
	{
		_model.P = null;
		_model.T = 789;
		_model.Id = null;

		var result = await _model.OnGet();

		AssertRedirect(result, "/Forum/Topics/Index");
		var routeValues = ((RedirectToPageResult)result).RouteValues;
		Assert.IsNotNull(routeValues);
		Assert.AreEqual(789, routeValues["Id"]);
		Assert.IsFalse(routeValues.ContainsKey("CurrentPage"));
		Assert.IsFalse(routeValues.ContainsKey("Highlight"));
	}

	[TestMethod]
	public async Task OnGet_IdParameterOnly_RedirectsToTopicWithIdValue()
	{
		_model.P = null;
		_model.T = null;
		_model.Id = 321;

		var result = await _model.OnGet();

		AssertRedirect(result, "/Forum/Topics/Index");
		var routeValues = ((RedirectToPageResult)result).RouteValues;
		Assert.IsNotNull(routeValues);
		Assert.AreEqual(321, routeValues["Id"]);
		Assert.IsFalse(routeValues.ContainsKey("CurrentPage"));
		Assert.IsFalse(routeValues.ContainsKey("Highlight"));
	}

	[TestMethod]
	public async Task OnGet_BothTAndIdSet_PrioritizesTOverId()
	{
		_model.P = null;
		_model.T = 111;
		_model.Id = 222;

		var result = await _model.OnGet();

		AssertRedirect(result, "/Forum/Topics/Index", 111);
	}

	[TestMethod]
	public async Task OnGet_PWithRestrictedForumPermission_PassesTrueToService()
	{
		const int postId = 555;
		_model.P = postId;

		var user = _db.AddUserWithRole("AdminUser").Entity;
		await _db.SaveChangesAsync();

		AddAuthenticatedUser(_model, user, [PermissionTo.SeeRestrictedForums]);
		var postPosition = new PostPosition(Page: 1, TopicId: 666);
		_forumService.GetPostPosition(postId, true).Returns(postPosition);

		await _model.OnGet();

		await _forumService.Received(1).GetPostPosition(postId, true);
	}

	[TestMethod]
	public async Task OnGet_PWithoutRestrictedForumPermission_PassesFalseToService()
	{
		const int postId = 777;
		_model.P = postId;

		var user = _db.AddUserWithRole("RegularUser").Entity;
		await _db.SaveChangesAsync();

		AddAuthenticatedUser(_model, user, []);
		var postPosition = new PostPosition(Page: 2, TopicId: 888);
		_forumService.GetPostPosition(postId, false).Returns(postPosition);

		await _model.OnGet();

		await _forumService.Received(1).GetPostPosition(postId, false);
	}

	[TestMethod]
	public async Task OnGet_PPrioritizedOverTAndId_OnlyUsesP()
	{
		const int postId = 123;
		const int tValue = 456;
		const int idValue = 789;

		_model.P = postId;
		_model.T = tValue;
		_model.Id = idValue;

		var postPosition = new PostPosition(Page: 1, TopicId: 999);
		_forumService.GetPostPosition(postId, Arg.Any<bool>()).Returns(postPosition);

		var result = await _model.OnGet();

		// Uses result from GetPostPosition, not T or Id parameters
		AssertRedirect(result, "/Forum/Topics/Index", 999);
		Assert.AreEqual(postId, ((RedirectToPageResult)result).RouteValues!["Highlight"]);
		await _forumService.Received(1).GetPostPosition(postId, Arg.Any<bool>());
	}

	[TestMethod]
	public async Task OnGet_PWithPostOnDifferentPages_RedirectsToCorrectPage()
	{
		const int postId = 123;
		const int topicId = 456;
		const int pageNumber = 78;

		_model.P = postId;
		var postPosition = new PostPosition(Page: pageNumber, TopicId: topicId);
		_forumService.GetPostPosition(postId, Arg.Any<bool>()).Returns(postPosition);

		var result = await _model.OnGet();

		AssertRedirect(result, "/Forum/Topics/Index", topicId);
		Assert.AreEqual(pageNumber, ((RedirectToPageResult)result).RouteValues!["CurrentPage"]);
		Assert.AreEqual(postId, ((RedirectToPageResult)result).RouteValues!["Highlight"]);
	}
}
