using System.Security.Claims;
using TASVideos.Core.Services;
using TASVideos.Core.Services.ExternalMediaPublisher;
using TASVideos.Data.Entity.Forum;
using TASVideos.Pages.Forum.Topics;
using TASVideos.Services;

namespace TASVideos.RazorPages.Tests.Pages.Forum.Topics;

[TestClass]
public class CreateModelTests : BasePageModelTests
{
	private readonly IUserManager _userManager;
	private readonly IExternalMediaPublisher _publisher;
	private readonly IForumService _forumService;
	private readonly CreateModel _model;

	public CreateModelTests()
	{
		_userManager = Substitute.For<IUserManager>();
		_publisher = Substitute.For<IExternalMediaPublisher>();
		_forumService = Substitute.For<IForumService>();
		_forumService.UserAvatars(Arg.Any<int>()).Returns(new AvatarUrls(null, null));
		_forumService.GetTopicCountInForum(Arg.Any<int>(), Arg.Any<int>()).Returns(0);

		_model = new CreateModel(_userManager, _db, _publisher, _forumService)
		{
			PageContext = TestPageContext()
		};
	}

	[TestMethod]
	public async Task OnGet_NonExistentForum_ReturnsNotFound()
	{
		_model.ForumId = 999;
		var result = await _model.OnGet();
		AssertForumNotFound(result);
	}

	[TestMethod]
	public async Task OnGet_ForumDoesNotAllowTopicCreation_ReturnsAccessDenied()
	{
		var forum = _db.AddForum("No New Topics Forum").Entity;
		forum.CanCreateTopics = false;
		await _db.SaveChangesAsync();
		_model.ForumId = forum.Id;

		var result = await _model.OnGet();

		AssertAccessDenied(result);
	}

	[TestMethod]
	public async Task OnGet_ValidForum_PopulatesForumNameAndUserSettings()
	{
		var user = _db.AddUserWithRole("TestUser").Entity;
		user.AutoWatchTopic = UserPreference.Always;
		var forum = _db.AddForum("Test Forum").Entity;
		forum.CanCreateTopics = true;
		await _db.SaveChangesAsync();
		_userManager.GetRequiredUser(Arg.Any<ClaimsPrincipal>()).Returns(user);
		_model.ForumId = forum.Id;

		var result = await _model.OnGet();

		Assert.IsInstanceOfType(result, typeof(PageResult));
		Assert.AreEqual("Test Forum", _model.ForumName);
		Assert.IsTrue(_model.WatchTopic); // AutoWatchTopic.Always should set WatchTopic to true
	}

	[TestMethod]
	public async Task OnGet_UserAutoWatchTopicNever_SetsWatchTopicToFalse()
	{
		var user = _db.AddUserWithRole("TestUser").Entity;
		user.AutoWatchTopic = UserPreference.Never;
		var forum = _db.AddForum("Test Forum").Entity;
		forum.CanCreateTopics = true;
		await _db.SaveChangesAsync();
		_userManager.GetRequiredUser(Arg.Any<ClaimsPrincipal>()).Returns(user);
		AddAuthenticatedUser(_model, user, [PermissionTo.CreateForumTopics]);
		_model.ForumId = forum.Id;

		var result = await _model.OnGet();

		Assert.IsInstanceOfType(result, typeof(PageResult));
		Assert.IsFalse(_model.WatchTopic);
	}

	[TestMethod]
	public async Task OnGet_RestrictedForum_WithoutPermission_ReturnsNotFound()
	{
		var restrictedForum = _db.AddForum("Restricted Forum", true).Entity;
		restrictedForum.CanCreateTopics = true;
		await _db.SaveChangesAsync();
		_model.ForumId = restrictedForum.Id;

		var result = await _model.OnGet();

		AssertForumNotFound(result);
	}

	[TestMethod]
	public async Task OnGet_RestrictedForum_WithPermission_ReturnsPage()
	{
		var user = _db.AddUserWithRole("TestUser").Entity;
		var restrictedForum = _db.AddForum("Restricted Forum", true).Entity;
		restrictedForum.CanCreateTopics = true;
		await _db.SaveChangesAsync();
		_userManager.GetRequiredUser(Arg.Any<ClaimsPrincipal>()).Returns(user);
		AddAuthenticatedUser(_model, user, [PermissionTo.SeeRestrictedForums]);
		_model.ForumId = restrictedForum.Id;

		var result = await _model.OnGet();

		Assert.IsInstanceOfType(result, typeof(PageResult));
		Assert.AreEqual("Restricted Forum", _model.ForumName);
	}

	[TestMethod]
	public async Task OnPost_InvalidModelState_ReturnsPage()
	{
		_model.ModelState.AddModelError("Title", "Title is required");

		var result = await _model.OnPost();

		Assert.IsInstanceOfType(result, typeof(PageResult));
		await _forumService.Received(1).UserAvatars(Arg.Any<int>());
	}

