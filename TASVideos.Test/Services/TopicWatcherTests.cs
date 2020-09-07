using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Moq;

using TASVideos.Data.Entity;
using TASVideos.Data.Entity.Forum;
using TASVideos.Services;
using TASVideos.Services.Email;

// ReSharper disable InconsistentNaming
namespace TASVideos.Test.Services
{
	[TestClass]
	public class TopicWatcherTests
	{
		private Mock<IEmailService> _mockEmailService = null!;
		private Mock<IOptions<AppSettings>> _mockAppSettings = null!;
		private TestDbContext _db = null!;

		private ITopicWatcher _topicWatcher = null!;

		[TestInitialize]
		public void Initialize()
		{
			_db = TestDbContext.Create();
			_mockEmailService = new Mock<IEmailService>();
			_mockAppSettings = new Mock<IOptions<AppSettings>>();
			_mockAppSettings.Setup(m => m.Value).Returns(new AppSettings
			{
				BaseUrl = "http://example.com"
			});
			_topicWatcher = new TopicWatcher(_mockEmailService.Object, _db, _mockAppSettings.Object);
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
			int user1Id = 1;
			int user2Id = 2;
			int topic1Id = 1;
			int topic2Id = 2;
			_db.Users.Add(new User { Id = user1Id });
			_db.Users.Add(new User { Id = user2Id });
			var forum = new Forum { Id = 1 };
			_db.ForumTopics.Add(new ForumTopic { Id = topic1Id, ForumId = forum.Id, Forum = forum });
			_db.ForumTopics.Add(new ForumTopic { Id = topic2Id, ForumId = forum.Id, Forum = forum });
			_db.ForumTopicWatches.Add(new ForumTopicWatch { ForumTopicId = topic1Id, UserId = user1Id });
			_db.ForumTopicWatches.Add(new ForumTopicWatch { ForumTopicId = topic2Id, UserId = user1Id });
			_db.ForumTopicWatches.Add(new ForumTopicWatch { ForumTopicId = 1, UserId = user2Id });
			_db.ForumTopicWatches.Add(new ForumTopicWatch { ForumTopicId = 1 + 1, UserId = user2Id });
			_db.SaveChanges();

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
			_db.SaveChanges();
			_mockEmailService
				.Setup(m => m.TopicReplyNotification(It.IsAny<IEnumerable<string>>(), It.IsAny<TopicReplyNotificationTemplate>()));

			await _topicWatcher.NotifyNewPost(new TopicNotification
			{
				TopicId = 1,
				PosterId = 1
			});

			_mockEmailService.Verify(
				v => v.TopicReplyNotification(It.IsAny<IEnumerable<string>>(), It.IsAny<TopicReplyNotificationTemplate>()),
				Times.Never);
		}

		[TestMethod]
		public async Task NotifyNewPost_NotifiesOtherUsers()
		{
			int watcher = 1;
			int poster = 2;
			string posterEmail = "a@b.com";
			_db.Users.Add(new User { Id = watcher, Email = posterEmail });
			_db.ForumTopics.Add(new ForumTopic { Id = 1 });
			_db.ForumTopicWatches.Add(new ForumTopicWatch
			{
				ForumTopicId = 1,
				UserId = watcher,
				IsNotified = false
			});
			_db.SaveChanges();

			var recipients = new List<string> { posterEmail }.AsEnumerable();
			var template = new TopicReplyNotificationTemplate();
			_mockEmailService
				.Setup(m => m.TopicReplyNotification(recipients, template));

			await _topicWatcher.NotifyNewPost(new TopicNotification
			{
				TopicId = 1,
				PosterId = poster
			});

			_mockEmailService.Verify(
				v => v.TopicReplyNotification(It.IsAny<IEnumerable<string>>(), It.IsAny<TopicReplyNotificationTemplate>()),
				Times.Once);
		}

