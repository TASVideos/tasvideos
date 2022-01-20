using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TASVideos.Core.Services;
using TASVideos.Data.Entity;
using TASVideos.Data.Entity.Forum;
using TASVideos.Tests.Base;

namespace TASVideos.Core.Tests.Services
{
	[TestClass]
	public class ForumServiceTests
	{
		private readonly ForumService _forumService;
		private readonly TestDbContext _db;
		private readonly TestCache _cache;
		private readonly Mock<ITopicWatcher> _topicWatcher;

		public ForumServiceTests()
		{
			_db = TestDbContext.Create();
			_cache = new TestCache();
			_topicWatcher = new Mock<ITopicWatcher>();
			_forumService = new ForumService(_db, _cache, _topicWatcher.Object);
		}

		[TestMethod]
		public async Task GetPostPosition_InvalidPostId_ReturnsNull()
		{
			var actual = await _forumService.GetPostPosition(int.MaxValue, true);
			Assert.IsNull(actual);
		}

		[TestMethod]
		public async Task GetPostPosition_UnableToSeeRestrictedPost_ReturnsNull()
		{
			int forumId = 1;
			int topicId = 1;
			var forum = new Forum { Id = forumId, Restricted = true };
			_db.Forums.Add(forum);
			_db.ForumTopics.Add(new ForumTopic { Id = topicId, ForumId = forumId });
			var entry = _db.ForumPosts.Add(new ForumPost { TopicId = topicId, ForumId = forumId });
			await _db.SaveChangesAsync();

			var actual = await _forumService.GetPostPosition(entry.Entity.Id, false);
			Assert.IsNull(actual);
		}

		[TestMethod]
		public async Task GetPostPosition_ValidPost_ReturnsPosition()
		{
			int forumId = 1;
			int topicId = 1;
			var forum = new Forum { Id = forumId, Restricted = false };
			_db.Forums.Add(forum);
			_db.ForumTopics.Add(new ForumTopic { Id = topicId, ForumId = forumId });
			var entry = _db.ForumPosts.Add(new ForumPost { TopicId = topicId, ForumId = forumId });
			await _db.SaveChangesAsync();

			var actual = await _forumService.GetPostPosition(entry.Entity.Id, false);
			Assert.IsNotNull(actual);
			Assert.AreEqual(1, actual.Page);
			Assert.AreEqual(topicId, actual.TopicId);
		}

		[TestMethod]
		public async Task GetAllLatestPosts_SinglePost()
		{
			int posterId = 1;
			string posterName = "Test";
			int forumId = 1;
			int topicId = 2;
			int postId = 3;
			DateTime postDate = DateTime.UtcNow;
			_db.Users.Add(new User { Id = posterId, UserName = posterName });
			_db.Forums.Add(new Forum { Id = forumId });
			_db.ForumTopics.Add(new ForumTopic { Id = topicId });
			_db.ForumPosts.Add(new ForumPost
			{
				Id = postId,
				ForumId = forumId,
				TopicId = 1,
				PosterId = posterId,
				CreateTimestamp = postDate
			});
			await _db.SaveChangesAsync();

			var actual = await _forumService.GetAllLatestPosts();

			Assert.IsNotNull(actual);
			Assert.AreEqual(1, actual.Count);
			Assert.IsTrue(actual.ContainsKey(forumId));
			var latestPost = actual[forumId];
			Assert.IsNotNull(latestPost);
			Assert.AreEqual(posterName, latestPost.PosterName);
			Assert.AreEqual(postDate, latestPost.Timestamp);
			Assert.AreEqual(postId, latestPost.Id);
			Assert.AreEqual(1, _cache.Count());
		}

		[TestMethod]
		public async Task GetAllLatestPosts_MultiplePost_GetsLatest()
		{
			int poster1Id = 1;
			string poster1Name = "Test";
			int poster2Id = 10;
			string poster2Name = "Test2";

			int forumId = 1;
			int topicId = 2;
			int post1Id = 3;
			int post2Id = 4;
			DateTime post1Date = DateTime.UtcNow.AddDays(-2);
			DateTime post2Date = DateTime.UtcNow.AddDays(-1);
			_db.Users.Add(new User { Id = poster1Id, UserName = poster1Name });
			_db.Users.Add(new User { Id = poster2Id, UserName = poster2Name });
			_db.Forums.Add(new Forum { Id = forumId });
			_db.ForumTopics.Add(new ForumTopic { Id = topicId });
			_db.ForumPosts.Add(new ForumPost
			{
				Id = post1Id,
				ForumId = forumId,
				TopicId = 1,
				PosterId = poster1Id,
				CreateTimestamp = post1Date
			});

			_db.ForumPosts.Add(new ForumPost
			{
				Id = post2Id,
				ForumId = forumId,
				TopicId = 1,
				PosterId = poster2Id,
				CreateTimestamp = post2Date
			});

			await _db.SaveChangesAsync();

			var actual = await _forumService.GetAllLatestPosts();

			Assert.IsNotNull(actual);
			Assert.AreEqual(1, actual.Count);
			Assert.IsTrue(actual.ContainsKey(forumId));
			var latestPost = actual[forumId];
			Assert.IsNotNull(latestPost);
			Assert.AreEqual(poster2Name, latestPost.PosterName);
			Assert.AreEqual(post2Date, latestPost.Timestamp);
			Assert.AreEqual(post2Id, latestPost.Id);
			Assert.AreEqual(1, _cache.Count());
		}

