using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
		private Mock<IEmailService> _mockEmailService;
		private TestDbContext _db;

		private ITopicWatcher _topicWatcher;

		[TestInitialize]
		public void Initialize()
		{
			_db = TestDbContext.Create();
			_mockEmailService = new Mock<IEmailService>();
			_topicWatcher = new TopicWatcher(_mockEmailService.Object, _db);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public async Task NotifyNewPost_ThrowArgumentNull()
		{
			await _topicWatcher.NotifyNewPost(null);
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

		// Unwatch Topic - removes if already exists
	}
}