		[TestMethod]
		public async Task NotifyNewPost_WhenNotified_IsNotified_IsFalse()
		{
			int watcher = 1;
			int poster = 2;
			string posterEmail = "a@b.com";
			_db.Users.Add(new User { Id = watcher, Email = posterEmail });
			_db.ForumTopics.Add(new ForumTopic { Id = 1 });
			_db.ForumTopicWatches.Add(new ForumTopicWatch
			{
				ForumTopicId = 1,
				UserId = watcher,
				IsNotified = false
			});
			_db.SaveChanges();

			var recipients = new List<string> { posterEmail }.AsEnumerable();
			var template = new TopicReplyNotificationTemplate();
			_mockEmailService
				.Setup(m => m.TopicReplyNotification(recipients, template));

			await _topicWatcher.NotifyNewPost(new TopicNotification
			{
				TopicId = 1,
				PosterId = poster
			});

			Assert.IsTrue(_db.ForumTopicWatches.All(w => w.IsNotified));
		}

		[TestMethod]
		public async Task MarkSeen_IsNotifiedFalse()
		{
			int userId = 1;
			int topicId = 1;
			_db.Users.Add(new User { Id = userId });
			_db.ForumTopics.Add(new ForumTopic { Id = topicId });
			_db.ForumTopicWatches.Add(new ForumTopicWatch
			{
				UserId = userId,
				ForumTopicId = topicId,
				IsNotified = true
			});
			_db.SaveChanges();

			await _topicWatcher.MarkSeen(topicId, userId);

			Assert.AreEqual(1, _db.ForumTopicWatches.Count());
			Assert.IsFalse(_db.ForumTopicWatches.Single().IsNotified);
		}

		[TestMethod]
		public async Task WatchTopic_AddsWatch_IfNoneExist()
		{
			int userId = 1;
			int topicId = 1;
			_db.Users.Add(new User { Id = userId });
			_db.ForumTopics.Add(new ForumTopic { Id = topicId });
			_db.SaveChanges();

			await _topicWatcher.WatchTopic(topicId, userId, true);

			Assert.AreEqual(1, _db.ForumTopicWatches.Count());
			Assert.AreEqual(userId, _db.ForumTopicWatches.Single().UserId);
			Assert.AreEqual(topicId, _db.ForumTopicWatches.Single().ForumTopicId);
		}

		[TestMethod]
		public async Task WatchTopic_DoesNotAdd_IfAlreadyExists()
		{
			int userId = 1;
			int topicId = 1;
			_db.Users.Add(new User { Id = userId });
			_db.ForumTopics.Add(new ForumTopic { Id = topicId });
			_db.ForumTopicWatches.Add(new ForumTopicWatch { UserId = userId, ForumTopicId = topicId });
			_db.SaveChanges();

			await _topicWatcher.WatchTopic(topicId, userId, true);
			Assert.AreEqual(1, _db.ForumTopicWatches.Count());
			Assert.AreEqual(userId, _db.ForumTopicWatches.Single().UserId);
			Assert.AreEqual(topicId, _db.ForumTopicWatches.Single().ForumTopicId);
		}
		
		[TestMethod]
		public async Task WatchTopic_DoesNotAddRestricted_IfUserCanNotSeeRestricted()
		{
			int userId = 1;
			int topicId = 1;
			_db.Users.Add(new User { Id = userId });
			var forum = new Forum { Id = 1, Restricted = true };
			_db.Forums.Add(forum);
			_db.ForumTopics.Add(new ForumTopic { Id = topicId, ForumId = forum.Id });
			_db.SaveChanges();

			await _topicWatcher.WatchTopic(topicId, userId, false);

			Assert.AreEqual(0, _db.ForumTopicWatches.Count());
		}

		[TestMethod]
		public async Task UnwatchTopic_RemovesTopic()
		{
			int userId = 1;
			int topicId = 1;
			_db.Users.Add(new User { Id = userId });
			_db.ForumTopics.Add(new ForumTopic { Id = topicId });
			_db.ForumTopicWatches.Add(new ForumTopicWatch { UserId = userId, ForumTopicId = topicId });
			_db.SaveChanges();

			await _topicWatcher.UnwatchTopic(topicId, userId);

			Assert.AreEqual(0, _db.ForumTopicWatches.Count());
		}
	}
}
