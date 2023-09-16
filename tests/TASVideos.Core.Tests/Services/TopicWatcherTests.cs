using TASVideos.Core.Services.Email;
using TASVideos.Core.Settings;
using TASVideos.Data.Entity;
using TASVideos.Data.Entity.Forum;

namespace TASVideos.Core.Tests.Services;

[TestClass]
public class TopicWatcherTests
{
	private readonly IEmailService _mockEmailService;
	private readonly TestDbContext _db;

	private readonly ITopicWatcher _topicWatcher;

	public TopicWatcherTests()
	{
		_db = TestDbContext.Create();
		_mockEmailService = Substitute.For<IEmailService>();
		var settings = new AppSettings
		{
			BaseUrl = "http://example.com"
		};

		_topicWatcher = new TopicWatcher(_mockEmailService, _db, settings);
	}

	[TestMethod]
	public async Task UserWatches_EmptyList_WhenUserDoesNotExist()
	{
		var actual = await _topicWatcher.UserWatches(int.MaxValue);

		Assert.IsNotNull(actual);
		Assert.AreEqual(0, actual.Count());
	}

	[TestMethod]
	public async Task UserWatches_ReturnsUserWatches()
	{
		const int user1Id = 1;
		const int user2Id = 2;
		const int topic1Id = 1;
		const int topic2Id = 2;
		_db.Users.Add(new User { Id = user1Id });
		_db.Users.Add(new User { Id = user2Id });
		var forum = new Forum { Id = 1 };
		_db.ForumTopics.Add(new ForumTopic { Id = topic1Id, ForumId = forum.Id, Forum = forum });
		_db.ForumTopics.Add(new ForumTopic { Id = topic2Id, ForumId = forum.Id, Forum = forum });
		_db.ForumPosts.Add(new ForumPost { TopicId = topic1Id });
		_db.ForumPosts.Add(new ForumPost { TopicId = topic2Id });
		_db.ForumTopicWatches.Add(new ForumTopicWatch { ForumTopicId = topic1Id, UserId = user1Id });
		_db.ForumTopicWatches.Add(new ForumTopicWatch { ForumTopicId = topic2Id, UserId = user1Id });
		_db.ForumTopicWatches.Add(new ForumTopicWatch { ForumTopicId = 1, UserId = user2Id });
		_db.ForumTopicWatches.Add(new ForumTopicWatch { ForumTopicId = 1 + 1, UserId = user2Id });
		await _db.SaveChangesAsync();

		var actual = await _topicWatcher.UserWatches(user1Id);

		Assert.IsNotNull(actual);

		var list = actual.ToList();
		Assert.AreEqual(2, list.Count);
		Assert.AreEqual(1, list.Count(l => l.TopicId == topic1Id));
		Assert.AreEqual(1, list.Count(l => l.TopicId == topic2Id));
	}

	[TestMethod]
	public async Task NotifyNewPost_DoesNotNotifyPoster()
	{
		_db.Users.Add(new User { Id = 1 });
		_db.ForumTopics.Add(new ForumTopic { Id = 1 });
		_db.ForumTopicWatches.Add(new ForumTopicWatch
		{
			ForumTopicId = 1,
			UserId = 1,
			IsNotified = false
		});
		await _db.SaveChangesAsync();

		await _topicWatcher.NotifyNewPost(new TopicNotification(0, 1, "", 1));

		await _mockEmailService.DidNotReceive().TopicReplyNotification(Arg.Any<IEnumerable<string>>(), Arg.Any<TopicReplyNotificationTemplate>());
	}

	[TestMethod]
	public async Task NotifyNewPost_NotifiesOtherUsers()
	{
		const int watcher = 1;
		const int poster = 2;
		const string posterEmail = "a@b.com";
		_db.Users.Add(new User { Id = watcher, Email = posterEmail });
		_db.ForumTopics.Add(new ForumTopic { Id = 1 });
		_db.ForumTopicWatches.Add(new ForumTopicWatch
		{
			ForumTopicId = 1,
			UserId = watcher,
			IsNotified = false
		});
		await _db.SaveChangesAsync();

		await _topicWatcher.NotifyNewPost(new TopicNotification(0, 1, "", poster));

		await _mockEmailService.Received(1).TopicReplyNotification(Arg.Any<IEnumerable<string>>(), Arg.Any<TopicReplyNotificationTemplate>());
	}

