using TASVideos.Core;
using TASVideos.Core.Services;
using TASVideos.Pages.Forum.Posts;

namespace TASVideos.RazorPages.Tests.Pages.Forum.Posts;

[TestClass]
public class NewModelTests : BasePageModelTests
{
	private readonly IUserManager _userManager;
	private readonly NewModel _model;

	public NewModelTests()
	{
		_userManager = Substitute.For<IUserManager>();
		_model = new NewModel(_db, _userManager)
		{
			PageContext = TestPageContext()
		};
	}

	[TestMethod]
	public async Task OnGet_NoPosts_ReturnsEmptyPagedResult()
	{
		var user = _db.AddUser("TestUser").Entity;
		user.LastLoggedInTimeStamp = DateTime.UtcNow.AddDays(-1);
		await _db.SaveChangesAsync();
		_userManager.GetRequiredUser(Arg.Any<System.Security.Claims.ClaimsPrincipal>()).Returns(user);

		await _model.OnGet();

		Assert.AreEqual(0, _model.Posts.Count());
		Assert.AreEqual(0, _model.Posts.RowCount);
	}

	[TestMethod]
	public async Task OnGet_PostsSinceLastLogin_ReturnsNewPosts()
	{
		var user = _db.AddUser("TestUser").Entity;
		user.LastLoggedInTimeStamp = DateTime.UtcNow.AddDays(-2);
		var topic = _db.AddTopic(user).Entity;
		topic.Title = "Test Topic";

		var newPost = _db.CreatePostForTopic(topic, user).Entity;
		newPost.CreateTimestamp = DateTime.UtcNow.AddDays(-1); // After last login
		newPost.Text = "New post text";

		await _db.SaveChangesAsync();

		_userManager.GetRequiredUser(Arg.Any<System.Security.Claims.ClaimsPrincipal>()).Returns(user);

		await _model.OnGet();

		Assert.AreEqual(1, _model.Posts.Count());
		var latestPost = _model.Posts.First();
		Assert.AreEqual(newPost.Id, latestPost.Id);
		Assert.AreEqual("Test Topic", latestPost.TopicTitle);
		Assert.AreEqual("New post text", latestPost.Text);
		Assert.AreEqual("TestUser", latestPost.PosterName);
	}

	[TestMethod]
	public async Task OnGet_PostsBeforeLastLogin_ExcludesOldPosts()
	{
		var user = _db.AddUser("TestUser").Entity;
		user.LastLoggedInTimeStamp = DateTime.UtcNow.AddDays(-1);
		var topic = _db.AddTopic(user).Entity;

		var oldPost = _db.CreatePostForTopic(topic, user).Entity;
		oldPost.CreateTimestamp = DateTime.UtcNow.AddDays(-2); // Before last login

		var newPost = _db.CreatePostForTopic(topic, user).Entity;
		newPost.CreateTimestamp = DateTime.UtcNow.AddHours(-1); // After last login

		await _db.SaveChangesAsync();

		_userManager.GetRequiredUser(Arg.Any<System.Security.Claims.ClaimsPrincipal>()).Returns(user);

		await _model.OnGet();

		Assert.AreEqual(1, _model.Posts.Count());
		var latestPost = _model.Posts.First();
		Assert.AreEqual(newPost.Id, latestPost.Id);
	}

	[TestMethod]
	public async Task OnGet_UserWithNullLastLogin_UsesCurrentTime()
	{
		var user = _db.AddUser("TestUser").Entity;
		user.LastLoggedInTimeStamp = null; // Never logged in before
		var topic = _db.AddTopic(user).Entity;

		var oldPost = _db.CreatePostForTopic(topic, user).Entity;
		oldPost.CreateTimestamp = DateTime.UtcNow.AddDays(-1);

		await _db.SaveChangesAsync();

		_userManager.GetRequiredUser(Arg.Any<System.Security.Claims.ClaimsPrincipal>()).Returns(user);

		await _model.OnGet();

		// Should return no posts since we use current time as baseline for null LastLoggedInTimeStamp
		Assert.AreEqual(0, _model.Posts.Count());
	}

	[TestMethod]
	public async Task OnGet_PostsOrderedByCreateTimestamp_ReturnsNewestFirst()
	{
		var user = _db.AddUser("TestUser").Entity;
		user.LastLoggedInTimeStamp = DateTime.UtcNow.AddDays(-3);
		var topic = _db.AddTopic(user).Entity;

		var earlierPost = _db.CreatePostForTopic(topic, user).Entity;
		earlierPost.CreateTimestamp = DateTime.UtcNow.AddDays(-2);
		earlierPost.Text = "Earlier post";

		var laterPost = _db.CreatePostForTopic(topic, user).Entity;
		laterPost.CreateTimestamp = DateTime.UtcNow.AddDays(-1);
		laterPost.Text = "Later post";

		await _db.SaveChangesAsync();

		_userManager.GetRequiredUser(Arg.Any<System.Security.Claims.ClaimsPrincipal>()).Returns(user);

		await _model.OnGet();

		Assert.AreEqual(2, _model.Posts.Count());
		var posts = _model.Posts.ToList();
		Assert.AreEqual("Later post", posts[0].Text);
		Assert.AreEqual("Earlier post", posts[1].Text);
		Assert.IsTrue(posts[0].CreateTimestamp > posts[1].CreateTimestamp);
	}

