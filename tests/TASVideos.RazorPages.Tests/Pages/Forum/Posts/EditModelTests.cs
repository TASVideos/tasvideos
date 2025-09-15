using TASVideos.Core.Services;
using TASVideos.Core.Services.ExternalMediaPublisher;
using TASVideos.Data.Entity.Forum;
using TASVideos.Pages.Forum.Posts;
using TASVideos.Services;

namespace TASVideos.RazorPages.Tests.Pages.Forum.Posts;

// TODO: there are scenarios not tested here due to the problems with ExecuteUpdate/ExecuteDelete not actually deleting in the test database
[TestClass]
public class EditModelTests : BasePageModelTests
{
	private readonly IExternalMediaPublisher _publisher;
	private readonly IForumService _forumService;
	private readonly IUserManager _userManager;
	private readonly EditModel _model;

	public EditModelTests()
	{
		_publisher = Substitute.For<IExternalMediaPublisher>();
		_forumService = Substitute.For<IForumService>();
		_userManager = Substitute.For<IUserManager>();

		_model = new EditModel(_db, _publisher, _forumService, _userManager)
		{
			PageContext = TestPageContext()
		};
	}

	[TestMethod]
	public async Task OnGet_NonExistentPost_ReturnsNotFound()
	{
		_model.Id = 999;
		var result = await _model.OnGet();
		AssertForumNotFound(result);
	}

	[TestMethod]
	public async Task OnGet_RestrictedPost_WithoutPermission_ReturnsNotFound()
	{
		var user = _db.AddUserWithRole("TestUser").Entity;
		var topic = _db.AddTopic(user, true).Entity;
		var post = _db.CreatePostForTopic(topic, user).Entity;
		await _db.SaveChangesAsync();

		AddAuthenticatedUser(_model, user, [PermissionTo.CreateForumPosts, PermissionTo.EditForumPosts]);
		_model.Id = post.Id;

		var result = await _model.OnGet();

		AssertForumNotFound(result);
	}

	[TestMethod]
	public async Task OnGet_CannotEditPost_WithoutPermission_ReturnsAccessDenied()
	{
		var user = _db.AddUserWithRole("TestUser").Entity;
		var otherUser = _db.AddUser("OtherUser").Entity;
		var topic = _db.AddTopic(user).Entity;
		var post = _db.CreatePostForTopic(topic, otherUser).Entity;
		post.Text = "Other user's post";
		await _db.SaveChangesAsync();

		AddAuthenticatedUser(_model, user, [PermissionTo.CreateForumPosts, PermissionTo.EditForumPosts]); // Can edit own posts but not others
		_model.Id = post.Id;

		var result = await _model.OnGet();

		AssertAccessDenied(result);
	}

	[TestMethod]
	public async Task OnGet_CanEditOwnPost_ReturnsPageWithPostData()
	{
		var user = _db.AddUserWithRole("TestUser").Entity;
		var topic = _db.AddTopic(user).Entity;
		topic.Title = "Test Topic";
		var post = _db.CreatePostForTopic(topic, user).Entity;
		post.Subject = "Test Subject";
		post.Text = "Test post content";
		post.PosterMood = ForumPostMood.Happy;
		post.EnableBbCode = true;
		post.EnableHtml = false;
		await _db.SaveChangesAsync();

		_forumService.UserAvatars(user.Id).Returns(new AvatarUrls("avatar.png", "mood"));

		AddAuthenticatedUser(_model, user, [PermissionTo.CreateForumPosts, PermissionTo.EditForumPosts]);
		_model.Id = post.Id;

		var result = await _model.OnGet();

		Assert.IsInstanceOfType(result, typeof(PageResult));
		Assert.AreEqual(post.Id, _model.Id);
		Assert.AreEqual("Test Subject", _model.Post.Subject);
		Assert.AreEqual("Test post content", _model.Post.Text);
		Assert.AreEqual(ForumPostMood.Happy, _model.Post.Mood);
		Assert.AreEqual("Test Topic", _model.Post.TopicTitle);
		Assert.AreEqual(user.Id, _model.Post.PosterId);
		Assert.AreEqual(user.UserName, _model.Post.PosterName);
		Assert.IsTrue(_model.Post.EnableBbCode);
		Assert.IsFalse(_model.Post.EnableHtml);
	}