	[TestMethod]
	public async Task NotifyNewPost_WhenNotified_IsNotified_IsFalse()
	{
		const int watcher = 1;
		const int poster = 2;
		const string posterEmail = "a@b.com";
		_db.Users.Add(new User { Id = watcher, Email = posterEmail });
		_db.ForumTopics.Add(new ForumTopic { Id = 1 });
		_db.ForumTopicWatches.Add(new ForumTopicWatch
		{
			ForumTopicId = 1,
			UserId = watcher,
			IsNotified = false
		});
		await _db.SaveChangesAsync();

		await _topicWatcher.NotifyNewPost(new TopicNotification(0, 1, "", poster));

		Assert.IsTrue(_db.ForumTopicWatches.All(w => w.IsNotified));
	}

	[TestMethod]
	public async Task MarkSeen_IsNotifiedFalse()
	{
		const int userId = 1;
		const int topicId = 1;
		_db.Users.Add(new User { Id = userId });
		_db.ForumTopics.Add(new ForumTopic { Id = topicId });
		_db.ForumTopicWatches.Add(new ForumTopicWatch
		{
			UserId = userId,
			ForumTopicId = topicId,
			IsNotified = true
		});
		await _db.SaveChangesAsync();

		await _topicWatcher.MarkSeen(topicId, userId);

		Assert.AreEqual(1, _db.ForumTopicWatches.Count());
		Assert.IsFalse(_db.ForumTopicWatches.Single().IsNotified);
	}

	[TestMethod]
	public async Task WatchTopic_AddsWatch_IfNoneExist()
	{
		const int userId = 1;
		const int topicId = 1;
		_db.Users.Add(new User { Id = userId });
		_db.ForumTopics.Add(new ForumTopic { Id = topicId });
		await _db.SaveChangesAsync();

		await _topicWatcher.WatchTopic(topicId, userId, true);

		Assert.AreEqual(1, _db.ForumTopicWatches.Count());
		Assert.AreEqual(userId, _db.ForumTopicWatches.Single().UserId);
		Assert.AreEqual(topicId, _db.ForumTopicWatches.Single().ForumTopicId);
	}

	[TestMethod]
	public async Task WatchTopic_DoesNotAdd_IfAlreadyExists()
	{
		const int userId = 1;
		const int topicId = 1;
		_db.Users.Add(new User { Id = userId });
		_db.ForumTopics.Add(new ForumTopic { Id = topicId });
		_db.ForumTopicWatches.Add(new ForumTopicWatch { UserId = userId, ForumTopicId = topicId });
		await _db.SaveChangesAsync();

		await _topicWatcher.WatchTopic(topicId, userId, true);
		Assert.AreEqual(1, _db.ForumTopicWatches.Count());
		Assert.AreEqual(userId, _db.ForumTopicWatches.Single().UserId);
		Assert.AreEqual(topicId, _db.ForumTopicWatches.Single().ForumTopicId);
	}

	[TestMethod]
	public async Task WatchTopic_DoesNotAddRestricted_IfUserCanNotSeeRestricted()
	{
		const int userId = 1;
		const int topicId = 1;
		_db.Users.Add(new User { Id = userId });
		var forum = new Forum { Id = 1, Restricted = true };
		_db.Forums.Add(forum);
		_db.ForumTopics.Add(new ForumTopic { Id = topicId, ForumId = forum.Id });
		await _db.SaveChangesAsync();

		await _topicWatcher.WatchTopic(topicId, userId, false);

		Assert.AreEqual(0, _db.ForumTopicWatches.Count());
	}

	[TestMethod]
	public async Task UnwatchTopic_RemovesTopic()
	{
		const int userId = 1;
		const int topicId = 1;
		_db.Users.Add(new User { Id = userId });
		_db.ForumTopics.Add(new ForumTopic { Id = topicId });
		_db.ForumTopicWatches.Add(new ForumTopicWatch { UserId = userId, ForumTopicId = topicId });
		await _db.SaveChangesAsync();

		await _topicWatcher.UnwatchTopic(topicId, userId);

		Assert.AreEqual(0, _db.ForumTopicWatches.Count());
	}

	[TestMethod]
	public async Task UnwatchAllTopics_RemovesAllTopics()
	{
		const int userId = 1;
		const int topic1Id = 1;
		const int topic2Id = 2;
		_db.Users.Add(new User { Id = userId });
		_db.ForumTopics.Add(new ForumTopic { Id = topic1Id });
		_db.ForumTopics.Add(new ForumTopic { Id = topic2Id });
		_db.ForumTopicWatches.Add(new ForumTopicWatch { UserId = userId, ForumTopicId = topic1Id });
		_db.ForumTopicWatches.Add(new ForumTopicWatch { UserId = userId, ForumTopicId = topic2Id });
		await _db.SaveChangesAsync();

		await _topicWatcher.UnwatchAllTopics(userId);

		Assert.AreEqual(0, _db.ForumTopicWatches.Count());
	}
}
