using TASVideos.Core;
using TASVideos.Pages.Forum.Posts;

namespace TASVideos.RazorPages.Tests.Pages.Forum.Posts;

[TestClass]
public class UnansweredModelTests : BasePageModelTests
{
	private readonly UnansweredModel _model;

	public UnansweredModelTests()
	{
		_model = new UnansweredModel(_db)
		{
			PageContext = TestPageContext()
		};
	}

	[TestMethod]
	public async Task OnGet_NoTopics_ReturnsEmptyPagedResult()
	{
		await _model.OnGet();

		Assert.AreEqual(0, _model.Posts.Count());
		Assert.AreEqual(0, _model.Posts.RowCount);
	}

	[TestMethod]
	public async Task OnGet_TopicsWithOnePost_ReturnsUnansweredTopics()
	{
		var user = _db.AddUser("TestUser").Entity;
		var topic = _db.AddTopic(user).Entity;
		topic.Title = "Unanswered Topic";
		topic.Forum!.Name = "Test Forum";

		var post = _db.CreatePostForTopic(topic, user).Entity;
		post.CreateTimestamp = DateTime.UtcNow.AddDays(-1);

		await _db.SaveChangesAsync();

		await _model.OnGet();

		Assert.AreEqual(1, _model.Posts.Count());
		var unansweredPost = _model.Posts.First();
		Assert.AreEqual(topic.ForumId, unansweredPost.ForumId);
		Assert.AreEqual("Test Forum", unansweredPost.ForumName);
		Assert.AreEqual(topic.Id, unansweredPost.TopicId);
		Assert.AreEqual("Unanswered Topic", unansweredPost.TopicName);
		Assert.AreEqual(user.Id, unansweredPost.AuthorId);
		Assert.AreEqual("TestUser", unansweredPost.AuthorName);
	}

	[TestMethod]
	public async Task OnGet_TopicsWithMultiplePosts_ExcludesAnsweredTopics()
	{
		var user1 = _db.AddUser("User1").Entity;
		var user2 = _db.AddUser("User2").Entity;

		var unansweredTopic = _db.AddTopic(user1).Entity;
		unansweredTopic.Title = "Unanswered Topic";
		_db.CreatePostForTopic(unansweredTopic, user1);

		var answeredTopic = _db.AddTopic(user1).Entity;
		answeredTopic.Title = "Answered Topic";
		_db.CreatePostForTopic(answeredTopic, user1);
		_db.CreatePostForTopic(answeredTopic, user2);

		await _db.SaveChangesAsync();

		await _model.OnGet();

		Assert.AreEqual(1, _model.Posts.Count());
		var result = _model.Posts.First();
		Assert.AreEqual("Unanswered Topic", result.TopicName);
	}

	[TestMethod]
	public async Task OnGet_TopicsOrderedByCreateTimestamp_ReturnsNewestFirst()
	{
		var user = _db.AddUser("TestUser").Entity;

		var olderTopic = _db.AddTopic(user).Entity;
		olderTopic.Title = "Older Topic";
		olderTopic.CreateTimestamp = DateTime.UtcNow.AddDays(-2);
		_db.CreatePostForTopic(olderTopic, user);

		var newerTopic = _db.AddTopic(user).Entity;
		newerTopic.Title = "Newer Topic";
		newerTopic.CreateTimestamp = DateTime.UtcNow.AddDays(-1);
		_db.CreatePostForTopic(newerTopic, user);

		await _db.SaveChangesAsync();

		await _model.OnGet();

		Assert.AreEqual(2, _model.Posts.Count());
		var posts = _model.Posts.ToList();
		Assert.AreEqual("Newer Topic", posts[0].TopicName);
		Assert.AreEqual("Older Topic", posts[1].TopicName);
		Assert.IsTrue(posts[0].PostDate > posts[1].PostDate);
	}

