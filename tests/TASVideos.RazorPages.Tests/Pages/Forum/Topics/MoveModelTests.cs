using TASVideos.Core.Services;
using TASVideos.Core.Services.ExternalMediaPublisher;
using TASVideos.Data.Entity;
using TASVideos.Pages.Forum.Topics;
using TASVideos.Services;
using static TASVideos.RazorPages.Tests.RazorTestHelpers;

namespace TASVideos.RazorPages.Tests.Pages.Forum.Topics;

[TestClass]
public class MoveModelTests : BasePageModelTests
{
	private readonly IExternalMediaPublisher _publisher;
	private readonly IForumService _forumService;
	private readonly MoveModel _model;

	public MoveModelTests()
	{
		_publisher = Substitute.For<IExternalMediaPublisher>();
		_forumService = Substitute.For<IForumService>();

		_model = new MoveModel(_db, _publisher, _forumService)
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
	public async Task OnGet_ExistingTopic_PopulatesTopicMove()
	{
		var user = _db.AddUserWithRole("TestUser").Entity;
		var topic = _db.AddTopic(user).Entity;
		await _db.SaveChangesAsync();

		_model.Id = topic.Id;

		var result = await _model.OnGet();

		Assert.IsInstanceOfType(result, typeof(PageResult));
		Assert.AreEqual(topic.Title, _model.Topic.Topic);
		Assert.AreEqual(topic.Forum!.Id, _model.Topic.NewForum);
		Assert.AreEqual(topic.Forum.Name, _model.Topic.CurrentForum);
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
	public async Task OnGet_RestrictedTopic_WithoutPermission_ReturnsNotFound()
	{
		var user = _db.AddUserWithRole("TestUser").Entity;
		var restrictedForum = _db.AddForum("Restricted Forum", true).Entity;
		var topic = _db.AddTopic(user).Entity;
		topic.Forum = restrictedForum;
		await _db.SaveChangesAsync();

		AddAuthenticatedUser(_model, user, [PermissionTo.MoveTopics]);
		_model.Id = topic.Id;

		var result = await _model.OnGet();

		Assert.IsInstanceOfType(result, typeof(NotFoundResult));
	}

	[TestMethod]
	public async Task OnPost_InvalidModelState_ReturnsPage()
	{
		_model.ModelState.AddModelError("NewForum", "Test error");

		var result = await _model.OnPost();

		Assert.IsInstanceOfType(result, typeof(PageResult));
	}

	[TestMethod]
	public async Task OnPost_NonExistentTopic_ReturnsNotFound()
	{
		_model.Id = 999;

		var result = await _model.OnPost();

		Assert.IsInstanceOfType(result, typeof(NotFoundResult));
	}

	[TestMethod]
	public async Task OnPost_NonExistentTargetForum_ReturnsNotFound()
	{
		var user = _db.AddUserWithRole("TestUser").Entity;
		var topic = _db.AddTopic(user).Entity;
		await _db.SaveChangesAsync();

		AddAuthenticatedUser(_model, user, [PermissionTo.MoveTopics]);
		_model.Id = topic.Id;
		_model.Topic = new MoveModel.TopicMove { NewForum = 999 };

		var result = await _model.OnPost();

		Assert.IsInstanceOfType(result, typeof(NotFoundResult));
	}

	[TestMethod]
	public async Task OnPost_ValidMove_UpdatesTopicAndPosts()
	{
		var user = _db.AddUserWithRole("TestUser").Entity;
		var sourceForum = _db.AddForum("Source Forum").Entity;
		var targetForum = _db.AddForum("Target Forum").Entity;
		var topic = _db.AddTopic(user).Entity;
		topic.Forum = sourceForum;
		var post = _db.CreatePostForTopic(topic, user).Entity;
		await _db.SaveChangesAsync();

		AddAuthenticatedUser(_model, user, [PermissionTo.MoveTopics]);
		_model.Id = topic.Id;
		_model.Topic = new MoveModel.TopicMove
		{
			NewForum = targetForum.Id,
			Topic = topic.Title,
			CurrentForum = sourceForum.Name
		};

		var result = await _model.OnPost();

		Assert.IsInstanceOfType(result, typeof(RedirectToPageResult));
		var redirect = (RedirectToPageResult)result;
		Assert.AreEqual("Index", redirect.PageName);

		await _db.Entry(topic).ReloadAsync();
		await _db.Entry(post).ReloadAsync();
		Assert.AreEqual(targetForum.Id, topic.ForumId);
		Assert.AreEqual(targetForum.Id, post.ForumId);
	}

	[TestMethod]
	public async Task OnPost_ValidMove_ClearsCacheAndSendsNotification()
	{
		var user = _db.AddUserWithRole("TestUser").Entity;
		var sourceForum = _db.AddForum("Source Forum").Entity;
		var targetForum = _db.AddForum("Target Forum").Entity;
		var topic = _db.AddTopic(user).Entity;
		topic.Forum = sourceForum;
		_db.CreatePostForTopic(topic, user);
		await _db.SaveChangesAsync();

		AddAuthenticatedUser(_model, user, [PermissionTo.MoveTopics]);
		_model.Id = topic.Id;
		_model.Topic = new MoveModel.TopicMove
		{
			NewForum = targetForum.Id,
			Topic = topic.Title,
			CurrentForum = sourceForum.Name
		};

		var result = await _model.OnPost();

		Assert.IsInstanceOfType(result, typeof(RedirectToPageResult));
		_forumService.Received(1).ClearLatestPostCache();
		_forumService.Received(1).ClearTopicActivityCache();
		await _publisher.Received(1).Send(Arg.Any<Post>());
	}

	[TestMethod]
	public async Task OnPost_MoveFromRestrictedForum_SendsRestrictedNotification()
	{
		var user = _db.AddUserWithRole("TestUser").Entity;
		var restrictedForum = _db.AddForum("Restricted Forum", true).Entity;
		var targetForum = _db.AddForum("Target Forum").Entity;
		var topic = _db.AddTopic(user).Entity;
		topic.Forum = restrictedForum;
		_db.CreatePostForTopic(topic, user);
		await _db.SaveChangesAsync();

		AddAuthenticatedUser(_model, user, [PermissionTo.MoveTopics, PermissionTo.SeeRestrictedForums]);
		_model.Id = topic.Id;
		_model.Topic = new MoveModel.TopicMove
		{
			NewForum = targetForum.Id,
			Topic = topic.Title,
			CurrentForum = restrictedForum.Name
		};

		var result = await _model.OnPost();

		Assert.IsInstanceOfType(result, typeof(RedirectToPageResult));
		await _publisher.Received(1).Send(Arg.Any<Post>());
	}

	[TestMethod]
	public async Task OnPost_MoveToRestrictedForum_SendsRestrictedNotification()
	{
		var user = _db.AddUserWithRole("TestUser").Entity;
		var sourceForum = _db.AddForum("Source Forum").Entity;
		var restrictedForum = _db.AddForum("Restricted Forum", true).Entity;
		var topic = _db.AddTopic(user).Entity;
		topic.Forum = sourceForum;
		_db.CreatePostForTopic(topic, user);
		await _db.SaveChangesAsync();

		AddAuthenticatedUser(_model, user, [PermissionTo.MoveTopics, PermissionTo.SeeRestrictedForums]);
		_model.Id = topic.Id;
		_model.Topic = new MoveModel.TopicMove
		{
			NewForum = restrictedForum.Id,
			Topic = topic.Title,
			CurrentForum = sourceForum.Name
		};

		var result = await _model.OnPost();

		Assert.IsInstanceOfType(result, typeof(RedirectToPageResult));
		await _publisher.Received(1).Send(Arg.Any<Post>());
	}

	[TestMethod]
	public async Task OnPost_MoveWithMultiplePosts_UpdatesAllPosts()
	{
		var user = _db.AddUserWithRole("TestUser").Entity;
		var user2 = _db.AddUserWithRole("SecondUser").Entity;
		var sourceForum = _db.AddForum("Source Forum").Entity;
		var targetForum = _db.AddForum("Target Forum").Entity;
		var topic = _db.AddTopic(user).Entity;
		topic.Forum = sourceForum;
		var post1 = _db.CreatePostForTopic(topic, user).Entity;
		var post2 = _db.CreatePostForTopic(topic, user2).Entity;
		await _db.SaveChangesAsync();

		AddAuthenticatedUser(_model, user, [PermissionTo.MoveTopics]);
		_model.Id = topic.Id;
		_model.Topic = new MoveModel.TopicMove
		{
			NewForum = targetForum.Id,
			Topic = topic.Title,
			CurrentForum = sourceForum.Name
		};

		var result = await _model.OnPost();

		Assert.IsInstanceOfType(result, typeof(RedirectToPageResult));

		await _db.Entry(post1).ReloadAsync();
		await _db.Entry(post2).ReloadAsync();
		Assert.AreEqual(targetForum.Id, post1.ForumId);
		Assert.AreEqual(targetForum.Id, post2.ForumId);
	}
}
