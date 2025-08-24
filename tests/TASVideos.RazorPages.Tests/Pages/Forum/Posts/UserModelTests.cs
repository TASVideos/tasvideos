using TASVideos.Core.Services;
using TASVideos.Pages.Forum.Posts;
using static TASVideos.Pages.Forum.Topics.IndexModel;

namespace TASVideos.RazorPages.Tests.Pages.Forum.Posts;

[TestClass]
public class UserModelTests : BasePageModelTests
{
	private readonly IAwards _awards;
	private readonly IPointsService _pointsService;
	private readonly UserModel _model;

	public UserModelTests()
	{
		_awards = Substitute.For<IAwards>();
		_awards.ForUser(Arg.Any<int>()).Returns([]);
		_pointsService = Substitute.For<IPointsService>();
		_pointsService.PlayerPoints(Arg.Any<int>()).Returns((0.0, ""));
		_model = new UserModel(_db, _awards, _pointsService)
		{
			PageContext = TestPageContext()
		};
	}

	[TestMethod]
	public async Task OnGet_NonExistentUser_ReturnsNotFound()
	{
		_model.UserName = "NonExistentUser";
		var result = await _model.OnGet();
		Assert.IsInstanceOfType(result, typeof(NotFoundResult));
	}

	[TestMethod]
	public async Task OnGet_UserWithPosts_ReturnsUserPosts()
	{
		var user = _db.AddUser("TestUser").Entity;
		var topic = _db.AddTopic(user).Entity;
		topic.Title = "Test Topic";
		topic.Forum!.Name = "Test Forum";

		var post = _db.CreatePostForTopic(topic, user).Entity;
		post.Text = "Test post content";
		post.Subject = "Test Subject";
		post.CreateTimestamp = DateTime.UtcNow.AddDays(-1);

		await _db.SaveChangesAsync();
		_model.UserName = "TestUser";

		var result = await _model.OnGet();

		Assert.IsInstanceOfType(result, typeof(PageResult));
		Assert.AreEqual(1, _model.Posts.Count());
		var userPost = _model.Posts.First();
		Assert.AreEqual(post.Id, userPost.Id);
		Assert.AreEqual("Test post content", userPost.Text);
		Assert.AreEqual("Test Subject", userPost.Subject);
		Assert.AreEqual("Test Topic", userPost.TopicTitle);
		Assert.AreEqual("Test Forum", userPost.ForumName);
	}

	[TestMethod]
	public async Task OnGet_PostsOrderedByCreateTimestamp_ReturnsNewestFirst()
	{
		var user = _db.AddUser("TestUser").Entity;
		var topic = _db.AddTopic(user).Entity;

		var olderPost = _db.CreatePostForTopic(topic, user).Entity;
		olderPost.Text = "Older post";
		olderPost.CreateTimestamp = DateTime.UtcNow.AddDays(-2);

		var newerPost = _db.CreatePostForTopic(topic, user).Entity;
		newerPost.Text = "Newer post";
		newerPost.CreateTimestamp = DateTime.UtcNow.AddDays(-1);
		await _db.SaveChangesAsync();

		_model.UserName = "TestUser";

		await _model.OnGet();

		Assert.AreEqual(2, _model.Posts.Count());
		var posts = _model.Posts.ToList();
		Assert.AreEqual("Newer post", posts[0].Text);
		Assert.AreEqual("Older post", posts[1].Text);
		Assert.IsTrue(posts[0].CreateTimestamp > posts[1].CreateTimestamp);
	}

	[TestMethod]
	public async Task OnGet_RestrictedPosts_WithoutPermission_ExcludesRestrictedPosts()
	{
		var user = _db.AddUserWithRole("RegularUser").Entity;
		var publicForum = _db.AddForum("Public Forum", false).Entity;
		var restrictedTopic = _db.AddTopic(user, true).Entity;
		var restrictedPost = _db.CreatePostForTopic(restrictedTopic, user).Entity;
		restrictedPost.Text = "Restricted post";

		var publicTopic = _db.AddTopic(user).Entity;
		publicTopic.Forum = publicForum;
		var publicPost = _db.CreatePostForTopic(publicTopic, user).Entity;
		publicPost.Text = "Public post";
		await _db.SaveChangesAsync();

		AddAuthenticatedUser(_model, user, []);
		_model.UserName = user.UserName;

		await _model.OnGet();

		Assert.AreEqual(1, _model.Posts.Count());
		Assert.AreEqual("Public post", _model.Posts.First().Text);
	}

