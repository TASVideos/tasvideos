using Microsoft.EntityFrameworkCore;
using TASVideos.Data.Entity.Forum;

namespace TASVideos.Core.Tests.Services;

[TestClass]
public class ForumServiceTests : TestDbBase
{
	private readonly ForumService _forumService;
	private readonly TestCache _cache;
	private readonly ITopicWatcher _topicWatcher;

	public ForumServiceTests()
	{
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
		var topic = _db.AddTopic().Entity;
		topic.Forum!.Restricted = true;
		await _db.SaveChangesAsync();
		var entry = _db.ForumPosts.Add(new ForumPost { Topic = topic, Forum = topic.Forum, Poster = topic.Poster });
		await _db.SaveChangesAsync();

		var actual = await _forumService.GetPostPosition(entry.Entity.Id, false);
		Assert.IsNull(actual);
	}

	[TestMethod]
	public async Task GetPostPosition_ValidPost_ReturnsPosition()
	{
		var topic = _db.AddTopic().Entity;
		topic.Forum!.Restricted = false;
		await _db.SaveChangesAsync();
		var entry = _db.ForumPosts.Add(new ForumPost { Topic = topic, Forum = topic.Forum, Poster = topic.Poster });
		await _db.SaveChangesAsync();

		var actual = await _forumService.GetPostPosition(entry.Entity.Id, false);
		Assert.IsNotNull(actual);
		Assert.AreEqual(1, actual.Page);
		Assert.AreEqual(topic.Id, actual.TopicId);
	}

	[TestMethod]
	public async Task GetAllLatestPosts_SinglePost()
	{
		const int posterId = 1;
		const string posterName = "Test";
		const int postId = 3;
		var postDate = DateTime.UtcNow;
		_db.AddUser(posterId, posterName);
		var topic = _db.AddTopic().Entity;
		await _db.SaveChangesAsync();
		_db.ForumPosts.Add(new ForumPost
		{
			Id = postId,
			ForumId = topic.ForumId,
			TopicId = topic.Id,
			PosterId = posterId,
			CreateTimestamp = postDate
		});
		await _db.SaveChangesAsync();

		var actual = await _forumService.GetAllLatestPosts();

		Assert.IsNotNull(actual);
		Assert.AreEqual(1, actual.Count);
		Assert.IsTrue(actual.ContainsKey(topic.ForumId));
		var latestPost = actual[topic.ForumId];
		Assert.IsNotNull(latestPost);
		Assert.AreEqual(posterName, latestPost.PosterName);
		Assert.AreEqual(postDate.Ticks, latestPost.Timestamp.Ticks, TimeSpan.FromSeconds(1).Ticks);
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

		const int post1Id = 3;
		const int post2Id = 4;
		var post1Date = DateTime.UtcNow.AddDays(-2);
		var post2Date = DateTime.UtcNow.AddDays(-1);
		_db.AddUser(poster1Id, poster1Name);
		_db.AddUser(poster2Id, poster2Name);
		var topic = _db.AddTopic().Entity;
		await _db.SaveChangesAsync();
		_db.ForumPosts.Add(new ForumPost
		{
			Id = post1Id,
			ForumId = topic.ForumId,
			TopicId = topic.Id,
			PosterId = poster1Id,
			CreateTimestamp = post1Date
		});

		_db.ForumPosts.Add(new ForumPost
		{
			Id = post2Id,
			ForumId = topic.ForumId,
			TopicId = topic.Id,
			PosterId = poster2Id,
			CreateTimestamp = post2Date
		});

		await _db.SaveChangesAsync();

		var actual = await _forumService.GetAllLatestPosts();

		Assert.IsNotNull(actual);
		Assert.AreEqual(1, actual.Count);
		Assert.IsTrue(actual.ContainsKey(topic.ForumId));
		var latestPost = actual[topic.ForumId];
		Assert.IsNotNull(latestPost);
		Assert.AreEqual(poster2Name, latestPost.PosterName);
		Assert.AreEqual(post2Date.Ticks, latestPost.Timestamp.Ticks, TimeSpan.FromSeconds(1).Ticks);
		Assert.AreEqual(post2Id, latestPost.Id);
		Assert.AreEqual(1, _cache.Count());
	}