		// No posts returns null latest post
		[TestMethod]
		public async Task GetAllLatestPosts_NoPosts_ReturnsNull()
		{
			int posterId = 1;
			string posterName = "Test";
			int forumId = 1;
			_db.Users.Add(new User { Id = posterId, UserName = posterName });
			_db.Forums.Add(new Forum { Id = forumId });
			await _db.SaveChangesAsync();

			var actual = await _forumService.GetAllLatestPosts();

			Assert.IsNotNull(actual);
			Assert.AreEqual(1, actual.Count);
			Assert.IsTrue(actual.ContainsKey(forumId));
			Assert.IsNull(actual[forumId]);
			Assert.AreEqual(1, _cache.Count());
		}

		[TestMethod]
		public void CacheLatestPost_NoCache_DoesNotUpdate()
		{
			_forumService.CacheLatestPost(1, 1, new LatestPost(1, DateTime.UtcNow, "Test"));
			Assert.AreEqual(0, _cache.Count());
		}

		[TestMethod]
		public void CacheLatestPost_UpdatesCache()
		{
			int forumId = 1;
			int topicId = 1;
			int oldPostId = 1;
			DateTime oldPostTime = DateTime.UtcNow.AddDays(-1);
			string oldPoster = "OldPoster";
			var mapping = new Dictionary<int, LatestPost?>
			{
				[forumId] = new (oldPostId, oldPostTime, oldPoster)
			};
			_cache.Set(ForumService.LatestPostCacheKey, mapping);

			int newPostId = 2;
			DateTime newPostTime = DateTime.UtcNow;
			string newPoster = "NewPoster";

			_forumService.CacheLatestPost(forumId, topicId, new LatestPost(newPostId, newPostTime, newPoster));

			Assert.IsTrue(_cache.ContainsKey(ForumService.LatestPostCacheKey));
			_cache.TryGetValue(ForumService.LatestPostCacheKey, out Dictionary<int, LatestPost?> actual);
			Assert.IsNotNull(actual);
			Assert.IsTrue(actual.ContainsKey(forumId));
			var actualLatest = actual[forumId];
			Assert.IsNotNull(actualLatest);
			Assert.AreEqual(newPostId, actualLatest.Id);
			Assert.AreEqual(newPostTime, actualLatest.Timestamp);
			Assert.AreEqual(newPoster, actualLatest.PosterName);
		}

		[TestMethod]
		public void ClearCache()
		{
			_cache.Set(ForumService.LatestPostCacheKey, new Dictionary<int, LatestPost?>());

			_forumService.ClearCache();

			Assert.IsFalse(_cache.ContainsKey(ForumService.LatestPostCacheKey));
		}

		[TestMethod]
		public async Task CreatePoll_AddsPollToTopic()
		{
			int topicId = 2;
			var topic = new ForumTopic { Id = topicId };
			_db.ForumTopics.Add(topic);
			await _db.SaveChangesAsync();

			string question = "Is this a question?";
			int daysOpen = 5;
			bool multiSelect = false;
			var option1 = "1";
			var option2 = "2";
			var option3 = "3";
			var options = new List<string> { option1, option2, option3 };
			var poll = new PollCreateDto(question, daysOpen, multiSelect, options);

			await _forumService.CreatePoll(topic, poll);

			Assert.AreEqual(1, _db.ForumTopics.Count(t => t.Id == topicId));
			var actualTopic = await _db.ForumTopics.SingleOrDefaultAsync(t => t.Id == topic.Id);
			Assert.IsNotNull(actualTopic);
			Assert.IsNotNull(actualTopic.Poll);
			Assert.AreEqual(question, actualTopic.Poll.Question);
			Assert.IsNotNull(actualTopic.Poll.CloseDate);
			Assert.AreEqual(DateTime.UtcNow.AddDays(daysOpen).Day, actualTopic.Poll.CloseDate.Value.Day);
			Assert.AreEqual(multiSelect, actualTopic.Poll.MultiSelect);
			Assert.IsNotNull(actualTopic.Poll.PollOptions);
			var actualOptions = actualTopic.Poll.PollOptions;
			Assert.AreEqual(options.Count, actualOptions.Count);
			Assert.AreEqual(1, actualOptions.Count(o => o.Text == option1 && o.Ordinal == 0));
			Assert.AreEqual(1, actualOptions.Count(o => o.Text == option2 && o.Ordinal == 1));
			Assert.AreEqual(1, actualOptions.Count(o => o.Text == option3 && o.Ordinal == 2));
		}

