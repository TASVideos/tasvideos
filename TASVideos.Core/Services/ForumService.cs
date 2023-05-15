using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Data.Entity.Forum;

namespace TASVideos.Core.Services;

public interface IForumService
{
	Task<PostPositionDto?> GetPostPosition(int postId, bool seeRestricted);
	Task<IReadOnlyCollection<ForumCategoryDisplayDto>> GetAllCategories();
	void CacheLatestPost(int forumId, int topicId, LatestPost post);
	void CacheNewPostActivity(int forumId, int topicId, int postId, DateTime createTimestamp);
	void CacheEditedPostActivity(int forumId, int topicId, int postId, DateTime createTimestamp);
	void ClearLatestPostCache();
	void ClearTopicActivityCache();
	Task CreatePoll(ForumTopic topic, PollCreateDto poll);
	Task<int> CreatePost(PostCreateDto post);
	Task<bool> IsTopicLocked(int topicId);
	Task<AvatarUrls> UserAvatars(int userId);
	Task<Dictionary<int, (string, string)>> GetPostActivityOfSubforum(int subforumId);
}

internal class ForumService : IForumService
{
	internal const string LatestPostCacheKey = "Forum-LatestPost-Mapping";
	internal const string PostActivityOfTopicsCacheKey = "Forum-PostActivityOfTopics";
	internal const string PostActivityOfSubforumsCacheKey = "Forum-PostActivityOfSubforums";
	private readonly ApplicationDbContext _db;
	private readonly ICacheService _cacheService;
	private readonly ITopicWatcher _topicWatcher;

	public ForumService(
		ApplicationDbContext db,
		ICacheService cacheService,
		ITopicWatcher topicWatcher)
	{
		_db = db;
		_cacheService = cacheService;
		_topicWatcher = topicWatcher;
	}

	public async Task<PostPositionDto?> GetPostPosition(int postId, bool seeRestricted)
	{
		var post = await _db.ForumPosts
			.ExcludeRestricted(seeRestricted)
			.SingleOrDefaultAsync(p => p.Id == postId);

		if (post is null)
		{
			return null;
		}

		var posts = await _db.ForumPosts
			.ForTopic(post.TopicId ?? -1)
			.OldestToNewest()
			.ToListAsync();

		var position = posts.IndexOf(post);
		return new PostPositionDto(
			(position / ForumConstants.PostsPerPage) + 1,
			post.TopicId ?? 0);
	}

	public async Task<IReadOnlyCollection<ForumCategoryDisplayDto>> GetAllCategories()
	{
		var latestPostMappings = await GetAllLatestPosts();
		var dto = await _db.ForumCategories
			.Select(c => new ForumCategoryDisplayDto
			{
				Id = c.Id,
				Ordinal = c.Ordinal,
				Title = c.Title,
				Description = c.Description,
				Forums = c.Forums
					.Select(f => new ForumCategoryDisplayDto.Forum
					{
						Id = f.Id,
						Ordinal = f.Ordinal,
						Restricted = f.Restricted,
						Name = f.Name,
						Description = f.Description
					})
			})
			.ToListAsync();

		var allActivityPosts = await GetPostActivityOfSubforums();

		foreach (var forum in dto.SelectMany(c => c.Forums))
		{
			if (latestPostMappings.TryGetValue(forum.Id, out var lastPost))
			{
				forum.LastPost = lastPost;

			}
			allActivityPosts.TryGetValue(forum.Id, out var activityPosts);
			forum.ActivityPostsCreated = activityPosts.Item1 ?? "";
			forum.ActivityPostsEdited = activityPosts.Item2 ?? "";
		}

		return dto;
	}

	public void CacheLatestPost(int forumId, int topicId, LatestPost post)
	{
		if (_cacheService.TryGetValue(LatestPostCacheKey, out Dictionary<int, LatestPost?> dict))
		{
			dict[forumId] = post;
			_cacheService.Set(LatestPostCacheKey, dict, Durations.OneDayInSeconds);
		}
	}

	public void CacheNewPostActivity(int forumId, int topicId, int postId, DateTime createTimestamp)
	{
		if (_cacheService.TryGetValue(PostActivityOfTopicsCacheKey, out Dictionary<int, Dictionary<int, (string, string)>> forumActivity))
		{
			forumActivity.TryGetValue(forumId, out Dictionary<int, (string, string)>? subforumActivity);
			subforumActivity ??= new Dictionary<int, (string, string)>();
			subforumActivity.TryGetValue(topicId, out (string, string) postActivity);
			var createPostActivity = postActivity.Item1 == null ? new Dictionary<int, long>() : JsonSerializer.Deserialize<Dictionary<int, long>>(postActivity.Item1)!;
			createPostActivity[postId] = createTimestamp.UnixTimestamp();
			subforumActivity[topicId] = (JsonSerializer.Serialize(createPostActivity), postActivity.Item2);
			forumActivity[forumId] = subforumActivity;
			_cacheService.Set(PostActivityOfTopicsCacheKey, forumActivity, Durations.OneDayInSeconds);
		}
		if (_cacheService.TryGetValue(PostActivityOfSubforumsCacheKey, out Dictionary<int, (string, string)> fullSubforumActivity))
		{
			fullSubforumActivity.TryGetValue(forumId, out (string, string) postActivity);
			var createPostActivity = postActivity.Item1 == null ? new Dictionary<int, long>() : JsonSerializer.Deserialize<Dictionary<int, long>>(postActivity.Item1)!;
			createPostActivity[postId] = createTimestamp.UnixTimestamp();
			fullSubforumActivity[forumId] = (JsonSerializer.Serialize(createPostActivity), postActivity.Item2);
			_cacheService.Set(PostActivityOfSubforumsCacheKey, fullSubforumActivity, Durations.OneDayInSeconds);
		}
	}