	[TestMethod]
	public async Task OnGet_RestrictedPosts_WithoutPermission_ExcludesRestrictedPosts()
	{
		var user = _db.AddUserWithRole("RegularUser").Entity;
		user.LastLoggedInTimeStamp = DateTime.UtcNow.AddDays(-2);
		var restrictedForum = _db.AddForum("Restricted Forum", true).Entity;
		var publicForum = _db.AddForum("Public Forum", false).Entity;

		var restrictedTopic = _db.AddTopic(user).Entity;
		restrictedTopic.Forum = restrictedForum;
		var publicTopic = _db.AddTopic(user).Entity;
		publicTopic.Forum = publicForum;

		var restrictedPost = _db.CreatePostForTopic(restrictedTopic, user).Entity;
		restrictedPost.CreateTimestamp = DateTime.UtcNow.AddDays(-1);
		restrictedPost.Text = "Restricted post";

		var publicPost = _db.CreatePostForTopic(publicTopic, user).Entity;
		publicPost.CreateTimestamp = DateTime.UtcNow.AddDays(-1);
		publicPost.Text = "Public post";

		await _db.SaveChangesAsync();

		AddAuthenticatedUser(_model, user, []);
		_userManager.GetRequiredUser(Arg.Any<System.Security.Claims.ClaimsPrincipal>()).Returns(user);

		await _model.OnGet();

		Assert.AreEqual(1, _model.Posts.Count());
		var latestPost = _model.Posts.First();
		Assert.AreEqual("Public post", latestPost.Text);
	}

	[TestMethod]
	public async Task OnGet_RestrictedPosts_WithPermission_IncludesRestrictedPosts()
	{
		var user = _db.AddUserWithRole("AdminUser").Entity;
		user.LastLoggedInTimeStamp = DateTime.UtcNow.AddDays(-2);
		var restrictedForum = _db.AddForum("Restricted Forum", true).Entity;
		var publicForum = _db.AddForum("Public Forum", false).Entity;

		var restrictedTopic = _db.AddTopic(user).Entity;
		restrictedTopic.Forum = restrictedForum;
		var publicTopic = _db.AddTopic(user).Entity;
		publicTopic.Forum = publicForum;

		var restrictedPost = _db.CreatePostForTopic(restrictedTopic, user).Entity;
		restrictedPost.CreateTimestamp = DateTime.UtcNow.AddDays(-1);
		restrictedPost.Text = "Restricted post";

		var publicPost = _db.CreatePostForTopic(publicTopic, user).Entity;
		publicPost.CreateTimestamp = DateTime.UtcNow.AddDays(-1);
		publicPost.Text = "Public post";

		await _db.SaveChangesAsync();

		AddAuthenticatedUser(_model, user, [PermissionTo.SeeRestrictedForums]);
		_userManager.GetRequiredUser(Arg.Any<System.Security.Claims.ClaimsPrincipal>()).Returns(user);

		await _model.OnGet();

		Assert.AreEqual(2, _model.Posts.Count());
		var posts = _model.Posts.ToList();
		Assert.IsTrue(posts.Any(p => p.Text == "Restricted post"));
		Assert.IsTrue(posts.Any(p => p.Text == "Public post"));
	}

	[TestMethod]
	public async Task OnGet_MultiplePosts_MapsAllFieldsCorrectly()
	{
		var user = _db.AddUser("TestPoster").Entity;
		user.LastLoggedInTimeStamp = DateTime.UtcNow.AddDays(-2);
		var topic = _db.AddTopic(user).Entity;
		topic.Title = "Test Topic Title";
		topic.Forum!.Name = "Test Forum";

		var post = _db.CreatePostForTopic(topic, user).Entity;
		var testTimestamp = DateTime.UtcNow.AddDays(-1);
		post.CreateTimestamp = testTimestamp;
		post.Text = "Test post content";

		await _db.SaveChangesAsync();

		_userManager.GetRequiredUser(Arg.Any<System.Security.Claims.ClaimsPrincipal>()).Returns(user);

		await _model.OnGet();

		Assert.AreEqual(1, _model.Posts.Count());
		var latestPost = _model.Posts.First();

		Assert.IsTrue(Math.Abs((testTimestamp - latestPost.CreateTimestamp).TotalMilliseconds) < 1000, "Timestamps should be within 1 second");
		Assert.AreEqual(post.Id, latestPost.Id);
		Assert.AreEqual(topic.Id, latestPost.TopicId);
		Assert.AreEqual("Test Topic Title", latestPost.TopicTitle);
		Assert.AreEqual(topic.ForumId, latestPost.ForumId);
		Assert.AreEqual("Test Forum", latestPost.ForumName);
		Assert.AreEqual("Test post content", latestPost.Text);
		Assert.AreEqual("TestPoster", latestPost.PosterName);
	}

	[TestMethod]
	public async Task OnGet_WithPagingModel_UsesCorrectPagination()
	{
		var user = _db.AddUser("TestUser").Entity;
		user.LastLoggedInTimeStamp = DateTime.UtcNow.AddDays(-2);
		var topic = _db.AddTopic(user).Entity;

		for (int i = 0; i < 30; i++)
		{
			var post = _db.CreatePostForTopic(topic, user).Entity;
			post.CreateTimestamp = DateTime.UtcNow.AddDays(-1).AddMinutes(i);
			post.Text = $"Post {i}";
		}

		await _db.SaveChangesAsync();

		_userManager.GetRequiredUser(Arg.Any<System.Security.Claims.ClaimsPrincipal>()).Returns(user);
		_model.Search = new PagingModel { CurrentPage = 2, PageSize = 10 };

		await _model.OnGet();

		Assert.AreEqual(10, _model.Posts.Count());
		Assert.AreEqual(30, _model.Posts.RowCount);
		Assert.AreEqual(2, _model.Posts.Request.CurrentPage);
	}

	[TestMethod]
	public void HasAuthorizeAttribute() => AssertRequiresAuthorization(typeof(NewModel));

	[TestMethod]
	public void HasRequireCurrentPermissionsAttribute() => AssertRequiresCurrentPermissions(typeof(NewModel));
}
