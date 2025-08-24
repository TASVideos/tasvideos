using System.Security.Claims;
using TASVideos.Core.Services;
using TASVideos.Core.Services.ExternalMediaPublisher;
using TASVideos.Data.Entity.Forum;
using TASVideos.Pages.Forum.Posts;
using TASVideos.Services;

namespace TASVideos.RazorPages.Tests.Pages.Forum.Posts;

[TestClass]
public class CreateModelTests : BasePageModelTests
{
	private readonly IUserManager _userManager;
	private readonly IExternalMediaPublisher _publisher;
	private readonly ITopicWatcher _topicWatcher;
	private readonly IForumService _forumService;
	private readonly CreateModel _model;

	public CreateModelTests()
	{
		_userManager = Substitute.For<IUserManager>();
		_publisher = Substitute.For<IExternalMediaPublisher>();
		_topicWatcher = Substitute.For<ITopicWatcher>();
		_forumService = Substitute.For<IForumService>();
		_forumService.UserAvatars(Arg.Any<int>()).Returns(new AvatarUrls(null, null));
		_model = new CreateModel(_userManager, _publisher, _db, _topicWatcher, _forumService)
		{
			PageContext = TestPageContext()
		};
	}

	[TestMethod]
	public async Task OnGet_NonExistentTopic_ReturnsNotFound()
	{
		_model.TopicId = 999;
		var result = await _model.OnGet();
		AssertForumNotFound(result);
	}

	[TestMethod]
	public async Task OnGet_RestrictedTopic_WithoutPermission_ReturnsNotFound()
	{
		var user = _db.AddUserWithRole("TestUser").Entity;
		var topic = _db.AddTopic(user, true).Entity;
		await _db.SaveChangesAsync();
		AddAuthenticatedUser(_model, user, [PermissionTo.CreateForumPosts]);
		_model.TopicId = topic.Id;

		var result = await _model.OnGet();

		AssertForumNotFound(result);
	}

	[TestMethod]
	public async Task OnGet_LockedTopic_WithoutPermission_ReturnsAccessDenied()
	{
		var user = _db.AddUserWithRole("TestUser").Entity;
		var topic = _db.AddTopic(user).Entity;
		topic.IsLocked = true;
		await _db.SaveChangesAsync();

		AddAuthenticatedUser(_model, user, []); // No PostInLockedTopics permission
		_model.TopicId = topic.Id;

		var result = await _model.OnGet();

		AssertAccessDenied(result);
	}

	[TestMethod]
	public async Task OnGet_LockedTopic_WithPermission_ReturnsPage()
	{
		var user = _db.AddUserWithRole("TestUser").Entity;
		var topic = _db.AddTopic(user).Entity;
		topic.IsLocked = true;
		await _db.SaveChangesAsync();

		_userManager.GetRequiredUser(Arg.Any<ClaimsPrincipal>()).Returns(user);
		AddAuthenticatedUser(_model, user, [PermissionTo.CreateForumPosts, PermissionTo.PostInLockedTopics]);
		_model.TopicId = topic.Id;

		var result = await _model.OnGet();

		Assert.IsInstanceOfType(result, typeof(PageResult));
		Assert.IsTrue(_model.IsLocked);
	}

	[TestMethod]
	public async Task OnGet_ValidTopic_PopulatesTopicData()
	{
		var user = _db.AddUserWithRole("TestUser").Entity;
		var topic = _db.AddTopic(user).Entity;
		topic.Title = "Test Topic";
		topic.IsLocked = false;
		await _db.SaveChangesAsync();

		_userManager.GetRequiredUser(Arg.Any<ClaimsPrincipal>()).Returns(user);
		_forumService.GetPostCountInTopic(user.Id, topic.Id).Returns(5);

		AddAuthenticatedUser(_model, user, [PermissionTo.CreateForumPosts]);
		_model.TopicId = topic.Id;

		var result = await _model.OnGet();

		Assert.IsInstanceOfType(result, typeof(PageResult));
		Assert.AreEqual("Test Topic", _model.TopicTitle);
		Assert.IsFalse(_model.IsLocked);
		Assert.AreEqual("5", _model.BackupSubmissionDeterminator);
	}

	[TestMethod]
	public async Task OnGet_WithQuoteId_PopulatesQuotedText()
	{
		var user = _db.AddUserWithRole("TestUser").Entity;
		var quotedUser = _db.AddUser("QuotedUser").Entity;
		var topic = _db.AddTopic(user).Entity;
		topic.Title = "Test Topic";

		var post = _db.CreatePostForTopic(topic, quotedUser).Entity;
		post.Text = "Original post content";
		await _db.SaveChangesAsync();

		_userManager.GetRequiredUser(Arg.Any<ClaimsPrincipal>()).Returns(user);
		AddAuthenticatedUser(_model, user, [PermissionTo.CreateForumPosts]);
		_model.TopicId = topic.Id;
		_model.QuoteId = post.Id;

		var result = await _model.OnGet();

		Assert.IsInstanceOfType(result, typeof(PageResult));
		Assert.IsTrue(_model.Text.Contains($"[quote=\"[post={post.Id}][/post] QuotedUser\"]Original post content[/quote]"));
	}

