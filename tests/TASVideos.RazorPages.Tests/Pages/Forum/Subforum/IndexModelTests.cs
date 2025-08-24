using TASVideos.Core.Services;
using TASVideos.Data.Entity.Forum;
using TASVideos.Pages.Forum.Subforum;

namespace TASVideos.RazorPages.Tests.Pages.Forum.Subforum;

[TestClass]
public class IndexModelTests : BasePageModelTests
{
	private readonly IForumService _forumService;
	private readonly IndexModel _model;

	public IndexModelTests()
	{
		_forumService = Substitute.For<IForumService>();
		_model = new IndexModel(_db, _forumService)
		{
			PageContext = TestPageContext()
		};
	}

	[TestMethod]
	public async Task OnGet_NonExistentForum_ReturnsNotFound()
	{
		_model.Id = 999;

		var result = await _model.OnGet();

		Assert.IsInstanceOfType(result, typeof(NotFoundResult));
	}

	[TestMethod]
	public async Task OnGet_RestrictedForumWithPermission_AllowsAccess()
	{
		var user = _db.AddUserWithRole("AdminUser").Entity;
		var forum = _db.AddForum("Restricted Forum", true).Entity;
		await _db.SaveChangesAsync();

		AddAuthenticatedUser(_model, user, [PermissionTo.SeeRestrictedForums]);
		_forumService.GetPostActivityOfSubforum(Arg.Any<int>()).Returns(new Dictionary<int, (string, string)>());

		_model.Id = forum.Id;

		var result = await _model.OnGet();

		Assert.IsInstanceOfType(result, typeof(PageResult));
		Assert.AreEqual(forum.Id, _model.Forum.Id);
	}

	[TestMethod]
	public async Task OnGet_RestrictedForumWithoutPermission_ReturnsNotFound()
	{
		var user = _db.AddUserWithRole("RegularUser").Entity;
		var forum = _db.AddForum("Restricted Forum", true).Entity;
		await _db.SaveChangesAsync();

		AddAuthenticatedUser(_model, user, []);
		_forumService.GetPostActivityOfSubforum(Arg.Any<int>()).Returns(new Dictionary<int, (string, string)>());

		_model.Id = forum.Id;

		var result = await _model.OnGet();

		Assert.IsInstanceOfType(result, typeof(NotFoundResult));
	}

	[TestMethod]
	public async Task OnGet_ExistingForum_PopulatesCorrectly()
	{
		_forumService.GetPostActivityOfSubforum(Arg.Any<int>()).Returns(new Dictionary<int, (string, string)>());
		var user = _db.AddUserWithRole("TestUser").Entity;
		var user2 = _db.AddUserWithRole("User2").Entity;
		var forum = _db.AddForum("Test Forum").Entity;
		forum.CanCreateTopics = true;
		var topic = _db.AddTopic(user).Entity;
		topic.Forum = forum;
		topic.Title = "Test Topic";

		// Create initial post plus 3 replies
		_db.CreatePostForTopic(topic, user);
		_db.CreatePostForTopic(topic, user);
		_db.CreatePostForTopic(topic, user);

		var lastPost = _db.CreatePostForTopic(topic, user2).Entity;
		lastPost.CreateTimestamp = DateTime.UtcNow.AddHours(1);

		await _db.SaveChangesAsync();

		_model.Id = forum.Id;

		var result = await _model.OnGet();

		Assert.IsInstanceOfType(result, typeof(PageResult));
		Assert.AreEqual(forum.Id, _model.Forum.Id);
		Assert.AreEqual(forum.Name, _model.Forum.Name);
		Assert.IsTrue(_model.Forum.CanCreateTopics);
		Assert.IsTrue(_model.Topics.Any());

		var topicEntry = _model.Topics.First();
		Assert.AreEqual(topic.Id, topicEntry.Id);
		Assert.AreEqual(topic.Title, topicEntry.Topics);
		Assert.AreEqual(user.UserName, topicEntry.Author);
		Assert.AreEqual(4, topicEntry.Replies); // Total posts count

		// Verify last post set correctly
		Assert.IsNotNull(topicEntry.LastPost);
		Assert.AreEqual(lastPost.Id, topicEntry.LastPost.Id);
	}

	[TestMethod]
	public async Task OnGet_PaginationParameters_AreRespected()
	{
		var user = _db.AddUserWithRole("TestUser").Entity;
		var forum = _db.AddForum("Test Forum").Entity;

		// Create multiple topics
		for (int i = 0; i < 30; i++)
		{
			var topic = _db.AddTopic(user).Entity;
			topic.Forum = forum;
			topic.Title = $"Topic {i}";
			_db.CreatePostForTopic(topic, user);
		}

		await _db.SaveChangesAsync();
		_forumService.GetPostActivityOfSubforum(Arg.Any<int>()).Returns(new Dictionary<int, (string, string)>());

		_model.Id = forum.Id;
		_model.Search = new() { CurrentPage = 2, PageSize = 10 };

		var result = await _model.OnGet();

		Assert.IsInstanceOfType(result, typeof(PageResult));
		Assert.AreEqual(2, _model.Topics.Request.CurrentPage);
		Assert.AreEqual(30, _model.Topics.RowCount);
		Assert.AreEqual(10, _model.Topics.Count());
	}

	[TestMethod]
	public async Task OnGet_TopicTypes_AreOrderedCorrectly()
	{
		var user = _db.AddUserWithRole("TestUser").Entity;
		var forum = _db.AddForum("Test Forum").Entity;

		var normalTopic = _db.AddTopic(user).Entity;
		normalTopic.Forum = forum;
		normalTopic.Title = "Normal Topic";
		normalTopic.Type = ForumTopicType.Regular;
		_db.CreatePostForTopic(normalTopic, user);

		var stickyTopic = _db.AddTopic(user).Entity;
		stickyTopic.Forum = forum;
		stickyTopic.Title = "Sticky Topic";
		stickyTopic.Type = ForumTopicType.Sticky;
		_db.CreatePostForTopic(stickyTopic, user);

		await _db.SaveChangesAsync();
		_forumService.GetPostActivityOfSubforum(Arg.Any<int>()).Returns(new Dictionary<int, (string, string)>());

		_model.Id = forum.Id;

		await _model.OnGet();

		var topics = _model.Topics.ToList();
		Assert.AreEqual(2, topics.Count);

		// Sticky topics should come first (ordered by Type descending)
		Assert.AreEqual(ForumTopicType.Sticky, topics[0].Type);
		Assert.AreEqual(ForumTopicType.Regular, topics[1].Type);
	}

	[TestMethod]
	public async Task OnGet_EmptyForum_ReturnsEmptyTopicsList()
	{
		var forum = _db.AddForum("Empty Forum").Entity;
		await _db.SaveChangesAsync();
		_forumService.GetPostActivityOfSubforum(Arg.Any<int>()).Returns(new Dictionary<int, (string, string)>());

		_model.Id = forum.Id;

		var result = await _model.OnGet();

		Assert.IsInstanceOfType(result, typeof(PageResult));
		Assert.IsFalse(_model.Topics.Any());
		Assert.AreEqual(0, _model.Topics.RowCount);
	}
}
