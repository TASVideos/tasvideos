using Microsoft.EntityFrameworkCore;
using TASVideos.Data.Entity;
using TASVideos.Data.Entity.Forum;

namespace TASVideos.Core.Tests.Services;

[TestClass]
public class ForumServiceTests
{
	private readonly ForumService _forumService;
	private readonly TestDbContext _db;
	private readonly TestCache _cache;
	private readonly ITopicWatcher _topicWatcher;

	public ForumServiceTests()
	{
		_db = TestDbContext.Create();
		_cache = new TestCache();
		_topicWatcher = Substitute.For<ITopicWatcher>();
		_forumService = new ForumService(_db, _cache, _topicWatcher);
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
		const int forumId = 1;
		const int topicId = 1;
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
		const int forumId = 1;
		const int topicId = 1;
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
		const int posterId = 1;
		const string posterName = "Test";
		const int forumId = 1;
		const int topicId = 2;
		const int postId = 3;
		DateTime postDate = DateTime.UtcNow;
		_db.AddUser(posterId, posterName);
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
		const int poster1Id = 1;
		const string poster1Name = "Test";
		const int poster2Id = 10;
		const string poster2Name = "Test2";

		const int forumId = 1;
		const int topicId = 2;
		const int post1Id = 3;
		const int post2Id = 4;
		DateTime post1Date = DateTime.UtcNow.AddDays(-2);
		DateTime post2Date = DateTime.UtcNow.AddDays(-1);
		_db.AddUser(poster1Id, poster1Name);
		_db.AddUser(poster2Id, poster2Name);
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
		const int posterId = 1;
		const string posterName = "Test";
		const int forumId = 1;
		_db.AddUser(posterId, posterName);
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
		const int forumId = 1;
		const int topicId = 1;
		const int oldPostId = 1;
		DateTime oldPostTime = DateTime.UtcNow.AddDays(-1);
		const string oldPoster = "OldPoster";
		var mapping = new Dictionary<int, LatestPost?>
		{
			[forumId] = new(oldPostId, oldPostTime, oldPoster)
		};
		_cache.Set(ForumService.LatestPostCacheKey, mapping);

		const int newPostId = 2;
		DateTime newPostTime = DateTime.UtcNow;
		const string newPoster = "NewPoster";

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

		_forumService.ClearLatestPostCache();

		Assert.IsFalse(_cache.ContainsKey(ForumService.LatestPostCacheKey));
	}

	[TestMethod]
	public async Task CreatePoll_AddsPollToTopic()
	{
		const int topicId = 2;
		var topic = new ForumTopic { Id = topicId };
		_db.ForumTopics.Add(topic);
		await _db.SaveChangesAsync();

		const string question = "Is this a question?";
		const int daysOpen = 5;
		const bool multiSelect = false;
		const string option1 = "1";
		const string option2 = "2";
		const string option3 = "3";
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
		const int forumId = 1;
		const int topicId = 2;
		const string subject = "Test Subject";
		const string postText = "This is a post";
		const int posterId = 3;
		const string posterName = "Test User";
		const ForumPostMood mood = ForumPostMood.Normal;
		const string ipAddress = "8.8.8.8";
		const bool watchTopic = false;
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
		await _topicWatcher.Received(1).UnwatchTopic(Arg.Any<int>(), Arg.Any<int>());

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
		const int forumId = 1;
		const int topicId = 2;
		const string subject = "Test Subject";
		const string postText = "This is a post";
		const int posterId = 3;
		const string posterName = "Test User";
		const ForumPostMood mood = ForumPostMood.Normal;
		const string ipAddress = "8.8.8.8";
		const bool watchTopic = true;
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
		await _topicWatcher.Received(1).WatchTopic(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<bool>());

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
	public async Task IsTopicLocked_TopicDoesNotExist_ReturnsFalse()
	{
		var actual = await _forumService.IsTopicLocked(int.MaxValue);
		Assert.IsFalse(actual);
	}

	[TestMethod]
	public async Task IsTopicLocked_TopicExistsAndNotLocked_ReturnsFalse()
	{
		const int topicId = 1;
		_db.ForumTopics.Add(new ForumTopic { Id = topicId, IsLocked = false });
		await _db.SaveChangesAsync();

		var actual = await _forumService.IsTopicLocked(topicId);
		Assert.IsFalse(actual);
	}

	[TestMethod]
	public async Task IsTopicLocked_TopicExistsAndLocked_ReturnsTrue()
	{
		const int topicId = 1;
		_db.ForumTopics.Add(new ForumTopic { Id = topicId, IsLocked = true });
		await _db.SaveChangesAsync();

		var actual = await _forumService.IsTopicLocked(topicId);
		Assert.IsTrue(actual);
	}
}
