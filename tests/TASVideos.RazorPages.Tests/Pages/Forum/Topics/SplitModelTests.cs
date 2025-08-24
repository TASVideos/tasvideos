using TASVideos.Core.Services;
using TASVideos.Core.Services.ExternalMediaPublisher;
using TASVideos.Pages.Forum.Topics;
using TASVideos.Services;

namespace TASVideos.RazorPages.Tests.Pages.Forum.Topics;

[TestClass]
public class SplitModelTests : BasePageModelTests
{
	private readonly IExternalMediaPublisher _publisher;
	private readonly IForumService _forumService;
	private readonly SplitModel _model;

	public SplitModelTests()
	{
		_publisher = Substitute.For<IExternalMediaPublisher>();
		_forumService = Substitute.For<IForumService>();

		_model = new SplitModel(_db, _publisher, _forumService)
		{
			PageContext = TestPageContext()
		};
	}

	[TestMethod]
	public async Task OnGet_NonExistentTopic_ReturnsNotFound()
	{
		_model.Id = 999;

		var result = await _model.OnGet();

		Assert.IsInstanceOfType(result, typeof(NotFoundResult));
	}

	[TestMethod]
	public async Task OnGet_ExistingTopic_PopulatesTopicSplit()
	{
		var user = _db.AddUserWithRole("TestUser").Entity;
		var topic = _db.AddTopic(user).Entity;
		topic.Title = "Test Topic";
		_db.CreatePostForTopic(topic, user);
		await _db.SaveChangesAsync();

		_model.Id = topic.Id;

		var result = await _model.OnGet();

		Assert.IsInstanceOfType(result, typeof(PageResult));
		Assert.AreEqual(topic.Title, _model.Topic.Title);
		Assert.AreEqual($"(Split from {topic.Title})", _model.Topic.NewTopicName);
		Assert.AreEqual(topic.Forum!.Id, _model.Topic.CreateNewTopicIn);
		Assert.AreEqual(topic.Forum.Id, _model.Topic.ForumId);
		Assert.AreEqual(topic.Forum.Name, _model.Topic.ForumName);
		Assert.AreEqual(1, _model.Topic.PostsCount);
	}

	[TestMethod]
	public async Task OnGet_ExistingTopic_PopulatesAvailableForums()
	{
		var user = _db.AddUserWithRole("TestUser").Entity;
		var topic = _db.AddTopic(user).Entity;
		await _db.SaveChangesAsync();

		_model.Id = topic.Id;

		var result = await _model.OnGet();

		Assert.IsInstanceOfType(result, typeof(PageResult));
		Assert.IsTrue(_model.AvailableForums.Count > 0);
	}

	[TestMethod]
	public async Task OnGet_TopicWithMultiplePosts_PopulatesPosts()
	{
		var user = _db.AddUserWithRole("TestUser").Entity;
		var topic = _db.AddTopic(user).Entity;
		var post1 = _db.CreatePostForTopic(topic, user).Entity;
		post1.Subject = "First Post";
		post1.Text = "First post content";
		var post2 = _db.CreatePostForTopic(topic, user).Entity;
		post2.Subject = "Second Post";
		post2.Text = "Second post content";
		await _db.SaveChangesAsync();

		_model.Id = topic.Id;

		var result = await _model.OnGet();

		Assert.IsInstanceOfType(result, typeof(PageResult));
		Assert.AreEqual(2, _model.Topic.Posts.Count);
		Assert.AreEqual("First Post", _model.Topic.Posts[0].Subject);
		Assert.AreEqual("Second Post", _model.Topic.Posts[1].Subject);
		Assert.AreEqual(user.UserName, _model.Topic.Posts[0].PosterName);
	}

	[TestMethod]
	public async Task OnGet_RestrictedTopic_WithoutPermission_ReturnsNotFound()
	{
		var user = _db.AddUserWithRole("TestUser").Entity;
		var restrictedForum = _db.AddForum("Restricted Forum", true).Entity;
		var topic = _db.AddTopic(user).Entity;
		topic.Forum = restrictedForum;
		await _db.SaveChangesAsync();

		AddAuthenticatedUser(_model, user, [PermissionTo.SplitTopics]);
		_model.Id = topic.Id;

		var result = await _model.OnGet();

		Assert.IsInstanceOfType(result, typeof(NotFoundResult));
	}

	[TestMethod]
	public async Task OnPost_InvalidModelState_ReturnsPage()
	{
		_model.ModelState.AddModelError("TestError", "Test error message");

		var result = await _model.OnPost();

		Assert.IsInstanceOfType(result, typeof(PageResult));
	}

	[TestMethod]
	public async Task OnPost_NonExistentTopic_ReturnsNotFound()
	{
		_model.Id = 999;
		_model.Topic = new SplitModel.TopicSplit
		{
			CreateNewTopicIn = 1,
			NewTopicName = "New Topic"
		};

		var result = await _model.OnPost();

		Assert.IsInstanceOfType(result, typeof(NotFoundResult));
	}