	[TestMethod]
	public async Task OnGet_CanEditOthersPost_WithEditUsersForumPostsPermission_ReturnsPage()
	{
		var user = _db.AddUserWithRole("TestUser").Entity;
		var otherUser = _db.AddUser("OtherUser").Entity;
		var topic = _db.AddTopic(user).Entity;
		var post = _db.CreatePostForTopic(topic, otherUser).Entity;
		post.Text = "Other user's post";
		await _db.SaveChangesAsync();

		AddAuthenticatedUser(_model, user, [PermissionTo.CreateForumPosts, PermissionTo.EditUsersForumPosts]);
		_model.Id = post.Id;

		var result = await _model.OnGet();

		Assert.IsInstanceOfType(result, typeof(PageResult));
		Assert.AreEqual(otherUser.Id, _model.Post.PosterId);
		Assert.AreEqual(otherUser.UserName, _model.Post.PosterName);
	}

	[TestMethod]
	public async Task OnGet_FirstPost_SetsIsFirstPostTrue()
	{
		var user = _db.AddUserWithRole("TestUser").Entity;
		var topic = _db.AddTopic(user).Entity;
		var firstPost = _db.CreatePostForTopic(topic, user).Entity;
		firstPost.CreateTimestamp = DateTime.UtcNow.AddMinutes(-10);
		var secondPost = _db.CreatePostForTopic(topic, user).Entity;
		secondPost.CreateTimestamp = DateTime.UtcNow.AddMinutes(-5);
		await _db.SaveChangesAsync();

		AddAuthenticatedUser(_model, user, [PermissionTo.CreateForumPosts, PermissionTo.EditForumPosts]);
		_model.Id = firstPost.Id;

		var result = await _model.OnGet();

		Assert.IsInstanceOfType(result, typeof(PageResult));
		Assert.IsTrue(_model.IsFirstPost);
	}

	[TestMethod]
	public async Task OnGet_NotFirstPost_SetsIsFirstPostFalse()
	{
		var user = _db.AddUserWithRole("TestUser").Entity;
		var topic = _db.AddTopic(user).Entity;
		var firstPost = _db.CreatePostForTopic(topic, user).Entity;
		firstPost.CreateTimestamp = DateTime.UtcNow.AddMinutes(-10);
		var secondPost = _db.CreatePostForTopic(topic, user).Entity;
		secondPost.CreateTimestamp = DateTime.UtcNow.AddMinutes(-5);
		await _db.SaveChangesAsync();

		AddAuthenticatedUser(_model, user, [PermissionTo.CreateForumPosts, PermissionTo.EditForumPosts]);
		_model.Id = secondPost.Id;

		var result = await _model.OnGet();

		Assert.IsInstanceOfType(result, typeof(PageResult));
		Assert.IsFalse(_model.IsFirstPost);
	}

	[TestMethod]
	public async Task OnGet_WithPreviousPosts_LoadsPreviousPosts()
	{
		var user = _db.AddUserWithRole("TestUser").Entity;
		var topic = _db.AddTopic(user).Entity;

		var post1 = _db.CreatePostForTopic(topic, user).Entity;
		post1.Text = "First post";
		post1.CreateTimestamp = DateTime.UtcNow.AddMinutes(-20);

		var post2 = _db.CreatePostForTopic(topic, user).Entity;
		post2.Text = "Second post";
		post2.CreateTimestamp = DateTime.UtcNow.AddMinutes(-15);

		var currentPost = _db.CreatePostForTopic(topic, user).Entity;
		currentPost.Text = "Current post being edited";
		currentPost.CreateTimestamp = DateTime.UtcNow.AddMinutes(-10);

		var futurePost = _db.CreatePostForTopic(topic, user).Entity;
		futurePost.Text = "Future post";
		futurePost.CreateTimestamp = DateTime.UtcNow.AddMinutes(-5);

		await _db.SaveChangesAsync();

		AddAuthenticatedUser(_model, user, [PermissionTo.CreateForumPosts, PermissionTo.EditForumPosts]);
		_model.Id = currentPost.Id;

		var result = await _model.OnGet();

		Assert.IsInstanceOfType(result, typeof(PageResult));
		Assert.AreEqual(2, _model.PreviousPosts.Count);
		Assert.AreEqual("First post", _model.PreviousPosts[0].Text);
		Assert.AreEqual("Second post", _model.PreviousPosts[1].Text);
	}