	[TestMethod]
	public async Task OnGet_RestrictedPosts_WithPermission_IncludesRestrictedPosts()
	{
		var user = _db.AddUserWithRole("AdminUser").Entity;

		var restrictedTopic = _db.AddTopic(user, true).Entity;
		var restrictedPost = _db.CreatePostForTopic(restrictedTopic, user).Entity;
		restrictedPost.Text = "Restricted post";

		var publicTopic = _db.AddTopic(user).Entity;
		var publicPost = _db.CreatePostForTopic(publicTopic, user).Entity;
		publicPost.Text = "Public post";

		await _db.SaveChangesAsync();

		AddAuthenticatedUser(_model, user, [PermissionTo.SeeRestrictedForums]);
		_model.UserName = user.UserName;

		await _model.OnGet();

		Assert.AreEqual(2, _model.Posts.Count());
		var posts = _model.Posts.ToList();
		Assert.IsTrue(posts.Any(p => p.Text == "Restricted post"));
		Assert.IsTrue(posts.Any(p => p.Text == "Public post"));
	}

	[TestMethod]
	public async Task OnGet_PopulatesUserProfileData_FromUserRecord()
	{
		var user = _db.AddUser("TestUser").Entity;
		user.From = "Test Location";
		user.Avatar = "test-avatar.png";
		user.MoodAvatarUrlBase = "test-mood-base";
		user.Signature = "Test signature";
		user.PreferredPronouns = PreferredPronounTypes.TheyThem;
		user.BannedUntil = null;

		var topic = _db.AddTopic(user).Entity;
		_db.CreatePostForTopic(topic, user);

		await _db.SaveChangesAsync();
		_pointsService.PlayerPoints(user.Id).Returns((150.0, "Master"));

		_model.UserName = "TestUser";

		await _model.OnGet();

		var userPost = _model.Posts.First();
		Assert.AreEqual("TestUser", userPost.PosterName);
		Assert.AreEqual("Test Location", userPost.PosterLocation);
		Assert.AreEqual("test-avatar.png", userPost.PosterAvatar);
		Assert.AreEqual("test-mood-base", userPost.PosterMoodUrlBase);
		Assert.AreEqual("Test signature", userPost.Signature);
		Assert.AreEqual(PreferredPronounTypes.TheyThem, userPost.PosterPronouns);
		Assert.AreEqual(150.0, userPost.PosterPlayerPoints);
		Assert.AreEqual("Master", userPost.PosterPlayerRank);
		Assert.IsFalse(userPost.PosterIsBanned);
	}

	[TestMethod]
	public async Task OnGet_PopulatesUserAwards_AndPlayerPoints()
	{
		var user = _db.AddUser("TestUser").Entity;
		var topic = _db.AddTopic(user).Entity;
		_db.CreatePostForTopic(topic, user);
		await _db.SaveChangesAsync();

		var testAwards = new List<AwardAssignmentSummary>
		{
			new("TestAward", "Test Award Description", 2023)
		};
		_awards.ForUser(user.Id).Returns(testAwards);
		_pointsService.PlayerPoints(user.Id).Returns((99.9, "Novice"));

		_model.UserName = "TestUser";

		await _model.OnGet();

		var userPost = _model.Posts.First();
		Assert.AreEqual(testAwards.Count, userPost.Awards.Count);
		Assert.AreEqual("TestAward", userPost.Awards.First().ShortName);
		await _awards.Received(1).ForUser(user.Id);
		Assert.AreEqual(99.9, userPost.PosterPlayerPoints);
		Assert.AreEqual("Novice", userPost.PosterPlayerRank);
	}

	[TestMethod]
	public async Task OnGet_BannedUser_SetsPosterIsBannedCorrectly()
	{
		var user = _db.AddUser("BannedUser").Entity;
		user.BannedUntil = DateTime.UtcNow.AddDays(1);
		var topic = _db.AddTopic(user).Entity;
		_db.CreatePostForTopic(topic, user);
		await _db.SaveChangesAsync();
		_model.UserName = "BannedUser";

		await _model.OnGet();

		var userPost = _model.Posts.First();
		Assert.IsTrue(userPost.PosterIsBanned);
	}

	[TestMethod]
	public async Task OnGet_PreviouslyBannedUser_SetsPosterIsBannedFalse()
	{
		var user = _db.AddUser("FormerlyBannedUser").Entity;
		user.BannedUntil = DateTime.UtcNow.AddDays(-1); // Ban expired yesterday
		var topic = _db.AddTopic(user).Entity;
		_db.CreatePostForTopic(topic, user);
		await _db.SaveChangesAsync();
		_model.UserName = "FormerlyBannedUser";

		await _model.OnGet();

		var userPost = _model.Posts.First();
		Assert.IsFalse(userPost.PosterIsBanned);
	}