	[TestMethod]
	public async Task OnPost_NonExistentDestinationForum_ReturnsNotFound()
	{
		var user = _db.AddUserWithRole("TestUser").Entity;
		var topic = _db.AddTopic(user).Entity;
		await _db.SaveChangesAsync();

		AddAuthenticatedUser(_model, user, [PermissionTo.SplitTopics]);
		_model.Id = topic.Id;
		_model.Topic = new SplitModel.TopicSplit
		{
			CreateNewTopicIn = 999,
			NewTopicName = "New Topic",
			Title = topic.Title
		};

		var result = await _model.OnPost();

		Assert.IsInstanceOfType(result, typeof(NotFoundResult));
	}

	[TestMethod]
	public async Task OnPost_SplitBySelectedPosts_CreatesNewTopicAndMovesPosts()
	{
		var user = _db.AddUserWithRole("TestUser").Entity;
		var topic = _db.AddTopic(user).Entity;
		topic.Title = "Original Topic";
		var post1 = _db.CreatePostForTopic(topic, user).Entity;
		var post2 = _db.CreatePostForTopic(topic, user).Entity;
		var post3 = _db.CreatePostForTopic(topic, user).Entity;
		await _db.SaveChangesAsync();

		AddAuthenticatedUser(_model, user, [PermissionTo.SplitTopics]);
		_model.Id = topic.Id;
		_model.Topic = new SplitModel.TopicSplit
		{
			CreateNewTopicIn = topic.ForumId,
			NewTopicName = "Split Topic",
			Title = topic.Title,
			Posts = [
				new SplitModel.TopicSplit.Post { Id = post1.Id, Selected = false },
				new SplitModel.TopicSplit.Post { Id = post2.Id, Selected = true },
				new SplitModel.TopicSplit.Post { Id = post3.Id, Selected = true }
			]
		};

		var result = await _model.OnPost();

		AssertRedirect(result, "Index");

		// Verify new topic was created
		var newTopic = await _db.ForumTopics.SingleOrDefaultAsync(t => t.Title == "Split Topic");
		Assert.IsNotNull(newTopic);
		Assert.AreEqual(topic.ForumId, newTopic.ForumId);
		Assert.AreEqual(user.Id, newTopic.PosterId);

		// Verify posts were moved
		await _db.Entry(post1).ReloadAsync();
		await _db.Entry(post2).ReloadAsync();
		await _db.Entry(post3).ReloadAsync();
		Assert.AreEqual(topic.Id, post1.TopicId); // Not selected, stays in original
		Assert.AreEqual(newTopic.Id, post2.TopicId); // Selected, moved to new topic
		Assert.AreEqual(newTopic.Id, post3.TopicId); // Selected, moved to new topic
	}

	[TestMethod]
	public async Task OnPost_SplitFromSpecificPost_MovesPostsFromTimestamp()
	{
		var user = _db.AddUserWithRole("TestUser").Entity;
		var topic = _db.AddTopic(user).Entity;
		topic.Title = "Original Topic";
		var post1 = _db.CreatePostForTopic(topic, user).Entity;
		post1.CreateTimestamp = DateTime.UtcNow.AddMinutes(-10);
		var post2 = _db.CreatePostForTopic(topic, user).Entity;
		post2.CreateTimestamp = DateTime.UtcNow.AddMinutes(-5);
		var post3 = _db.CreatePostForTopic(topic, user).Entity;
		post3.CreateTimestamp = DateTime.UtcNow;
		await _db.SaveChangesAsync();

		AddAuthenticatedUser(_model, user, [PermissionTo.SplitTopics]);
		_model.Id = topic.Id;
		_model.Topic = new SplitModel.TopicSplit
		{
			CreateNewTopicIn = topic.ForumId,
			NewTopicName = "Split Topic",
			Title = topic.Title,
			SplitPostsStartingAt = post2.Id,
			Posts = [] // No selected posts, will use SplitPostsStartingAt
		};

		var result = await _model.OnPost();

		AssertRedirect(result, "Index");

		// Verify new topic was created
		var newTopic = await _db.ForumTopics
			.Where(t => t.Title == "Split Topic")
			.SingleOrDefaultAsync();
		Assert.IsNotNull(newTopic);

		// Verify posts were moved correctly
		await _db.Entry(post1).ReloadAsync();
		await _db.Entry(post2).ReloadAsync();
		await _db.Entry(post3).ReloadAsync();
		Assert.AreEqual(topic.Id, post1.TopicId); // Before split point, stays in original
		Assert.AreEqual(newTopic.Id, post2.TopicId); // At split point, moved to new topic
		Assert.AreEqual(newTopic.Id, post3.TopicId); // After split point, moved to new topic
	}