	[TestMethod]
	public async Task OnGet_EditingOwnPost_LoadsUserAvatars()
	{
		var user = _db.AddUserWithRole("TestUser").Entity;
		var topic = _db.AddTopic(user).Entity;
		var post = _db.CreatePostForTopic(topic, user).Entity;
		await _db.SaveChangesAsync();

		_forumService.UserAvatars(user.Id).Returns(new AvatarUrls("test_avatar.png", "mood_base"));

		AddAuthenticatedUser(_model, user, [PermissionTo.CreateForumPosts, PermissionTo.EditForumPosts]);
		_model.Id = post.Id;

		var result = await _model.OnGet();

		Assert.IsInstanceOfType(result, typeof(PageResult));
		Assert.AreEqual("test_avatar.png", _model.UserAvatars.Avatar);
		Assert.AreEqual("mood_base", _model.UserAvatars.MoodBase);
		await _forumService.Received(1).UserAvatars(user.Id);
	}

	[TestMethod]
	public async Task OnGet_EditingOthersPost_DoesNotLoadUserAvatars()
	{
		var user = _db.AddUserWithRole("TestUser").Entity;
		var otherUser = _db.AddUser("OtherUser").Entity;
		var topic = _db.AddTopic(user).Entity;
		var post = _db.CreatePostForTopic(topic, otherUser).Entity;
		await _db.SaveChangesAsync();

		AddAuthenticatedUser(_model, user, [PermissionTo.CreateForumPosts, PermissionTo.EditUsersForumPosts]);
		_model.Id = post.Id;

		var result = await _model.OnGet();

		Assert.IsInstanceOfType(result, typeof(PageResult));
		await _forumService.DidNotReceive().UserAvatars(Arg.Any<int>());
	}

	[TestMethod]
	public async Task OnPost_InvalidModelState_ReturnsPageWithAvatars()
	{
		var user = _db.AddUserWithRole("TestUser").Entity;
		var topic = _db.AddTopic(user).Entity;
		var post = _db.CreatePostForTopic(topic, user).Entity;
		await _db.SaveChangesAsync();

		_forumService.UserAvatars(user.Id).Returns(new AvatarUrls("avatar.png", "mood"));

		AddAuthenticatedUser(_model, user, [PermissionTo.CreateForumPosts, PermissionTo.EditForumPosts]);
		_model.Id = post.Id;
		_model.Post = new EditModel.ForumPostEditModel
		{
			PosterId = user.Id,
			TopicId = topic.Id,
			Text = "Valid text"
		};
		_model.ModelState.AddModelError("Subject", "Subject is invalid");

		var result = await _model.OnPost();

		Assert.IsInstanceOfType(result, typeof(PageResult));
		await _forumService.Received(1).UserAvatars(user.Id);
	}

