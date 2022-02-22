using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Data.Entity.Forum;

namespace TASVideos.Core.Services;

public interface IForumService
{
	Task<PostPositionDto?> GetPostPosition(int postId, bool seeRestricted);
	Task<ICollection<ForumCategoryDisplayDto>> GetAllCategories();
	void CacheLatestPost(int forumId, int topicId, LatestPost post);
	void CacheNewPostActivity(int forumId, int topicId, DateTime createTimestamp);
	void ClearLatestPostCache();
	void ClearTopicActivityCache();
	Task CreatePoll(ForumTopic topic, PollCreateDto poll);
	Task<int> CreatePost(PostCreateDto post);
	Task<Dictionary<int, DateTime>?> GetTopicsWithActivity(int subforumId);
	Task<bool> IsTopicLocked(int topicId);
	Task<AvatarUrls> UserAvatars(int userId);
}

internal class ForumService : IForumService
{
	internal const string LatestPostCacheKey = "Forum-LatestPost-Mapping";
	internal const string TopicActivityCacheKey = "Forum-TopicActivity";
	internal const string TopicActivityJsonCacheKey = "Forum-TopicActivityJson";
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

		if (post == null)
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

	public async Task<ICollection<ForumCategoryDisplayDto>> GetAllCategories()
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

		var activitySubforums = await GetSubforumsWithActivity();

		foreach (var forum in dto.SelectMany(c => c.Forums))
		{
			if (latestPostMappings.TryGetValue(forum.Id, out var lastPost))
			{
				forum.LastPost = lastPost;

			}
			activitySubforums.TryGetValue(forum.Id, out var activityTopics);
			forum.ActivityTopics = activityTopics ?? "";
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

	public void CacheNewPostActivity(int forumId, int topicId, DateTime createTimestamp)
	{
		if (_cacheService.TryGetValue(TopicActivityCacheKey, out Dictionary<int, Dictionary<int, DateTime>> forumActivity))
		{
			forumActivity.TryGetValue(forumId, out Dictionary<int, DateTime>? subforumActivity);
			subforumActivity ??= new Dictionary<int, DateTime>();
			subforumActivity[topicId] = createTimestamp;
			forumActivity[forumId] = subforumActivity;
			_cacheService.Set(TopicActivityCacheKey, forumActivity, Durations.OneDayInSeconds);
		}
		if (_cacheService.TryGetValue(TopicActivityJsonCacheKey, out Dictionary<int, string> jsonForumActivity))
		{
			var dictSubforumActivity = new Dictionary<int, string>();
			jsonForumActivity.TryGetValue(forumId, out string? jsonSubforumActivity);
			if (jsonSubforumActivity != null)
			{
				dictSubforumActivity = JsonSerializer.Deserialize<Dictionary<int, string>>(jsonSubforumActivity) ?? new Dictionary<int, string>();
			}
			dictSubforumActivity[topicId] = createTimestamp.UnixTimestamp().ToString();
			jsonForumActivity[forumId] = JsonSerializer.Serialize(dictSubforumActivity);
			_cacheService.Set(TopicActivityJsonCacheKey, jsonForumActivity, Durations.OneDayInSeconds);
		}
	}

	public void ClearLatestPostCache()
	{
		_cacheService.Remove(LatestPostCacheKey);
	}

	public void ClearTopicActivityCache()
	{
		_cacheService.Remove(TopicActivityCacheKey);
		_cacheService.Remove(TopicActivityJsonCacheKey);
	}

	public async Task CreatePoll(ForumTopic topic, PollCreateDto pollDto)
	{
		var poll = new ForumPoll
		{
			TopicId = topic.Id,
			Question = pollDto.Question ?? "",
			CloseDate = pollDto.DaysOpen.HasValue
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
		CacheNewPostActivity(post.ForumId, post.TopicId, forumPost.CreateTimestamp);

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
			tvalue => tvalue.LatestPost == null ? null : new LatestPost(
				tvalue.LatestPost.Id,
				tvalue.LatestPost.CreateTimestamp,
				tvalue.LatestPost.UserName ?? ""));

		_cacheService.Set(LatestPostCacheKey, dict, Durations.OneDayInSeconds);

		return dict;
	}

	internal async Task<Dictionary<int,Dictionary<int,DateTime>>> GetAllForumActivity()
	{
		if(_cacheService.TryGetValue(TopicActivityCacheKey, out Dictionary<int, Dictionary<int, DateTime>> forumActivity))
		{
			return forumActivity;
		}

		var fullrow = await _db.ForumPosts
			.Where(fp => fp.CreateTimestamp > DateTime.UtcNow.AddDays(-ForumConstants.DaysTopicsCountAsActive))
			.Select(fp => new
			{
				fp.Id,
				TopicId = fp.TopicId ?? 0,
				fp.ForumId,
				fp.CreateTimestamp
			})
			.ToListAsync();
		forumActivity = fullrow
			.GroupBy(fr => fr.ForumId)
			.ToDictionary(
				tkey => tkey.Key,
				tvalue => tvalue
					.GroupBy(ffr => ffr.TopicId)
					.ToDictionary(ttkey => ttkey.Key, ttvalue => ttvalue.Max(fffr => fffr.CreateTimestamp)));

		_cacheService.Set(TopicActivityCacheKey, forumActivity, Durations.OneDayInSeconds);

		return forumActivity;
	}

	internal async Task<Dictionary<int, string>> GetSubforumsWithActivity()
	{
		if (_cacheService.TryGetValue(TopicActivityJsonCacheKey, out Dictionary<int, string> subforumActivity))
		{
			return subforumActivity;
		}
		subforumActivity = new Dictionary<int, string>();
		var forumActivity = await GetAllForumActivity();
		foreach (var (subforumId, dict) in forumActivity)
		{
			var stringDict = new Dictionary<int, string>();
			foreach (var (topicId, datetime) in dict)
			{
				stringDict[topicId] = datetime.UnixTimestamp().ToString();
			}
			subforumActivity[subforumId] = JsonSerializer.Serialize(stringDict);
		}

		_cacheService.Set(TopicActivityJsonCacheKey, subforumActivity, Durations.OneDayInSeconds);

		return subforumActivity;
	}

	public async Task<Dictionary<int, DateTime>?> GetTopicsWithActivity(int subforumId)
	{
		var forumActivity = await GetAllForumActivity();
		forumActivity.TryGetValue(subforumId, out Dictionary<int, DateTime>? topicsWithActivity);
		return topicsWithActivity;
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