	// No posts returns null latest post
	[TestMethod]
	public async Task GetAllLatestPosts_NoPosts_ReturnsNull()
	{
		var topic = _db.AddTopic().Entity;
		await _db.SaveChangesAsync();

		var actual = await _forumService.GetAllLatestPosts();

		Assert.IsNotNull(actual);
		Assert.AreEqual(1, actual.Count);
		Assert.IsTrue(actual.ContainsKey(topic.ForumId));
		Assert.IsNull(actual[topic.ForumId]);
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
		var oldPostTime = DateTime.UtcNow.AddDays(-1);
		const string oldPoster = "OldPoster";
		var mapping = new Dictionary<int, LatestPost?>
		{
			[forumId] = new(oldPostId, oldPostTime, oldPoster)
		};
		_cache.Set(ForumService.LatestPostCacheKey, mapping);

		const int newPostId = 2;
		var newPostTime = DateTime.UtcNow;
		const string newPoster = "NewPoster";

		_forumService.CacheLatestPost(forumId, topicId, new LatestPost(newPostId, newPostTime, newPoster));

		Assert.IsTrue(_cache.ContainsKey(ForumService.LatestPostCacheKey));
		_cache.TryGetValue(ForumService.LatestPostCacheKey, out Dictionary<int, LatestPost?> actual);
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
		var topic = _db.AddTopic().Entity;
		await _db.SaveChangesAsync();

		const string question = "Is this a question?";
		const int daysOpen = 5;
		const bool multiSelect = false;
		const string option1 = "1";
		const string option2 = "2";
		const string option3 = "3";
		var options = new List<string> { option1, option2, option3 };
		var poll = new PollCreate(question, daysOpen, multiSelect, options);

		await _forumService.CreatePoll(topic, poll);

		Assert.AreEqual(1, _db.ForumTopics.Count(t => t.Id == topic.Id));
		var actualTopic = await _db.ForumTopics
			.Include(f => f.Poll)
			.ThenInclude(p => p!.PollOptions).SingleOrDefaultAsync(t => t.Id == topic.Id);
		Assert.IsNotNull(actualTopic);
		Assert.IsNotNull(actualTopic.Poll);
		Assert.AreEqual(question, actualTopic.Poll.Question);
		Assert.IsNotNull(actualTopic.Poll.CloseDate);
		Assert.AreEqual(DateTime.UtcNow.AddDays(daysOpen).Day, actualTopic.Poll.CloseDate.Value.Day);
		Assert.AreEqual(multiSelect, actualTopic.Poll.MultiSelect);
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
		var topic = _db.AddTopic().Entity;
		await _db.SaveChangesAsync();
		int forumId = topic.ForumId;
		int topicId = topic.Id;
		const string subject = "Test Subject";
		const string postText = "This is a post";
		var poster = _db.AddUser("Test User").Entity;
		await _db.SaveChangesAsync();
		int posterId = poster.Id;
		string posterName = poster.UserName;
		const ForumPostMood mood = ForumPostMood.Normal;
		const string ipAddress = "8.8.8.8";
		const bool watchTopic = false;
		var dto = new PostCreate(
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
		Assert.IsTrue(mapping.ContainsKey(forumId));
		var actualLatestPost = mapping[forumId];
		Assert.IsNotNull(actualLatestPost);
		Assert.AreEqual(actualLatestPost.Id, actual);
		Assert.AreEqual(actualLatestPost.PosterName, posterName);
	}

	[TestMethod]
	public async Task CreatePostWithWatch_CreatesPost()
	{
		_cache.Set(ForumService.LatestPostCacheKey, new Dictionary<int, LatestPost?>());
		var topic = _db.AddTopic().Entity;
		await _db.SaveChangesAsync();
		int forumId = topic.ForumId;
		int topicId = topic.Id;
		const string subject = "Test Subject";
		const string postText = "This is a post";
		var poster = _db.AddUser("Test User").Entity;
		await _db.SaveChangesAsync();
		int posterId = poster.Id;
		string posterName = poster.UserName;
		const ForumPostMood mood = ForumPostMood.Normal;
		const string ipAddress = "8.8.8.8";
		const bool watchTopic = true;

		int actual = await _forumService.CreatePost(new PostCreate(
			forumId,
			topicId,
			subject,
			postText,
			posterId,
			posterName,
			mood,
			ipAddress,
			watchTopic));

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
		Assert.IsTrue(mapping.ContainsKey(forumId));
		var actualLatestPost = mapping[forumId];
		Assert.IsNotNull(actualLatestPost);
		Assert.AreEqual(actualLatestPost.Id, actual);
		Assert.AreEqual(actualLatestPost.PosterName, posterName);
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
		var topic = _db.AddTopic().Entity;
		topic.IsLocked = false;
		await _db.SaveChangesAsync();

		var actual = await _forumService.IsTopicLocked(topic.Id);
		Assert.IsFalse(actual);
	}

	[TestMethod]
	public async Task IsTopicLocked_TopicExistsAndLocked_ReturnsTrue()
	{
		var topic = _db.AddTopic().Entity;
		topic.IsLocked = true;
		await _db.SaveChangesAsync();

		var actual = await _forumService.IsTopicLocked(topic.Id);
		Assert.IsTrue(actual);
	}

	[TestMethod]
	public async Task GetTopicCountInTopic_UserDoesNotExist_ReturnsZero()
	{
		var category = _db.ForumCategories.Add(new ForumCategory()).Entity;
		var forum = _db.Forums.Add(new Forum { Category = category }).Entity;
		await _db.SaveChangesAsync();

		var actual = await _forumService.GetTopicCountInForum(int.MaxValue, forum.Id);
		Assert.AreEqual(0, actual);
	}

	[TestMethod]
	public async Task GetTopicCountInTopic_TopicDoesNotExist_ReturnsZero()
	{
		var user = _db.AddUser(1);
		await _db.SaveChangesAsync();

		var actual = await _forumService.GetTopicCountInForum(user.Entity.Id, int.MaxValue);
		Assert.AreEqual(0, actual);
	}

	[TestMethod]
	public async Task GetTopicCountInTopic_ReturnsPostCountForTopic()
	{
		var user = _db.AddUser(1).Entity;
		var category = _db.ForumCategories.Add(new ForumCategory()).Entity;
		var targetForum = _db.Forums.Add(new Forum { Category = category }).Entity;
		var anotherForum = _db.Forums.Add(new Forum { Category = category }).Entity;
		_db.ForumPosts.Add(new ForumPost { Forum = targetForum, Topic = new ForumTopic { Forum = targetForum, Poster = user }, Poster = user });
		_db.ForumPosts.Add(new ForumPost { Forum = anotherForum, Topic = new ForumTopic { Forum = anotherForum, Poster = user }, Poster = user });

		await _db.SaveChangesAsync();

		var actual = await _forumService.GetTopicCountInForum(user.Id, targetForum.Id);
		Assert.AreEqual(1, actual);
	}

	[TestMethod]
	public async Task GetPostCountInTopic_UserDoesNotExist_ReturnsZero()
	{
		var topic = _db.AddTopic().Entity;
		await _db.SaveChangesAsync();

		var actual = await _forumService.GetPostCountInTopic(int.MaxValue, topic.Id);
		Assert.AreEqual(0, actual);
	}

	[TestMethod]
	public async Task GetPostCountInTopic_TopicDoesNotExist_ReturnsZero()
	{
		var user = _db.AddUser(1);
		await _db.SaveChangesAsync();

		var actual = await _forumService.GetPostCountInTopic(user.Entity.Id, int.MaxValue);
		Assert.AreEqual(0, actual);
	}

	[TestMethod]
	public async Task GetPostCountInTopic_ReturnsPostCountForTopic()
	{
		var user = _db.AddUser(1).Entity;
		var targetTopic = _db.AddTopic(user).Entity;
		var anotherTopic = _db.AddTopic(user).Entity;
		_db.ForumPosts.Add(new ForumPost { Forum = targetTopic.Forum, Topic = targetTopic, Poster = user });
		_db.ForumPosts.Add(new ForumPost { Forum = targetTopic.Forum, Topic = anotherTopic, Poster = user });

		await _db.SaveChangesAsync();

		var actual = await _forumService.GetPostCountInTopic(user.Id, targetTopic.Id);
		Assert.AreEqual(1, actual);
	}
}