	[TestMethod]
	public async Task OnGet_EditPermissions_OwnerCanEditInOpenTopic()
	{
		var user = _db.AddUser("TestUser").Entity;
		var topic = _db.AddTopic(user).Entity;
		topic.IsLocked = false;
		_db.CreatePostForTopic(topic, user);
		await _db.SaveChangesAsync();
		AddAuthenticatedUser(_model, user, [PermissionTo.EditForumPosts]);
		_model.UserName = "TestUser";

		await _model.OnGet();

		var userPost = _model.Posts.First();
		Assert.IsTrue(userPost.IsEditable);
	}

	[TestMethod]
	public async Task OnGet_EditPermissions_OwnerCannotEditInLockedTopic()
	{
		var user = _db.AddUser("TestUser").Entity;
		var topic = _db.AddTopic(user).Entity;
		topic.IsLocked = true; // Locked topic
		_db.CreatePostForTopic(topic, user);
		await _db.SaveChangesAsync();
		AddAuthenticatedUser(_model, user, [PermissionTo.EditForumPosts]);
		_model.UserName = "TestUser";

		await _model.OnGet();

		var userPost = _model.Posts.First();
		Assert.IsFalse(userPost.IsEditable);
	}

	[TestMethod]
	public async Task OnGet_EditPermissions_ModeratorCanEditAnyPost()
	{
		var user = _db.AddUser("TestUser").Entity;
		var moderator = _db.AddUser("Moderator").Entity;
		var topic = _db.AddTopic(user).Entity;
		topic.IsLocked = true; // Even locked topics
		_db.CreatePostForTopic(topic, user);
		await _db.SaveChangesAsync();

		AddAuthenticatedUser(_model, moderator, [PermissionTo.EditUsersForumPosts]);
		_model.UserName = "TestUser";

		await _model.OnGet();

		var userPost = _model.Posts.First();
		Assert.IsTrue(userPost.IsEditable);
	}

	[TestMethod]
	public async Task OnGet_DeletePermissions_PostIsDeletable()
	{
		var user = _db.AddUser("TestUser").Entity;
		var moderator = _db.AddUser("Moderator").Entity;
		var topic = _db.AddTopic(user).Entity;
		_db.CreatePostForTopic(topic, user);
		await _db.SaveChangesAsync();

		AddAuthenticatedUser(_model, moderator, [PermissionTo.DeleteForumPosts]);
		_model.UserName = "TestUser";

		await _model.OnGet();

		var userPost = _model.Posts.First();
		Assert.IsTrue(userPost.IsDeletable);
	}

	[TestMethod]
	public async Task OnGet_DeletePermissions_RegularUsersCannotDelete()
	{
		var user = _db.AddUser("TestUser").Entity;
		var topic = _db.AddTopic(user).Entity;
		_db.CreatePostForTopic(topic, user);
		await _db.SaveChangesAsync();
		_model.UserName = "TestUser";

		await _model.OnGet();

		var userPost = _model.Posts.First();
		Assert.IsFalse(userPost.IsDeletable);
	}

	[TestMethod]
	public async Task OnGet_WithPagingModel_UsesCorrectPagination()
	{
		var user = _db.AddUser("TestUser").Entity;
		var topic = _db.AddTopic(user).Entity;

		for (int i = 0; i < 30; i++)
		{
			var post = _db.CreatePostForTopic(topic, user).Entity;
			post.Text = $"Post {i}";
			post.CreateTimestamp = DateTime.UtcNow.AddDays(-1).AddMinutes(i);
		}

		await _db.SaveChangesAsync();

		_model.UserName = "TestUser";
		_model.Search = new TopicRequest { CurrentPage = 2, PageSize = 10 };

		await _model.OnGet();

		Assert.AreEqual(10, _model.Posts.Count());
		Assert.AreEqual(30, _model.Posts.RowCount);
		Assert.AreEqual(2, _model.Posts.Request.CurrentPage);
	}

	[TestMethod]
	public async Task OnGet_FiltersByUserId_OnlyShowsPostsFromSpecifiedUser()
	{
		var user1 = _db.AddUser("User1").Entity;
		var user2 = _db.AddUser("User2").Entity;
		var topic = _db.AddTopic(user1).Entity;

		var post1 = _db.CreatePostForTopic(topic, user1).Entity;
		post1.Text = "Post by User1";

		var post2 = _db.CreatePostForTopic(topic, user2).Entity;
		post2.Text = "Post by User2";

		await _db.SaveChangesAsync();
		_model.UserName = "User1";

		await _model.OnGet();

		Assert.AreEqual(1, _model.Posts.Count());
		Assert.AreEqual("Post by User1", _model.Posts.First().Text);
	}

	[TestMethod]
	public void AllowsAnonymousAttribute() => AssertAllowsAnonymousUsers(typeof(UserModel));
}
