using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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

		public ForumServiceTests()
		{
			_db = TestDbContext.Create();
			_cache = new TestCache();
			_forumService = new ForumService(_db, _cache);
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
	}
}
