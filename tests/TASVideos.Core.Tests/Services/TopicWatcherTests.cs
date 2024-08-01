using Microsoft.Extensions.Logging.Abstractions;
using TASVideos.Core.Services.Email;
using TASVideos.Core.Settings;
using TASVideos.Data.Entity.Forum;

namespace TASVideos.Core.Tests.Services;

[TestClass]
public class TopicWatcherTests : TestDbBase
{
	private readonly IEmailService _mockEmailService;

	private readonly TopicWatcher _topicWatcher;

	public TopicWatcherTests()
	{
		_mockEmailService = Substitute.For<IEmailService>();
		var settings = new AppSettings
		{
			BaseUrl = "http://example.com"
		};

		_topicWatcher = new TopicWatcher(_mockEmailService, _db, settings, new NullLogger<TopicWatcher>());
	}

	[TestMethod]
	public async Task UserWatches_EmptyList_WhenUserDoesNotExist()
	{
		var actual = await _topicWatcher.UserWatches(int.MaxValue, new PagingModel());

		Assert.IsNotNull(actual);
		Assert.AreEqual(0, actual.Count());
	}

	[TestMethod]
	public async Task UserWatches_ReturnsUserWatches()
	{
		const int user1Id = 1;
		const int user2Id = 2;
		_db.AddUser(user1Id, "_");
		_db.AddUser(user2Id, "__");
		var topic1 = _db.AddTopic().Entity;
		var topic2 = _db.AddTopic().Entity;
		await _db.SaveChangesAsync();
		_db.ForumPosts.Add(new ForumPost { Topic = topic1, Forum = topic1.Forum, PosterId = user1Id });
		_db.ForumPosts.Add(new ForumPost { Topic = topic2, Forum = topic2.Forum, PosterId = user2Id });
		_db.ForumTopicWatches.Add(new ForumTopicWatch { ForumTopic = topic1, UserId = user1Id });
		_db.ForumTopicWatches.Add(new ForumTopicWatch { ForumTopic = topic2, UserId = user1Id });
		_db.ForumTopicWatches.Add(new ForumTopicWatch { ForumTopic = topic1, UserId = user2Id });
		_db.ForumTopicWatches.Add(new ForumTopicWatch { ForumTopic = topic2, UserId = user2Id });
		await _db.SaveChangesAsync();

		var actual = await _topicWatcher.UserWatches(user1Id, new PagingModel());

		Assert.IsNotNull(actual);

		var list = actual.ToList();
		Assert.AreEqual(2, list.Count);
		Assert.AreEqual(1, list.Count(l => l.TopicId == topic1.Id));
		Assert.AreEqual(1, list.Count(l => l.TopicId == topic2.Id));
	}

	[TestMethod]
	public async Task NotifyNewPost_DoesNotNotifyPoster()
	{
		const int userId = 1;
		_db.AddUser(userId, "_");
		var topic = _db.AddTopic().Entity;
		await _db.SaveChangesAsync();
		_db.ForumTopicWatches.Add(new ForumTopicWatch
		{
			ForumTopicId = topic.Id,
			UserId = userId,
			IsNotified = false
		});
		await _db.SaveChangesAsync();

		await _topicWatcher.NotifyNewPost(0, topic.Id, "", userId);

		await _mockEmailService.DidNotReceive().TopicReplyNotification(Arg.Any<IEnumerable<string>>(), Arg.Any<TopicReplyNotificationTemplate>());
	}

	[TestMethod]
	public async Task NotifyNewPost_NotifiesOtherUsers()
	{
		const int watcher = 1;
		const int poster = 2;
		const string posterEmail = "a@b.com";
		_db.AddUser(watcher, posterEmail);
		var topic = _db.AddTopic().Entity;
		await _db.SaveChangesAsync();
		_db.ForumTopicWatches.Add(new ForumTopicWatch
		{
			ForumTopicId = topic.Id,
			UserId = watcher,
			IsNotified = false
		});
		await _db.SaveChangesAsync();

		await _topicWatcher.NotifyNewPost(0, topic.Id, "", poster);

		await _mockEmailService.Received(1).TopicReplyNotification(Arg.Any<IEnumerable<string>>(), Arg.Any<TopicReplyNotificationTemplate>());
	}

