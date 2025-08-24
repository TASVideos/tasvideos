using TASVideos.Core.Services;
using TASVideos.Core.Services.ExternalMediaPublisher;
using TASVideos.Pages.Forum.Topics;
using TASVideos.Services;

namespace TASVideos.RazorPages.Tests.Pages.Forum.Topics;

[TestClass]
public class MergeModelTests : BasePageModelTests
{
	private readonly IExternalMediaPublisher _publisher;
	private readonly IForumService _forumService;
	private readonly MergeModel _model;

	public MergeModelTests()
	{
		_publisher = Substitute.For<IExternalMediaPublisher>();
		_forumService = Substitute.For<IForumService>();

		_model = new MergeModel(_db, _publisher, _forumService)
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
	public async Task OnGet_ExistingTopic_PopulatesTopicMerge()
	{
		var user = _db.AddUserWithRole("TestUser").Entity;
		var topic = _db.AddTopic(user).Entity;
		topic.Title = "Test Topic";
		await _db.SaveChangesAsync();

		_model.Id = topic.Id;

		var result = await _model.OnGet();

		Assert.IsInstanceOfType(result, typeof(PageResult));
		Assert.AreEqual(topic.Title, _model.Topic.Title);
		Assert.AreEqual(topic.Forum!.Id, _model.Topic.ForumId);
		Assert.AreEqual(topic.Forum.Name, _model.Topic.ForumName);
		Assert.AreEqual(topic.Forum.Id, _model.Topic.DestinationForumId);
	}

	[TestMethod]
	public async Task OnGet_ExistingTopic_PopulatesAvailableForumsAndTopics()
	{
		var user = _db.AddUserWithRole("TestUser").Entity;
		var topic = _db.AddTopic(user).Entity;
		await _db.SaveChangesAsync();

		_model.Id = topic.Id;

		var result = await _model.OnGet();

		Assert.IsInstanceOfType(result, typeof(PageResult));
		Assert.IsTrue(_model.AvailableForums.Count > 0);
		Assert.IsTrue(_model.AvailableTopics.Count >= 0);
	}

	[TestMethod]
	public async Task OnGet_RestrictedTopic_WithoutPermission_ReturnsNotFound()
	{
		var user = _db.AddUserWithRole("TestUser").Entity;
		var restrictedForum = _db.AddForum("Restricted Forum", true).Entity;
		var topic = _db.AddTopic(user).Entity;
		topic.Forum = restrictedForum;
		await _db.SaveChangesAsync();

		AddAuthenticatedUser(_model, user, [PermissionTo.MergeTopics]);
		_model.Id = topic.Id;

		var result = await _model.OnGet();

		Assert.IsInstanceOfType(result, typeof(NotFoundResult));
	}

	[TestMethod]
	public async Task OnPost_MergeTopicIntoItself_AddsModelError()
	{
		var user = _db.AddUserWithRole("TestUser").Entity;
		var topic = _db.AddTopic(user).Entity;
		await _db.SaveChangesAsync();

		_model.Id = topic.Id;
		_model.Topic = new MergeModel.TopicMerge
		{
			DestinationTopicId = topic.Id
		};

		var result = await _model.OnPost();

		Assert.IsInstanceOfType(result, typeof(PageResult));
		Assert.IsTrue(_model.ModelState.ContainsKey($"{nameof(_model.Topic)}.{nameof(_model.Topic.DestinationTopicId)}"));
	}

	[TestMethod]
	public async Task OnPost_InvalidModelState_ReturnsPage()
	{
		_model.ModelState.AddModelError("TestError", "Test error message");

		var result = await _model.OnPost();

		Assert.IsInstanceOfType(result, typeof(PageResult));
	}

	[TestMethod]
	public async Task OnPost_NonExistentOriginalTopic_ReturnsNotFound()
	{
		var user = _db.AddUserWithRole("TestUser").Entity;
		var destinationTopic = _db.AddTopic(user).Entity;
		await _db.SaveChangesAsync();

		_model.Id = 999;
		_model.Topic = new MergeModel.TopicMerge
		{
			DestinationTopicId = destinationTopic.Id
		};

		var result = await _model.OnPost();

		Assert.IsInstanceOfType(result, typeof(NotFoundResult));
	}

	[TestMethod]
	public async Task OnPost_NonExistentDestinationTopic_ReturnsNotFound()
	{
		var user = _db.AddUserWithRole("TestUser").Entity;
		var originalTopic = _db.AddTopic(user).Entity;
		await _db.SaveChangesAsync();

		_model.Id = originalTopic.Id;
		_model.Topic = new MergeModel.TopicMerge
		{
			DestinationTopicId = 999
		};

		var result = await _model.OnPost();

		Assert.IsInstanceOfType(result, typeof(NotFoundResult));
	}

	[TestMethod]
	public async Task OnPost_ValidMerge_MovesPostsAndDeletesOriginalTopic()
	{
		var user = _db.AddUserWithRole("TestUser").Entity;
		var originalTopic = _db.AddTopic(user).Entity;
		originalTopic.Title = "Original Topic";
		var destinationTopic = _db.AddTopic(user).Entity;
		destinationTopic.Title = "Destination Topic";
		var post1 = _db.CreatePostForTopic(originalTopic, user).Entity;
		var post2 = _db.CreatePostForTopic(originalTopic, user).Entity;
		await _db.SaveChangesAsync();

		AddAuthenticatedUser(_model, user, [PermissionTo.MergeTopics]);
		_model.Id = originalTopic.Id;
		_model.Topic = new MergeModel.TopicMerge
		{
			DestinationTopicId = destinationTopic.Id,
			Title = originalTopic.Title,
			ForumId = originalTopic.ForumId,
			ForumName = originalTopic.Forum!.Name
		};

		var result = await _model.OnPost();

		AssertRedirect(result, "Index", destinationTopic.Id);

		await _db.Entry(post1).ReloadAsync();
		await _db.Entry(post2).ReloadAsync();
		Assert.AreEqual(destinationTopic.Id, post1.TopicId);
		Assert.AreEqual(destinationTopic.Id, post2.TopicId);
		Assert.AreEqual(destinationTopic.ForumId, post1.ForumId);
		Assert.AreEqual(destinationTopic.ForumId, post2.ForumId);

		var originalTopicExists = await _db.ForumTopics.AnyAsync(t => t.Id == originalTopic.Id);
		Assert.IsFalse(originalTopicExists);
	}

	[TestMethod]
	public async Task OnPost_ValidMerge_ClearsCacheAndSendsNotification()
	{
		var user = _db.AddUserWithRole("TestUser").Entity;
		var originalTopic = _db.AddTopic(user).Entity;
		originalTopic.Title = "Original Topic";
		var destinationTopic = _db.AddTopic(user).Entity;
		destinationTopic.Title = "Destination Topic";
		_db.CreatePostForTopic(originalTopic, user);
		await _db.SaveChangesAsync();

		AddAuthenticatedUser(_model, user, [PermissionTo.MergeTopics]);
		_model.Id = originalTopic.Id;
		_model.Topic = new MergeModel.TopicMerge
		{
			DestinationTopicId = destinationTopic.Id,
			Title = originalTopic.Title,
			ForumId = originalTopic.ForumId,
			ForumName = originalTopic.Forum!.Name
		};

		var result = await _model.OnPost();

		AssertRedirect(result, "Index");
		_forumService.Received(1).ClearLatestPostCache();
		_forumService.Received(1).ClearTopicActivityCache();
		await _publisher.Received(1).Send(Arg.Any<Post>());
	}

	[TestMethod]
	public async Task OnPost_MergeWithRestrictedForums_SendsRestrictedNotification()
	{
		var user = _db.AddUserWithRole("TestUser").Entity;
		var restrictedForum = _db.AddForum("Restricted Forum", true).Entity;
		var originalTopic = _db.AddTopic(user).Entity;
		originalTopic.Title = "Original Topic";
		originalTopic.Forum = restrictedForum;
		var destinationTopic = _db.AddTopic(user).Entity;
		destinationTopic.Title = "Destination Topic";
		_db.CreatePostForTopic(originalTopic, user);
		await _db.SaveChangesAsync();

		AddAuthenticatedUser(_model, user, [PermissionTo.MergeTopics, PermissionTo.SeeRestrictedForums]);
		_model.Id = originalTopic.Id;
		_model.Topic = new MergeModel.TopicMerge
		{
			DestinationTopicId = destinationTopic.Id,
			Title = originalTopic.Title,
			ForumId = originalTopic.ForumId,
			ForumName = originalTopic.Forum.Name
		};

		var result = await _model.OnPost();

		AssertRedirect(result, "Index");
		await _publisher.Received(1).Send(Arg.Any<Post>());
	}

	[TestMethod]
	public async Task OnGetTopicsForForum_ReturnsTopicsDropdown()
	{
		var topic = _db.AddTopic().Entity;
		await _db.SaveChangesAsync();

		_model.Id = topic.Id;

		var result = await _model.OnGetTopicsForForum(topic.ForumId);

		Assert.IsInstanceOfType(result, typeof(PartialViewResult));
		var partialResult = (PartialViewResult)result;
		Assert.IsNotNull(partialResult.Model);
	}

	[TestMethod]
	public async Task OnPost_MergeAcrossForums_UpdatesPostForumIds()
	{
		var user = _db.AddUserWithRole("TestUser").Entity;
		var sourceForum = _db.AddForum("Source Forum").Entity;
		var targetForum = _db.AddForum("Target Forum").Entity;

		var originalTopic = _db.AddTopic(user).Entity;
		originalTopic.Title = "Original Topic";
		originalTopic.Forum = sourceForum;

		var destinationTopic = _db.AddTopic(user).Entity;
		destinationTopic.Title = "Destination Topic";
		destinationTopic.Forum = targetForum;

		var post = _db.CreatePostForTopic(originalTopic, user).Entity;
		await _db.SaveChangesAsync();

		AddAuthenticatedUser(_model, user, [PermissionTo.MergeTopics]);
		_model.Id = originalTopic.Id;
		_model.Topic = new MergeModel.TopicMerge
		{
			DestinationTopicId = destinationTopic.Id,
			Title = originalTopic.Title,
			ForumId = originalTopic.ForumId,
			ForumName = originalTopic.Forum.Name
		};

		var result = await _model.OnPost();

		AssertRedirect(result, "Index", destinationTopic.Id);

		await _db.Entry(post).ReloadAsync();
		Assert.AreEqual(destinationTopic.Id, post.TopicId);
		Assert.AreEqual(targetForum.Id, post.ForumId);
	}
}