		[TestMethod]
		public async Task CreatePostWithNoWatch_CreatesPost()
		{
			_cache.Set(ForumService.LatestPostCacheKey, new Dictionary<int, LatestPost?>());
			int forumId = 1;
			int topicId = 2;
			string subject = "Test Subject";
			string postText = "This is a post";
			int posterId = 3;
			string posterName = "Test User";
			var mood = ForumPostMood.Normal;
			string ipAddress = "8.8.8.8";
			bool watchTopic = false;
			var dto = new PostCreateDto(
				forumId,
				topicId,
				subject,
				postText,
				posterId,
				posterName,
				mood,
				ipAddress,
				watchTopic);

			int actual = await _forumService.CreatePost(dto);

			// Post must match
			Assert.AreEqual(1, _db.ForumPosts.Count(p => p.Id == actual));
			var actualPost = _db.ForumPosts.Single(p => p.Id == actual);
			Assert.AreEqual(forumId, actualPost.ForumId);
			Assert.AreEqual(topicId, actualPost.TopicId);
			Assert.AreEqual(subject, actualPost.Subject);
			Assert.AreEqual(postText, actualPost.Text);
			Assert.AreEqual(posterId, actualPost.PosterId);
			Assert.AreEqual(mood, actualPost.PosterMood);
			Assert.AreEqual(ipAddress, actualPost.IpAddress);

			// Must have no watches
			_topicWatcher.Verify(v => v.UnwatchTopic(It.IsAny<int>(), It.IsAny<int>()));

			// Cache must be updated
			Assert.IsTrue(_cache.ContainsKey(ForumService.LatestPostCacheKey));
			_cache.TryGetValue(ForumService.LatestPostCacheKey, out Dictionary<int, LatestPost?> mapping);
			Assert.IsNotNull(mapping);
			Assert.IsTrue(mapping.ContainsKey(forumId));
			var actualLatestPost = mapping[forumId];
			Assert.IsNotNull(actualLatestPost);
			Assert.AreEqual(actual, actualLatestPost.Id);
			Assert.AreEqual(posterName, actualLatestPost.PosterName);
		}

		[TestMethod]
		public async Task CreatePostWithWatch_CreatesPost()
		{
			_cache.Set(ForumService.LatestPostCacheKey, new Dictionary<int, LatestPost?>());
			int forumId = 1;
			int topicId = 2;
			string subject = "Test Subject";
			string postText = "This is a post";
			int posterId = 3;
			string posterName = "Test User";
			var mood = ForumPostMood.Normal;
			string ipAddress = "8.8.8.8";
			bool watchTopic = true;
			var dto = new PostCreateDto(
				forumId,
				topicId,
				subject,
				postText,
				posterId,
				posterName,
				mood,
				ipAddress,
				watchTopic);

			int actual = await _forumService.CreatePost(dto);

			// Post must match
			Assert.AreEqual(1, _db.ForumPosts.Count(p => p.Id == actual));
			var actualPost = _db.ForumPosts.Single(p => p.Id == actual);
			Assert.AreEqual(forumId, actualPost.ForumId);
			Assert.AreEqual(topicId, actualPost.TopicId);
			Assert.AreEqual(subject, actualPost.Subject);
			Assert.AreEqual(postText, actualPost.Text);
			Assert.AreEqual(posterId, actualPost.PosterId);
			Assert.AreEqual(mood, actualPost.PosterMood);
			Assert.AreEqual(ipAddress, actualPost.IpAddress);

			// Must add a watch
			_topicWatcher.Verify(v => v.WatchTopic(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()));

			// Cache must be updated
			Assert.IsTrue(_cache.ContainsKey(ForumService.LatestPostCacheKey));
			_cache.TryGetValue(ForumService.LatestPostCacheKey, out Dictionary<int, LatestPost?> mapping);
			Assert.IsNotNull(mapping);
			Assert.IsTrue(mapping.ContainsKey(forumId));
			var actualLatestPost = mapping[forumId];
			Assert.IsNotNull(actualLatestPost);
			Assert.AreEqual(actual, actualLatestPost.Id);
			Assert.AreEqual(posterName, actualLatestPost.PosterName);
		}
	}
}