	[TestMethod]
	public async Task OnPost_SplitAcrossForums_UpdatesPostForumIds()
	{
		var user = _db.AddUserWithRole("TestUser").Entity;
		var sourceForum = _db.AddForum("Source Forum").Entity;
		var targetForum = _db.AddForum("Target Forum").Entity;
		var topic = _db.AddTopic(user).Entity;
		topic.Forum = sourceForum;
		var post = _db.CreatePostForTopic(topic, user).Entity;
		await _db.SaveChangesAsync();

		AddAuthenticatedUser(_model, user, [PermissionTo.SplitTopics]);
		_model.Id = topic.Id;
		_model.Topic = new SplitModel.TopicSplit
		{
			CreateNewTopicIn = targetForum.Id,
			NewTopicName = "Split Topic",
			Title = topic.Title,
			Posts = [new SplitModel.TopicSplit.Post { Id = post.Id, Selected = true }]
		};

		var result = await _model.OnPost();

		AssertRedirect(result, "Index");

		// Verify new topic was created in target forum
		var newTopic = await _db.ForumTopics
			.SingleOrDefaultAsync(t => t.Title == "Split Topic");
		Assert.IsNotNull(newTopic);
		Assert.AreEqual(targetForum.Id, newTopic.ForumId);

		// Verify post was moved to target forum
		await _db.Entry(post).ReloadAsync();
		Assert.AreEqual(targetForum.Id, post.ForumId);
		Assert.AreEqual(newTopic.Id, post.TopicId);
	}

	[TestMethod]
	public async Task OnPost_ValidSplit_ClearsCacheAndSendsNotification()
	{
		var user = _db.AddUserWithRole("TestUser").Entity;
		var topic = _db.AddTopic(user).Entity;
		topic.Title = "Original Topic";
		var post = _db.CreatePostForTopic(topic, user).Entity;
		await _db.SaveChangesAsync();

		AddAuthenticatedUser(_model, user, [PermissionTo.SplitTopics]);
		_model.Id = topic.Id;
		_model.Topic = new SplitModel.TopicSplit
		{
			CreateNewTopicIn = topic.ForumId,
			NewTopicName = "Split Topic",
			Title = topic.Title,
			Posts = [new SplitModel.TopicSplit.Post { Id = post.Id, Selected = true }]
		};

		var result = await _model.OnPost();

		AssertRedirect(result, "Index");
		_forumService.Received(1).ClearLatestPostCache();
		_forumService.Received(1).ClearTopicActivityCache();
		await _publisher.Received(1).Send(Arg.Any<Post>());
	}

	[TestMethod]
	public async Task OnPost_SplitWithRestrictedForums_SendsRestrictedNotification()
	{
		var user = _db.AddUserWithRole("TestUser").Entity;
		var restrictedForum = _db.AddForum("Restricted Forum", true).Entity;
		var topic = _db.AddTopic(user).Entity;
		topic.Forum = restrictedForum;
		var post = _db.CreatePostForTopic(topic, user).Entity;
		await _db.SaveChangesAsync();

		AddAuthenticatedUser(_model, user, [PermissionTo.SplitTopics, PermissionTo.SeeRestrictedForums]);
		_model.Id = topic.Id;
		_model.Topic = new SplitModel.TopicSplit
		{
			CreateNewTopicIn = restrictedForum.Id,
			NewTopicName = "Split Topic",
			Title = topic.Title,
			Posts = [new SplitModel.TopicSplit.Post { Id = post.Id, Selected = true }]
		};

		var result = await _model.OnPost();

		AssertRedirect(result, "Index");
		await _publisher.Received(1).Send(Arg.Any<Post>());
	}

	[TestMethod]
	public async Task OnPost_NoSelectedPostsAndInvalidSplitPost_ReturnsPage()
	{
		var user = _db.AddUserWithRole("TestUser").Entity;
		var topic = _db.AddTopic(user).Entity;
		_db.CreatePostForTopic(topic, user);
		await _db.SaveChangesAsync();

		AddAuthenticatedUser(_model, user, [PermissionTo.SplitTopics]);
		_model.Id = topic.Id;
		_model.Topic = new SplitModel.TopicSplit
		{
			CreateNewTopicIn = topic.ForumId,
			NewTopicName = "Split Topic",
			Title = topic.Title,
			SplitPostsStartingAt = 999, // Non-existent post
			Posts = [] // No selected posts
		};

		var result = await _model.OnPost();

		Assert.IsInstanceOfType(result, typeof(PageResult));
	}

	[TestMethod]
	public async Task OnGet_WithPagination_LoadsCorrectPosts()
	{
		var user = _db.AddUserWithRole("TestUser").Entity;
		var topic = _db.AddTopic(user).Entity;

		// Create enough posts to test pagination (more than 500)
		for (int i = 1; i <= 502; i++)
		{
			var post = _db.CreatePostForTopic(topic, user).Entity;
			post.Subject = $"Post {i}";
			post.CreateTimestamp = DateTime.UtcNow.AddMinutes(i);
		}

		await _db.SaveChangesAsync();

		_model.Id = topic.Id;
		_model.CurrentPage = 1;

		var result = await _model.OnGet();

		Assert.IsInstanceOfType(result, typeof(PageResult));
		Assert.AreEqual(502, _model.Topic.PostsCount);
		Assert.AreEqual(2, _model.TotalPages);
		Assert.AreEqual(1, _model.CurrentPage);
		Assert.AreEqual(2, _model.Topic.Posts.Count); // Should load remainder posts (502 % 500 = 2)
	}
}