	[TestMethod]
	public async Task OnGet_WithAutoWatchAlways_SetsWatchTopicTrue()
	{
		var user = _db.AddUserWithRole("TestUser").Entity;
		user.AutoWatchTopic = UserPreference.Always;
		var topic = _db.AddTopic(user).Entity;
		await _db.SaveChangesAsync();

		_userManager.GetRequiredUser(Arg.Any<ClaimsPrincipal>()).Returns(user);
		AddAuthenticatedUser(_model, user, [PermissionTo.CreateForumPosts]);
		_model.TopicId = topic.Id;

		var result = await _model.OnGet();

		Assert.IsInstanceOfType(result, typeof(PageResult));
		Assert.IsTrue(_model.WatchTopic); // Should be true due to AutoWatchTopic.Always
	}

	[TestMethod]
	public async Task OnGet_WithAutoWatchNever_SetsWatchTopicFalse()
	{
		var user = _db.AddUserWithRole("TestUser").Entity;
		user.AutoWatchTopic = UserPreference.Never;
		var topic = _db.AddTopic(user).Entity;
		await _db.SaveChangesAsync();

		_userManager.GetRequiredUser(Arg.Any<ClaimsPrincipal>()).Returns(user);
		AddAuthenticatedUser(_model, user, [PermissionTo.CreateForumPosts]);
		_model.TopicId = topic.Id;

		var result = await _model.OnGet();

		Assert.IsInstanceOfType(result, typeof(PageResult));
		Assert.IsFalse(_model.WatchTopic); // Should be false due to AutoWatchTopic.Never
	}

	[TestMethod]
	public async Task OnGet_WithPreviousPosts_LoadsLastTenPosts()
	{
		var user = _db.AddUserWithRole("TestUser").Entity;
		var poster = _db.AddUser("Poster").Entity;
		poster.PreferredPronouns = PreferredPronounTypes.TheyThem;
		var topic = _db.AddTopic(user).Entity;
		topic.Title = "Test Topic";

		// Create 15 posts to test the limit of 10
		for (int i = 1; i <= 15; i++)
		{
			var post = _db.CreatePostForTopic(topic, poster).Entity;
			post.Text = $"Post content {i}";
			post.CreateTimestamp = DateTime.UtcNow.AddMinutes(i);
			post.EnableBbCode = true;
			post.EnableHtml = false;
		}

		await _db.SaveChangesAsync();

		_userManager.GetRequiredUser(Arg.Any<ClaimsPrincipal>()).Returns(user);
		AddAuthenticatedUser(_model, user, [PermissionTo.CreateForumPosts]);
		_model.TopicId = topic.Id;

		var result = await _model.OnGet();

		Assert.IsInstanceOfType(result, typeof(PageResult));
		Assert.AreEqual(10, _model.PreviousPosts.Count);

		// Should be the most recent 10 posts (6-15), in chronological order
		Assert.AreEqual("Post content 6", _model.PreviousPosts[0].Text);
		Assert.AreEqual("Post content 15", _model.PreviousPosts[9].Text);
		Assert.AreEqual("Poster", _model.PreviousPosts[0].PosterName);
		Assert.AreEqual(PreferredPronounTypes.TheyThem, _model.PreviousPosts[0].PosterPronouns);
		Assert.IsFalse(_model.PreviousPosts[0].EnableHtml);
		Assert.IsTrue(_model.PreviousPosts[0].EnableBbCode);
	}

	[TestMethod]
	public async Task OnPost_InvalidModelState_ReturnsPageWithCorrectData()
	{
		var user = _db.AddUserWithRole("TestUser").Entity;
		user.Avatar = "user_avatar.png";
		user.MoodAvatarUrlBase = "mood_base";
		var topic = _db.AddTopic(user).Entity;
		await _db.SaveChangesAsync();

		_userManager.GetRequiredUser(Arg.Any<ClaimsPrincipal>()).Returns(user);
		_forumService.IsTopicLocked(topic.Id).Returns(false);

		AddAuthenticatedUser(_model, user, [PermissionTo.CreateForumPosts]);
		_model.TopicId = topic.Id;
		_model.ModelState.AddModelError("Text", "Text is required");

		var result = await _model.OnPost();

		Assert.IsInstanceOfType(result, typeof(PageResult));
		Assert.IsFalse(_model.IsLocked);
		Assert.AreEqual("user_avatar.png", _model.UserAvatars.Avatar);
		Assert.AreEqual("mood_base", _model.UserAvatars.MoodBase);
	}