	public void CacheEditedPostActivity(int forumId, int topicId, int postId, DateTime editTimestamp)
	{
		if (_cacheService.TryGetValue(PostActivityOfTopicsCacheKey, out Dictionary<int, Dictionary<int, (string, string)>> forumActivity))
		{
			forumActivity.TryGetValue(forumId, out Dictionary<int, (string, string)>? subforumActivity);
			subforumActivity ??= new Dictionary<int, (string, string)>();
			subforumActivity.TryGetValue(topicId, out (string, string) postActivity);
			var editPostActivity = postActivity.Item2 == null ? new Dictionary<int, long>() : JsonSerializer.Deserialize<Dictionary<int, long>>(postActivity.Item2)!;
			editPostActivity[postId] = editTimestamp.UnixTimestamp();
			subforumActivity[topicId] = (postActivity.Item1, JsonSerializer.Serialize(editPostActivity));
			forumActivity[forumId] = subforumActivity;
			_cacheService.Set(PostActivityOfTopicsCacheKey, forumActivity, Durations.OneDayInSeconds);
		}
		if (_cacheService.TryGetValue(PostActivityOfSubforumsCacheKey, out Dictionary<int, (string, string)> fullSubforumActivity))
		{
			fullSubforumActivity.TryGetValue(forumId, out (string, string) postActivity);
			var editPostActivity = postActivity.Item2 == null ? new Dictionary<int, long>() : JsonSerializer.Deserialize<Dictionary<int, long>>(postActivity.Item2)!;
			editPostActivity[postId] = editTimestamp.UnixTimestamp();
			fullSubforumActivity[forumId] = (postActivity.Item1, JsonSerializer.Serialize(editPostActivity));
			_cacheService.Set(PostActivityOfSubforumsCacheKey, fullSubforumActivity, Durations.OneDayInSeconds);
		}
	}

	public void ClearLatestPostCache()
	{
		_cacheService.Remove(LatestPostCacheKey);
	}

	public void ClearTopicActivityCache()
	{
		_cacheService.Remove(PostActivityOfTopicsCacheKey);
		_cacheService.Remove(PostActivityOfSubforumsCacheKey);
	}

	public async Task CreatePoll(ForumTopic topic, PollCreateDto pollDto)
	{
		var poll = new ForumPoll
		{
			TopicId = topic.Id,
			Question = pollDto.Question ?? "",
			CloseDate = pollDto.DaysOpen > 0
				? DateTime.UtcNow.AddDays(pollDto.DaysOpen.Value)
				: null,
			MultiSelect = pollDto.MultiSelect,
			PollOptions = pollDto.Options
				.Select((po, i) => new ForumPollOption
				{
					Text = po,
					Ordinal = i
				})
				.ToList()
		};

		_db.ForumPolls.Add(poll);
		topic.Poll = poll;
		await _db.SaveChangesAsync();
	}

	public async Task<int> CreatePost(PostCreateDto post)
	{
		var forumPost = new ForumPost
		{
			ForumId = post.ForumId,
			TopicId = post.TopicId,
			PosterId = post.PosterId,
			IpAddress = post.IpAddress,
			Subject = post.Subject,
			Text = post.Text.Replace("\r", ""),
			PosterMood = post.Mood,

			// New posts are always bbcode = true, html = false
			EnableHtml = false,
			EnableBbCode = true
		};
		_db.ForumPosts.Add(forumPost);
		await _db.SaveChangesAsync();
		CacheLatestPost(post.ForumId, post.TopicId, new LatestPost(forumPost.Id, forumPost.CreateTimestamp, post.PosterName));
		CacheNewPostActivity(post.ForumId, post.TopicId, forumPost.Id, forumPost.CreateTimestamp);

		if (post.WatchTopic)
		{
			await _topicWatcher.WatchTopic(post.TopicId, post.PosterId, canSeeRestricted: true);
		}
		else
		{
			await _topicWatcher.UnwatchTopic(post.TopicId, post.PosterId);
		}

		return forumPost.Id;
	}