	[TestMethod]
	public async Task OnPost_InvalidModelState_EditingOthersPost_DoesNotLoadAvatars()
	{
		var user = _db.AddUserWithRole("TestUser").Entity;
		var otherUser = _db.AddUser("OtherUser").Entity;
		var topic = _db.AddTopic(user).Entity;
		var post = _db.CreatePostForTopic(topic, otherUser).Entity;
		await _db.SaveChangesAsync();

		AddAuthenticatedUser(_model, user, [PermissionTo.CreateForumPosts, PermissionTo.EditUsersForumPosts]);
		_model.Id = post.Id;
		_model.Post = new EditModel.ForumPostEditModel
		{
			PosterId = otherUser.Id,
			TopicId = topic.Id,
			Text = "Valid text"
		};
		_model.ModelState.AddModelError("Subject", "Subject is invalid");

		var result = await _model.OnPost();

		Assert.IsInstanceOfType(result, typeof(PageResult));
		await _forumService.DidNotReceive().UserAvatars(Arg.Any<int>());
	}

	[TestMethod]
	public async Task OnPost_NonExistentPost_ReturnsNotFound()
	{
		var user = _db.AddUserWithRole("TestUser").Entity;
		await _db.SaveChangesAsync();

		AddAuthenticatedUser(_model, user, [PermissionTo.CreateForumPosts, PermissionTo.EditForumPosts]);
		_model.Id = 999;
		_model.Post = new EditModel.ForumPostEditModel
		{
			PosterId = user.Id,
			Text = "Valid text"
		};

		var result = await _model.OnPost();

		AssertForumNotFound(result);
	}

	[TestMethod]
	public async Task OnPost_CannotEditPost_ReturnsPageWithError()
	{
		var user = _db.AddUserWithRole("TestUser").Entity;
		var otherUser = _db.AddUser("OtherUser").Entity;
		var topic = _db.AddTopic(user).Entity;
		var post = _db.CreatePostForTopic(topic, otherUser).Entity;
		await _db.SaveChangesAsync();

		AddAuthenticatedUser(_model, user, [PermissionTo.CreateForumPosts, PermissionTo.EditForumPosts]); // Can edit own but not others
		_model.Id = post.Id;
		_model.Post = new EditModel.ForumPostEditModel
		{
			PosterId = otherUser.Id,
			TopicId = topic.Id,
			Text = "Updated text"
		};

		var result = await _model.OnPost();

		Assert.IsInstanceOfType(result, typeof(PageResult));
		Assert.IsFalse(_model.ModelState.IsValid);
		Assert.IsTrue(_model.ModelState.ContainsKey(""));
	}

	[TestMethod]
	public async Task OnPost_ValidEdit_UpdatesPostAndRedirects()
	{
		var user = _db.AddUserWithRole("TestUser").Entity;
		var topic = _db.AddTopic(user).Entity;
		topic.Title = "Original Topic Title";
		var post = _db.CreatePostForTopic(topic, user).Entity;
		post.Subject = "Original Subject";
		post.Text = "Original text";
		post.PosterMood = ForumPostMood.Normal;
		await _db.SaveChangesAsync();

		AddAuthenticatedUser(_model, user, [PermissionTo.CreateForumPosts, PermissionTo.EditForumPosts]);
		_model.Id = post.Id;
		_model.Post = new EditModel.ForumPostEditModel
		{
			PosterId = user.Id,
			TopicId = topic.Id,
			Subject = "Updated Subject",
			Text = "Updated text",
			Mood = ForumPostMood.Happy
		};

		var result = await _model.OnPost();

		Assert.IsInstanceOfType(result, typeof(RedirectResult));
		var redirectResult = (RedirectResult)result;
		Assert.AreEqual($"/Forum/Posts/{post.Id}", redirectResult.Url);

		// Verify the post was updated
		await _db.Entry(post).ReloadAsync();
		Assert.AreEqual("Updated Subject", post.Subject);
		Assert.AreEqual("Updated text", post.Text);
		Assert.AreEqual(ForumPostMood.Happy, post.PosterMood);
		Assert.IsNotNull(post.PostEditedTimestamp);

		// Verify the topic title was NOT updated
		await _db.Entry(topic).ReloadAsync();
		Assert.AreEqual("Original Topic Title", topic.Title);

		_forumService.Received(1).CacheEditedPostActivity(topic.Forum!.Id, topic.Id, post.Id, Arg.Any<DateTime>());
		await _publisher.Received(1).Send(Arg.Any<Post>());
	}