	[TestMethod]
	public async Task NotifyNewPost_WhenNotified_IsNotified_IsFalse()
	{
		const int watcher = 1;
		const int poster = 2;
		const string posterEmail = "a@b.com";
		_db.AddUser(watcher, posterEmail);
		var topic = _db.AddTopic().Entity;
		await _db.SaveChangesAsync();
		_db.ForumTopicWatches.Add(new ForumTopicWatch
		{
			ForumTopicId = topic.Id,
			UserId = watcher,
			IsNotified = false
		});
		await _db.SaveChangesAsync();

		await _topicWatcher.NotifyNewPost(0, topic.Id, "", poster);

		Assert.IsTrue(_db.ForumTopicWatches.All(w => w.IsNotified));
	}

	[TestMethod]
	public async Task MarkSeen_IsNotifiedFalse()
	{
		const int userId = 1;
		_db.AddUser(userId, "_");
		var topic = _db.AddTopic().Entity;
		_db.ForumTopicWatches.Add(new ForumTopicWatch
		{
			UserId = userId,
			ForumTopic = topic,
			IsNotified = true
		});
		await _db.SaveChangesAsync();

		await _topicWatcher.MarkSeen(topic.Id, userId);
		_db.ChangeTracker.Clear();

		Assert.AreEqual(1, _db.ForumTopicWatches.Count());
		Assert.IsFalse(_db.ForumTopicWatches.Single().IsNotified);
	}

	[TestMethod]
	public async Task WatchTopic_AddsWatch_IfNoneExist()
	{
		const int userId = 1;
		_db.AddUser(userId, "_");
		var topic = _db.AddTopic().Entity;
		await _db.SaveChangesAsync();

		await _topicWatcher.WatchTopic(topic.Id, userId, true);

		Assert.AreEqual(1, _db.ForumTopicWatches.Count());
		Assert.AreEqual(userId, _db.ForumTopicWatches.Single().UserId);
		Assert.AreEqual(topic.Id, _db.ForumTopicWatches.Single().ForumTopicId);
	}

	[TestMethod]
	public async Task WatchTopic_DoesNotAdd_IfAlreadyExists()
	{
		const int userId = 1;
		_db.AddUser(userId, "_");
		var topic = _db.AddTopic().Entity;
		await _db.SaveChangesAsync();
		_db.ForumTopicWatches.Add(new ForumTopicWatch { UserId = userId, ForumTopicId = topic.Id });
		await _db.SaveChangesAsync();

		await _topicWatcher.WatchTopic(topic.Id, userId, true);
		Assert.AreEqual(1, _db.ForumTopicWatches.Count());
		Assert.AreEqual(userId, _db.ForumTopicWatches.Single().UserId);
		Assert.AreEqual(topic.Id, _db.ForumTopicWatches.Single().ForumTopicId);
	}

	[TestMethod]
	public async Task WatchTopic_DoesNotAddRestricted_IfUserCanNotSeeRestricted()
	{
		const int userId = 1;
		const int topicId = 1;
		_db.AddUser(userId, "_");
		var topic = _db.AddTopic().Entity;
		topic.Forum!.Restricted = true;
		await _db.SaveChangesAsync();

		await _topicWatcher.WatchTopic(topicId, userId, false);

		Assert.AreEqual(0, _db.ForumTopicWatches.Count());
	}

	[TestMethod]
	public async Task UnwatchTopic_RemovesTopic()
	{
		const int userId = 1;
		_db.AddUser(userId, "_");
		var topic = _db.AddTopic().Entity;
		await _db.SaveChangesAsync();
		_db.ForumTopicWatches.Add(new ForumTopicWatch { UserId = userId, ForumTopicId = topic.Id });
		await _db.SaveChangesAsync();

		await _topicWatcher.UnwatchTopic(topic.Id, userId);

		Assert.AreEqual(0, _db.ForumTopicWatches.Count());
	}

	[TestMethod]
	public async Task UnwatchAllTopics_RemovesAllTopics()
	{
		const int userId = 1;
		_db.AddUser(userId, "_");
		var topic1 = _db.AddTopic().Entity;
		var topic2 = _db.AddTopic().Entity;
		await _db.SaveChangesAsync();
		_db.ForumTopicWatches.Add(new ForumTopicWatch { UserId = userId, ForumTopicId = topic1.Id });
		_db.ForumTopicWatches.Add(new ForumTopicWatch { UserId = userId, ForumTopicId = topic2.Id });
		await _db.SaveChangesAsync();

		await _topicWatcher.UnwatchAllTopics(userId);

		Assert.AreEqual(0, _db.ForumTopicWatches.Count());
	}
}