	[TestMethod]
	public async Task OnPost_PollWithoutQuestion_AddsModelError()
	{
		_model.Poll = new AddEditPollModel.PollCreate
		{
			Question = "", // Missing question
			PollOptions = ["Option 1", "Option 2"],
			DaysOpen = 7
		};

		var result = await _model.OnPost();

		Assert.IsInstanceOfType(result, typeof(PageResult));
		Assert.IsTrue(_model.ModelState.ContainsKey($"{nameof(_model.Poll.Question)}"));
	}

	[TestMethod]
	public async Task OnPost_PollWithInvalidOptions_AddsModelError()
	{
		_model.Poll = new AddEditPollModel.PollCreate
		{
			Question = "Test question?",
			PollOptions = ["Option 1"], // Only one option
			DaysOpen = 7
		};

		var result = await _model.OnPost();

		Assert.IsInstanceOfType(result, typeof(PageResult));
		Assert.IsTrue(_model.ModelState.ContainsKey($"{nameof(_model.Poll.PollOptions)}"));
	}

	[TestMethod]
	public async Task OnPost_NonExistentForum_ReturnsNotFound()
	{
		_model.ForumId = 999;
		var result = await _model.OnPost();
		AssertForumNotFound(result);
	}

	[TestMethod]
	public async Task OnPost_RestrictedForumWithoutPermission_ReturnsNotFound()
	{
		var restrictedForum = _db.AddForum("Restricted Forum", true).Entity;
		await _db.SaveChangesAsync();
		_model.ForumId = restrictedForum.Id;

		var result = await _model.OnPost();

		AssertForumNotFound(result);
	}

	[TestMethod]
	public async Task OnPost_ValidTopic_CreatesTopicAndPost()
	{
		var user = _db.AddUserWithRole("TestUser").Entity;
		var forum = _db.AddForum("Test Forum").Entity;
		await _db.SaveChangesAsync();

		var createModel = new CreateModel(_userManager, _db, _publisher, _forumService)
		{
			PageContext = TestPageContext(),
			ForumId = forum.Id,
			Title = "Test Topic Title",
			Post = "This is a test post content",
			Type = ForumTopicType.Sticky,
			WatchTopic = true,
			Mood = ForumPostMood.Playful
		};
		AddAuthenticatedUser(createModel, user, [PermissionTo.CreateForumTopics]);

		var result = await createModel.OnPost();

		AssertRedirect(result, "Index");

		var createdTopic = await _db.ForumTopics.SingleOrDefaultAsync(t => t.Title == "Test Topic Title");
		Assert.IsNotNull(createdTopic);
		Assert.AreEqual(forum.Id, createdTopic.ForumId);
		Assert.AreEqual(user.Id, createdTopic.PosterId);
		Assert.AreEqual(ForumTopicType.Sticky, createdTopic.Type);

		await _userManager.Received(1).AssignAutoAssignableRolesByPost(user.Id);
		await _publisher.Received(1).Send(Arg.Any<IPostable>());
		await _forumService.Received(1).CreatePost(Arg.Is<PostCreate>(p => p.WatchTopic == true));
		await _forumService.Received(1).CreatePost(Arg.Is<PostCreate>(p => p.Mood == ForumPostMood.Playful));
	}

	[TestMethod]
	public async Task OnPost_ValidTopicWithPoll_CreatesPoll()
	{
		var user = _db.AddUserWithRole("TestUser").Entity;
		var forum = _db.AddForum("Test Forum").Entity;
		await _db.SaveChangesAsync();

		var createModel = new CreateModel(_userManager, _db, _publisher, _forumService)
		{
			PageContext = TestPageContext(),
			ForumId = forum.Id,
			Title = "Topic with Poll",
			Post = "Post content",
			Poll = new AddEditPollModel.PollCreate
			{
				Question = "What do you think?",
				PollOptions = ["Option 1", "Option 2", "Option 3"],
				DaysOpen = 7
			}
		};
		AddAuthenticatedUser(createModel, user, [PermissionTo.CreateForumTopics, PermissionTo.CreateForumPolls]);

		var result = await createModel.OnPost();

		AssertRedirect(result, "Index");
		await _forumService.Received(1).CreatePoll(
			Arg.Is<ForumTopic>(t => t.Title == "Topic with Poll"),
			Arg.Is<PollCreate>(p => p.Question == "What do you think?"));
	}

	[TestMethod]
	public async Task OnPost_PollWithoutPermission_DoesNotCreatePoll()
	{
		var user = _db.AddUserWithRole("TestUser").Entity;
		var forum = _db.AddForum("Test Forum").Entity;
		await _db.SaveChangesAsync();

		var createModel = new CreateModel(_userManager, _db, _publisher, _forumService)
		{
			PageContext = TestPageContext(),
			ForumId = forum.Id,
			Title = "Topic with Poll",
			Post = "Post content",
			Poll = new AddEditPollModel.PollCreate
			{
				Question = "What do you think?",
				PollOptions = ["Option 1", "Option 2"],
				DaysOpen = 7
			}
		};
		AddAuthenticatedUser(createModel, user, [PermissionTo.CreateForumTopics]);

		var result = await createModel.OnPost();

		AssertRedirect(result, "Index");
		await _forumService.DidNotReceive().CreatePoll(Arg.Any<ForumTopic>(), Arg.Any<PollCreate>());
	}
}