	[TestMethod]
	public async Task OnPost_ValidEditFirstPost_UpdatesTopicTitleAndPost()
	{
		var user = _db.AddUserWithRole("TestUser").Entity;
		var topic = _db.AddTopic(user).Entity;
		var firstPost = _db.CreatePostForTopic(topic, user).Entity;
		firstPost.CreateTimestamp = DateTime.UtcNow.AddMinutes(-10);
		var secondPost = _db.CreatePostForTopic(topic, user).Entity;
		secondPost.CreateTimestamp = DateTime.UtcNow.AddMinutes(-5);
		await _db.SaveChangesAsync();

		AddAuthenticatedUser(_model, user, [PermissionTo.CreateForumPosts, PermissionTo.EditForumPosts]);
		_model.Id = firstPost.Id;
		_model.Post = new EditModel.ForumPostEditModel
		{
			PosterId = user.Id,
			TopicId = topic.Id,
			TopicTitle = "Updated Topic Title",
			Subject = "Updated Subject",
			Text = "Updated text"
		};

		var result = await _model.OnPost();

		Assert.IsInstanceOfType(result, typeof(RedirectResult));

		// Verify the topic title was updated
		await _db.Entry(topic).ReloadAsync();
		Assert.AreEqual("Updated Topic Title", topic.Title);

		// Verify the post was updated
		await _db.Entry(firstPost).ReloadAsync();
		Assert.AreEqual("Updated Subject", firstPost.Subject);
		Assert.AreEqual("Updated text", firstPost.Text);
	}

	[TestMethod]
	public async Task OnPostDelete_NonExistentPost_ReturnsNotFound()
	{
		_model.Id = 999;
		var result = await _model.OnPostDelete();
		AssertForumNotFound(result);
	}

	[TestMethod]
	public async Task OnPostDeleteWithoutDeletePermission_AndUserNotPoster_ReturnsAccessDenied()
	{
		var user = _db.AddUserWithRole("TestUser").Entity;
		var poster = _db.AddUser("OriginalPoster").Entity;
		var topic = _db.AddTopic(poster).Entity;
		var post = _db.CreatePostForTopic(topic, poster).Entity;
		await _db.SaveChangesAsync();
		AddAuthenticatedUser(_model, user, [PermissionTo.CreateForumPosts, PermissionTo.EditForumPosts]); // No DeleteForumPosts
		_model.Id = post.Id;

		var result = await _model.OnPostDelete();

		AssertAccessDenied(result);
	}

	[TestMethod]
	public async Task OnPostDelete_WithoutDeletePermission_NotLastPost_ReturnsNotFound()
	{
		var user = _db.AddUserWithRole("TestUser").Entity;
		var topic = _db.AddTopic(user).Entity;
		var post1 = _db.CreatePostForTopic(topic, user).Entity;
		post1.CreateTimestamp = DateTime.UtcNow.AddMinutes(-10);
		var post2 = _db.CreatePostForTopic(topic, user).Entity;
		post2.CreateTimestamp = DateTime.UtcNow.AddMinutes(-5);
		await _db.SaveChangesAsync();

		AddAuthenticatedUser(_model, user, [PermissionTo.CreateForumPosts, PermissionTo.EditForumPosts]); // No DeleteForumPosts
		_model.Id = post1.Id; // Not the last post

		var result = await _model.OnPostDelete();

		AssertForumNotFound(result);
	}

	[TestMethod]
	public async Task OnPostSpam_WithoutPermissions_ReturnsAccessDenied()
	{
		var user = _db.AddUserWithRole("TestUser").Entity;
		var topic = _db.AddTopic(user).Entity;
		var post = _db.CreatePostForTopic(topic, user).Entity;
		await _db.SaveChangesAsync();

		AddAuthenticatedUser(_model, user, [PermissionTo.CreateForumPosts]); // Missing DeleteForumPosts and AssignRoles
		_model.Id = post.Id;

		var result = await _model.OnPostSpam();

		AssertAccessDenied(result);
	}