	internal async Task<IDictionary<int, LatestPost?>> GetAllLatestPosts()
	{
		if (_cacheService.TryGetValue(LatestPostCacheKey, out Dictionary<int, LatestPost?> dict))
		{
			return dict;
		}

		var raw = await _db.Forums
			.Select(f => new
			{
				f.Id,
				LatestPost = f.ForumPosts.Select(fp => new
				{
					fp.Id,
					fp.CreateTimestamp,
					fp.Poster!.UserName
				})
					.FirstOrDefault(fp => fp.Id == f.ForumPosts.Max(fpp => fpp.Id))
			})
			.ToListAsync();

		dict = raw.ToDictionary(
			tkey => tkey.Id,
			tvalue => tvalue.LatestPost is null ? null : new LatestPost(
				tvalue.LatestPost.Id,
				tvalue.LatestPost.CreateTimestamp,
				tvalue.LatestPost.UserName ?? ""));

		_cacheService.Set(LatestPostCacheKey, dict, Durations.OneDayInSeconds);

		return dict;
	}

	internal async Task<Dictionary<int, Dictionary<int, (string, string)>>> GetPostActivityOfTopics()
	{
		if (_cacheService.TryGetValue(PostActivityOfTopicsCacheKey, out Dictionary<int, Dictionary<int, (string, string)>> forumActivity))
		{
			return forumActivity;
		}

		DateTime minimumDate = DateTime.UtcNow.AddDays(-ForumConstants.DaysPostsCountAsActive);

		var fullrow = await _db.ForumPosts
			.Where(fp => fp.CreateTimestamp > minimumDate || fp.PostEditedTimestamp > minimumDate)
			.Select(fp => new
			{
				fp.Id,
				TopicId = fp.TopicId ?? 0,
				fp.ForumId,
				fp.CreateTimestamp,
				fp.PostEditedTimestamp,
			})
			.ToListAsync();
		forumActivity = fullrow
			.GroupBy(fr => fr.ForumId)
			.ToDictionary(
				tkey => tkey.Key,
				tvalue => tvalue
					.GroupBy(ffr => ffr.TopicId)
					.ToDictionary(
						ttkey => ttkey.Key,
						ttvalue => (
							JsonSerializer.Serialize(ttvalue
								.Where(fffr => fffr.CreateTimestamp > minimumDate)
								.ToDictionary(
									tttkey => tttkey.Id,
									tttvalue => tttvalue.CreateTimestamp.UnixTimestamp())),
							JsonSerializer.Serialize(ttvalue
								.Where(fffr => fffr.PostEditedTimestamp != null && fffr.PostEditedTimestamp > minimumDate)
								.ToDictionary(
									tttkey => tttkey.Id,
									tttvalue => ((DateTime)tttvalue.PostEditedTimestamp!).UnixTimestamp()))
						)));

		_cacheService.Set(PostActivityOfTopicsCacheKey, forumActivity, Durations.OneDayInSeconds);

		return forumActivity;
	}

	public async Task<Dictionary<int, (string, string)>> GetPostActivityOfSubforum(int subforumId)
	{
		(await GetPostActivityOfTopics()).TryGetValue(subforumId, out var activityTopics);
		return activityTopics ?? new Dictionary<int, (string, string)>();
	}

	internal async Task<Dictionary<int, (string, string)>> GetPostActivityOfSubforums()
	{
		if (_cacheService.TryGetValue(PostActivityOfSubforumsCacheKey, out Dictionary<int, (string, string)> subforumActivity))
		{
			return subforumActivity;
		}

		DateTime minimumDate = DateTime.UtcNow.AddDays(-ForumConstants.DaysPostsCountAsActive);

		var fullrow = await _db.ForumPosts
			.Where(fp => fp.CreateTimestamp > minimumDate || fp.PostEditedTimestamp > minimumDate)
			.Select(fp => new
			{
				fp.Id,
				TopicId = fp.TopicId ?? 0,
				fp.ForumId,
				fp.CreateTimestamp,
				fp.PostEditedTimestamp,
			})
			.ToListAsync();
		subforumActivity = fullrow
			.GroupBy(fr => fr.ForumId)
			.ToDictionary(
				tkey => tkey.Key,
				tvalue => (
					JsonSerializer.Serialize(tvalue
						.Where(ffr => ffr.CreateTimestamp > minimumDate)
						.ToDictionary(
							ttkey => ttkey.Id,
							ttvalue => ttvalue.CreateTimestamp.UnixTimestamp())),
					JsonSerializer.Serialize(tvalue
						.Where(ffr => ffr.PostEditedTimestamp != null && ffr.PostEditedTimestamp > minimumDate)
						.ToDictionary(
							ttkey => ttkey.Id,
							ttvalue => ((DateTime)ttvalue.PostEditedTimestamp!).UnixTimestamp()))
				));

		_cacheService.Set(PostActivityOfSubforumsCacheKey, subforumActivity, Durations.OneDayInSeconds);

		return subforumActivity;
	}

	public async Task<bool> IsTopicLocked(int topicId)
	{
		return await _db.ForumTopics.AnyAsync(t => t.Id == topicId && t.IsLocked);
	}

	public async Task<AvatarUrls> UserAvatars(int userId)
	{
		return await _db.Users
			.Where(u => u.Id == userId)
			.Select(u => new AvatarUrls(u.Avatar, u.MoodAvatarUrlBase))
			.SingleAsync();
	}
}