	[TestMethod]
	public async Task OnPost_InvalidModelState_LockedTopic_WithoutPermission_ReturnsAccessDenied()
	{
		var user = _db.AddUserWithRole("TestUser").Entity;
		var topic = _db.AddTopic(user).Entity;
		await _db.SaveChangesAsync();

		_userManager.GetRequiredUser(Arg.Any<ClaimsPrincipal>()).Returns(user);
		_forumService.IsTopicLocked(topic.Id).Returns(true);

		AddAuthenticatedUser(_model, user, [PermissionTo.CreateForumPosts]); // No PostInLockedTopics permission
		_model.TopicId = topic.Id;
		_model.ModelState.AddModelError("Text", "Text is required");

		var result = await _model.OnPost();

		AssertAccessDenied(result);
	}

	[TestMethod]
	public async Task OnPost_NonExistentTopic_ReturnsNotFound()
	{
		var user = _db.AddUserWithRole("TestUser").Entity;
		await _db.SaveChangesAsync();

		_userManager.GetRequiredUser(Arg.Any<ClaimsPrincipal>()).Returns(user);

		AddAuthenticatedUser(_model, user, [PermissionTo.CreateForumPosts]);
		_model.TopicId = 999;

		var result = await _model.OnPost();

		AssertForumNotFound(result);
	}

	[TestMethod]
	public async Task OnPost_RestrictedTopic_WithoutPermission_ReturnsNotFound()
	{
		var user = _db.AddUserWithRole("TestUser").Entity;
		var topic = _db.AddTopic(user, true).Entity;
		await _db.SaveChangesAsync();

		_userManager.GetRequiredUser(Arg.Any<ClaimsPrincipal>()).Returns(user);
		AddAuthenticatedUser(_model, user, [PermissionTo.CreateForumPosts]); // No SeeRestrictedForums permission
		_model.TopicId = topic.Id;

		var result = await _model.OnPost();

		AssertForumNotFound(result);
	}

	[TestMethod]
	public async Task OnPost_LockedTopic_WithoutPermission_ReturnsAccessDenied()
	{
		var user = _db.AddUserWithRole("TestUser").Entity;
		var topic = _db.AddTopic(user).Entity;
		topic.IsLocked = true;
		await _db.SaveChangesAsync();

		_userManager.GetRequiredUser(Arg.Any<ClaimsPrincipal>()).Returns(user);

		AddAuthenticatedUser(_model, user, [PermissionTo.CreateForumPosts]); // No PostInLockedTopics permission
		_model.TopicId = topic.Id;
		_model.Text = "Valid post content";

		var result = await _model.OnPost();

		AssertAccessDenied(result);
	}

	[TestMethod]
	public async Task OnPost_ValidPost_CreatesPostAndRedirects()
	{
		var user = _db.AddUserWithRole("TestPoster").Entity;
		var topic = _db.AddTopic(user).Entity;
		topic.Title = "Test Topic";
		await _db.SaveChangesAsync();

		const int newPostId = 123;
		_userManager.GetRequiredUser(Arg.Any<ClaimsPrincipal>()).Returns(user);
		_forumService.CreatePost(Arg.Any<PostCreate>()).Returns(newPostId);

		AddAuthenticatedUser(_model, user, [PermissionTo.CreateForumPosts]);
		_model.TopicId = topic.Id;
		_model.Subject = "Test Subject";
		_model.Text = "Valid post content";
		_model.Mood = ForumPostMood.Normal;
		_model.WatchTopic = true;

		var result = await _model.OnPost();

		Assert.IsInstanceOfType(result, typeof(RedirectResult));
		var redirectResult = (RedirectResult)result;
		Assert.AreEqual("/Forum/Posts/123", redirectResult.Url);

		await _forumService.Received(1).CreatePost(Arg.Any<PostCreate>());
		await _userManager.Received(1).AssignAutoAssignableRolesByPost(user.Id);
		await _topicWatcher.Received(1).NotifyNewPost(newPostId, topic.Id, "Test Topic", user.Id);
		await _publisher.Received(1).Send(Arg.Any<Post>());
	}

	[TestMethod]
	public async Task OnGet_WithInvalidQuoteId_IgnoresQuote()
	{
		var user = _db.AddUserWithRole("TestUser").Entity;
		var topic = _db.AddTopic(user).Entity;
		await _db.SaveChangesAsync();

		_userManager.GetRequiredUser(Arg.Any<ClaimsPrincipal>()).Returns(user);
		AddAuthenticatedUser(_model, user, [PermissionTo.CreateForumPosts]);
		_model.TopicId = topic.Id;
		_model.QuoteId = 999; // Non-existent post

		var result = await _model.OnGet();

		Assert.IsInstanceOfType(result, typeof(PageResult));
		Assert.AreEqual("", _model.Text); // Should remain empty since quote doesn't exist
	}

	[TestMethod]
	public void RequiresPermission() => AssertHasPermission(typeof(CreateModel), PermissionTo.CreateForumPosts);
}