	[TestMethod]
	public async Task OnPostSpam_NonExistentPost_ReturnsNotFound()
	{
		var user = _db.AddUserWithRole("TestUser").Entity;
		await _db.SaveChangesAsync();

		AddAuthenticatedUser(_model, user, [PermissionTo.DeleteForumPosts, PermissionTo.AssignRoles]);
		_model.Id = 999;

		var result = await _model.OnPostSpam();

		AssertForumNotFound(result);
	}

	[TestMethod]
	public async Task OnPostSpam_RestrictedPost_ReturnsNotFound()
	{
		var user = _db.AddUserWithRole("TestUser").Entity;
		var topic = _db.AddTopic(user, true).Entity;
		var post = _db.CreatePostForTopic(topic, user).Entity;
		await _db.SaveChangesAsync();

		AddAuthenticatedUser(_model, user, [PermissionTo.DeleteForumPosts, PermissionTo.AssignRoles, PermissionTo.SeeRestrictedForums]);
		_model.Id = post.Id;

		var result = await _model.OnPostSpam();

		AssertForumNotFound(result);
	}

	[TestMethod]
	public async Task OnPostSpam_PosterCannotBeSpammed_ReturnsAccessDenied()
	{
		var user = _db.AddUserWithRole("TestUser").Entity;
		var adminUser = _db.AddUser("AdminUser").Entity; // User with AssignRoles cannot be spammed
		var adminRole = _db.Roles.Add(new Role { Name = "Admin" }).Entity;
		_db.RolePermission.Add(new RolePermission { PermissionId = PermissionTo.AssignRoles, Role = adminRole });
		_db.UserRoles.Add(new UserRole { User = adminUser, Role = adminRole });
		var topic = _db.AddTopic(user).Entity;
		var post = _db.CreatePostForTopic(topic, adminUser).Entity;
		await _db.SaveChangesAsync();

		AddAuthenticatedUser(_model, user, [PermissionTo.DeleteForumPosts, PermissionTo.AssignRoles]);
		_model.Id = post.Id;

		var result = await _model.OnPostSpam();

		AssertAccessDenied(result);
	}

	[TestMethod]
	public async Task OnPostSpam_ValidSpam_MovesPostToSpamAndBansUser()
	{
		var user = _db.AddUserWithRole("TestUser").Entity;
		var spamUser = _db.AddUser("SpamUser").Entity;

		// Create spam forum and topic
		var spamForum = _db.AddForum("Spam Forum").Entity;
		spamForum.Id = SiteGlobalConstants.SpamForumId;
		var spamTopic = _db.AddTopic(user).Entity;
		spamTopic.Id = SiteGlobalConstants.SpamTopicId;
		spamTopic.Forum = spamForum;

		var topic = _db.AddTopic(user).Entity;
		var post = _db.CreatePostForTopic(topic, spamUser).Entity;
		await _db.SaveChangesAsync();

		AddAuthenticatedUser(_model, user, [PermissionTo.DeleteForumPosts, PermissionTo.AssignRoles]);
		_model.Id = post.Id;

		var result = await _model.OnPostSpam();

		AssertRedirect(result, "/Forum/Subforum/Index");

		// Verify post was moved to spam
		await _db.Entry(post).ReloadAsync();
		Assert.AreEqual(SiteGlobalConstants.SpamTopicId, post.TopicId);
		Assert.AreEqual(SiteGlobalConstants.SpamForumId, post.ForumId);

		// Verify user was banned
		await _userManager.Received(1).PermaBanUser(spamUser.Id);

		// Verify cache was cleared
		_forumService.Received(1).ClearLatestPostCache();
		_forumService.Received(1).ClearTopicActivityCache();

		await _publisher.Received(1).Send(Arg.Any<Post>());
	}
}