	[TestMethod]
	public async Task OnGet_RestrictedTopics_WithoutPermission_ExcludesRestrictedTopics()
	{
		var user = _db.AddUserWithRole("RegularUser").Entity;
		var restrictedForum = _db.AddForum("Restricted Forum", true).Entity;
		var publicForum = _db.AddForum("Public Forum", false).Entity;

		var restrictedTopic = _db.AddTopic(user).Entity;
		restrictedTopic.Forum = restrictedForum;
		restrictedTopic.Title = "Restricted Topic";
		_db.CreatePostForTopic(restrictedTopic, user);

		var publicTopic = _db.AddTopic(user).Entity;
		publicTopic.Forum = publicForum;
		publicTopic.Title = "Public Topic";
		_db.CreatePostForTopic(publicTopic, user);

		await _db.SaveChangesAsync();

		AddAuthenticatedUser(_model, user, []);

		await _model.OnGet();

		Assert.AreEqual(1, _model.Posts.Count());
		var result = _model.Posts.First();
		Assert.AreEqual("Public Topic", result.TopicName);
	}

	[TestMethod]
	public async Task OnGet_RestrictedTopics_WithPermission_IncludesRestrictedTopics()
	{
		var user = _db.AddUserWithRole("AdminUser").Entity;
		var restrictedForum = _db.AddForum("Restricted Forum", true).Entity;
		var publicForum = _db.AddForum("Public Forum", false).Entity;

		var restrictedTopic = _db.AddTopic(user).Entity;
		restrictedTopic.Forum = restrictedForum;
		restrictedTopic.Title = "Restricted Topic";
		_db.CreatePostForTopic(restrictedTopic, user);

		var publicTopic = _db.AddTopic(user).Entity;
		publicTopic.Forum = publicForum;
		publicTopic.Title = "Public Topic";
		_db.CreatePostForTopic(publicTopic, user);
		await _db.SaveChangesAsync();

		AddAuthenticatedUser(_model, user, [PermissionTo.SeeRestrictedForums]);

		await _model.OnGet();

		Assert.AreEqual(2, _model.Posts.Count());
		var posts = _model.Posts.ToList();
		Assert.IsTrue(posts.Any(p => p.TopicName == "Restricted Topic"));
		Assert.IsTrue(posts.Any(p => p.TopicName == "Public Topic"));
	}

	[TestMethod]
	public async Task OnGet_MultipleUnansweredTopics_MapsAllFieldsCorrectly()
	{
		var author = _db.AddUser("TestAuthor").Entity;
		var topic = _db.AddTopic(author).Entity;
		topic.Title = "Test Topic Title";
		topic.Forum!.Name = "Test Forum Name";
		var testTimestamp = DateTime.UtcNow.AddDays(-1);
		topic.CreateTimestamp = testTimestamp;
		_db.CreatePostForTopic(topic, author);
		await _db.SaveChangesAsync();

		await _model.OnGet();

		Assert.AreEqual(1, _model.Posts.Count());
		var unansweredPost = _model.Posts.First();

		Assert.AreEqual(topic.ForumId, unansweredPost.ForumId);
		Assert.AreEqual("Test Forum Name", unansweredPost.ForumName);
		Assert.AreEqual(topic.Id, unansweredPost.TopicId);
		Assert.AreEqual("Test Topic Title", unansweredPost.TopicName);
		Assert.AreEqual(author.Id, unansweredPost.AuthorId);
		Assert.AreEqual("TestAuthor", unansweredPost.AuthorName);
		Assert.IsTrue(Math.Abs((testTimestamp - unansweredPost.PostDate).TotalMilliseconds) < 1000, "Timestamps should be within 1 second");
	}

	[TestMethod]
	public async Task OnGet_WithPagingModel_UsesCorrectPagination()
	{
		var user = _db.AddUser("TestUser").Entity;

		for (int i = 0; i < 30; i++)
		{
			var topic = _db.AddTopic(user).Entity;
			topic.Title = $"Topic {i}";
			topic.CreateTimestamp = DateTime.UtcNow.AddDays(-1).AddMinutes(i);
			_db.CreatePostForTopic(topic, user);
		}

		await _db.SaveChangesAsync();

		_model.Search = new PagingModel { CurrentPage = 2, PageSize = 10 };

		await _model.OnGet();

		Assert.AreEqual(10, _model.Posts.Count());
		Assert.AreEqual(30, _model.Posts.RowCount);
		Assert.AreEqual(2, _model.Posts.Request.CurrentPage);
	}

	[TestMethod]
	public void AllowsAnonymousAttribute() => AssertAllowsAnonymousUsers(typeof(UnansweredModel));
}
