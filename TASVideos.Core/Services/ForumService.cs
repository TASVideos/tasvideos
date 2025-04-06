using System.Text.Json;

namespace TASVideos.Core.Services;

public interface IForumService
{
	Task<PostPosition?> GetPostPosition(int postId, bool seeRestricted);
	Task<IReadOnlyCollection<ForumCategoryDisplay>> GetAllCategories();
	void CacheLatestPost(int forumId, int topicId, LatestPost post);
	void CacheNewPostActivity(int forumId, int topicId, int postId, DateTime createTimestamp);
	void CacheEditedPostActivity(int forumId, int topicId, int postId, DateTime createTimestamp);
	void ClearLatestPostCache();
	void ClearTopicActivityCache();
	Task CreatePoll(ForumTopic topic, PollCreate poll);
	Task<int> CreatePost(PostCreate post);
	Task<bool> IsTopicLocked(int topicId);
	Task<AvatarUrls> UserAvatars(int userId);
	Task<Dictionary<int, (string PostsCreated, string PostsEdited)>> GetPostActivityOfSubforum(int subforumId);
	Task<int> GetTopicCountInForum(int userId, int topicId);
	Task<int> GetPostCountInTopic(int userId, int topicId);
}

internal class ForumService(
	ApplicationDbContext db,
	ICacheService cacheService,
	ITopicWatcher topicWatcher)
	: IForumService
{
	internal const string LatestPostCacheKey = "Forum-LatestPost-Mapping";
	internal const string PostActivityOfTopicsCacheKey = "Forum-PostActivityOfTopics";
	internal const string PostActivityOfSubforumsCacheKey = "Forum-PostActivityOfSubforums";

	public async Task<PostPosition?> GetPostPosition(int postId, bool seeRestricted)
	{
		var post = await db.ForumPosts
			.ExcludeRestricted(seeRestricted)
			.SingleOrDefaultAsync(p => p.Id == postId);

		if (post is null)
		{
			return null;
		}

		var posts = await db.ForumPosts
			.ForTopic(post.TopicId ?? -1)
			.OldestToNewest()
			.ToListAsync();

		var position = posts.IndexOf(post);
		return new PostPosition(
			(position / ForumConstants.PostsPerPage) + 1,
			post.TopicId ?? 0);
	}

	public async Task<IReadOnlyCollection<ForumCategoryDisplay>> GetAllCategories()
	{
		var latestPostMappings = await GetAllLatestPosts();
		var dto = await db.ForumCategories
			.Select(c => new ForumCategoryDisplay
			{
				Id = c.Id,
				Ordinal = c.Ordinal,
				Title = c.Title,
				Description = c.Description,
				Forums = c.Forums
					.Select(f => new ForumCategoryDisplay.Forum
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
			forum.ActivityPostsCreated = activityPosts.PostsCreated ?? "";
			forum.ActivityPostsEdited = activityPosts.PostsEdited ?? "";
		}

		return dto;
	}

	public void CacheLatestPost(int forumId, int topicId, LatestPost post)
	{
		if (cacheService.TryGetValue(LatestPostCacheKey, out Dictionary<int, LatestPost?> dict))
		{
			dict[forumId] = post;
			cacheService.Set(LatestPostCacheKey, dict, Durations.OneDay);
		}
	}

	public void CacheNewPostActivity(int forumId, int topicId, int postId, DateTime createTimestamp)
	{
		if (cacheService.TryGetValue(PostActivityOfTopicsCacheKey, out Dictionary<int, Dictionary<int, (string, string)>> forumActivity))
		{
			forumActivity.TryGetValue(forumId, out Dictionary<int, (string, string)>? subforumActivity);
			subforumActivity ??= [];
			subforumActivity.TryGetValue(topicId, out (string, string) postActivity);
			var createPostActivity = postActivity.Item1 == null ? [] : JsonSerializer.Deserialize<Dictionary<int, long>>(postActivity.Item1)!;
			createPostActivity[postId] = createTimestamp.UnixTimestamp();
			subforumActivity[topicId] = (JsonSerializer.Serialize(createPostActivity), postActivity.Item2);
			forumActivity[forumId] = subforumActivity;
			cacheService.Set(PostActivityOfTopicsCacheKey, forumActivity, Durations.OneDay);
		}

		if (cacheService.TryGetValue(PostActivityOfSubforumsCacheKey, out Dictionary<int, (string, string)> fullSubforumActivity))
		{
			fullSubforumActivity.TryGetValue(forumId, out (string, string) postActivity);
			var createPostActivity = postActivity.Item1 == null ? [] : JsonSerializer.Deserialize<Dictionary<int, long>>(postActivity.Item1)!;
			createPostActivity[postId] = createTimestamp.UnixTimestamp();
			fullSubforumActivity[forumId] = (JsonSerializer.Serialize(createPostActivity), postActivity.Item2);
			cacheService.Set(PostActivityOfSubforumsCacheKey, fullSubforumActivity, Durations.OneDay);
		}
	}

	public void CacheEditedPostActivity(int forumId, int topicId, int postId, DateTime editTimestamp)
	{
		if (cacheService.TryGetValue(PostActivityOfTopicsCacheKey, out Dictionary<int, Dictionary<int, (string, string)>> forumActivity))
		{
			forumActivity.TryGetValue(forumId, out Dictionary<int, (string, string)>? subforumActivity);
			subforumActivity ??= [];
			subforumActivity.TryGetValue(topicId, out (string, string) postActivity);
			var editPostActivity = postActivity.Item2 == null ? [] : JsonSerializer.Deserialize<Dictionary<int, long>>(postActivity.Item2)!;
			editPostActivity[postId] = editTimestamp.UnixTimestamp();
			subforumActivity[topicId] = (postActivity.Item1, JsonSerializer.Serialize(editPostActivity));
			forumActivity[forumId] = subforumActivity;
			cacheService.Set(PostActivityOfTopicsCacheKey, forumActivity, Durations.OneDay);
		}

		if (cacheService.TryGetValue(PostActivityOfSubforumsCacheKey, out Dictionary<int, (string, string)> fullSubforumActivity))
		{
			fullSubforumActivity.TryGetValue(forumId, out (string, string) postActivity);
			var editPostActivity = postActivity.Item2 == null ? [] : JsonSerializer.Deserialize<Dictionary<int, long>>(postActivity.Item2)!;
			editPostActivity[postId] = editTimestamp.UnixTimestamp();
			fullSubforumActivity[forumId] = (postActivity.Item1, JsonSerializer.Serialize(editPostActivity));
			cacheService.Set(PostActivityOfSubforumsCacheKey, fullSubforumActivity, Durations.OneDay);
		}
	}

	public void ClearLatestPostCache()
	{
		cacheService.Remove(LatestPostCacheKey);
	}

	public void ClearTopicActivityCache()
	{
		cacheService.Remove(PostActivityOfTopicsCacheKey);
		cacheService.Remove(PostActivityOfSubforumsCacheKey);
	}

	public async Task CreatePoll(ForumTopic topic, PollCreate pollDto)
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

		db.ForumPolls.Add(poll);
		topic.Poll = poll;
		await db.SaveChangesAsync();
	}

	public async Task<int> CreatePost(PostCreate post)
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
		db.ForumPosts.Add(forumPost);
		await db.SaveChangesAsync();
		CacheLatestPost(post.ForumId, post.TopicId, new LatestPost(forumPost.Id, forumPost.CreateTimestamp, post.PosterName));
		CacheNewPostActivity(post.ForumId, post.TopicId, forumPost.Id, forumPost.CreateTimestamp);

		if (post.WatchTopic)
		{
			await topicWatcher.WatchTopic(post.TopicId, post.PosterId, canSeeRestricted: true);
		}
		else
		{
			await topicWatcher.UnwatchTopic(post.TopicId, post.PosterId);
		}

		return forumPost.Id;
	}

	internal async Task<IDictionary<int, LatestPost?>> GetAllLatestPosts()
	{
		if (cacheService.TryGetValue(LatestPostCacheKey, out Dictionary<int, LatestPost?> dict))
		{
			return dict;
		}

		var raw = await db.Forums
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
				tvalue.LatestPost.UserName));

		cacheService.Set(LatestPostCacheKey, dict, Durations.OneDay);

		return dict;
	}

	internal async Task<Dictionary<int, Dictionary<int, (string PostsCreated, string PostsEdited)>>> GetPostActivityOfTopics()
	{
		if (cacheService.TryGetValue(PostActivityOfTopicsCacheKey, out Dictionary<int, Dictionary<int, (string, string)>> forumActivity))
		{
			return forumActivity;
		}

		DateTime minimumDate = DateTime.UtcNow.AddDays(-(ForumConstants.DaysPostsCountAsActive - 1)); // we subtract 1 day here because we cache activity for 1 day

		var fullrow = await db.ForumPosts
			.Where(fp => fp.CreateTimestamp > minimumDate || fp.PostEditedTimestamp > minimumDate)
			.Select(fp => new
			{
				fp.Id,
				TopicId = fp.TopicId ?? 0,
				fp.ForumId,
				fp.CreateTimestamp,
				fp.PostEditedTimestamp
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

		cacheService.Set(PostActivityOfTopicsCacheKey, forumActivity, Durations.OneDay);

		return forumActivity;
	}

	public async Task<Dictionary<int, (string, string)>> GetPostActivityOfSubforum(int subforumId)
	{
		(await GetPostActivityOfTopics()).TryGetValue(subforumId, out var activityTopics);
		return activityTopics ?? [];
	}

	public async Task<int> GetTopicCountInForum(int userId, int forumId)
		=> await db.ForumTopics
			.ForForum(forumId)
			.CountAsync(fp => fp.PosterId == userId);

	public async Task<int> GetPostCountInTopic(int userId, int topicId)
		=> await db.ForumPosts
			.ForTopic(topicId)
			.CountAsync(fp => fp.PosterId == userId);

	internal async Task<Dictionary<int, (string PostsCreated, string PostsEdited)>> GetPostActivityOfSubforums()
	{
		if (cacheService.TryGetValue(PostActivityOfSubforumsCacheKey, out Dictionary<int, (string, string)> subforumActivity))
		{
			return subforumActivity;
		}

		DateTime minimumDate = DateTime.UtcNow.AddDays(-(ForumConstants.DaysPostsCountAsActive - 1)); // we subtract 1 day here because we cache activity for 1 day

		var fullrow = await db.ForumPosts
			.Where(fp => fp.CreateTimestamp > minimumDate || fp.PostEditedTimestamp > minimumDate)
			.Select(fp => new
			{
				fp.Id,
				TopicId = fp.TopicId ?? 0,
				fp.ForumId,
				fp.CreateTimestamp,
				fp.PostEditedTimestamp
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

		cacheService.Set(PostActivityOfSubforumsCacheKey, subforumActivity, Durations.OneDay);

		return subforumActivity;
	}

	public async Task<bool> IsTopicLocked(int topicId) => await db.ForumTopics.AnyAsync(t => t.Id == topicId && t.IsLocked);

	public async Task<AvatarUrls> UserAvatars(int userId)
	{
		return await db.Users
			.Where(u => u.Id == userId)
			.Select(u => new AvatarUrls(u.Avatar, u.MoodAvatarUrlBase))
			.SingleAsync();
	}
}

public record LatestPost(int Id, DateTime Timestamp, string PosterName);

public class ForumCategoryDisplay
{
	public int Id { get; init; }
	public int Ordinal { get; init; }
	public string Title { get; init; } = "";
	public string? Description { get; init; }

	public IEnumerable<Forum> Forums { get; init; } = [];
	public class Forum
	{
		public int Id { get; init; }
		public int Ordinal { get; init; }
		public bool Restricted { get; init; }
		public string Name { get; init; } = "";
		public string? Description { get; init; }
		public LatestPost? LastPost { get; set; }
		public string ActivityPostsCreated { get; set; } = "";
		public string ActivityPostsEdited { get; set; } = "";
	}
}

public record PostPosition(int Page, int TopicId);

public record PollCreate(string? Question, int? DaysOpen, bool MultiSelect, IEnumerable<string> Options);
public record PostCreate(
	int ForumId,
	int TopicId,
	string? Subject,
	string Text,
	int PosterId,
	string PosterName,
	ForumPostMood Mood,
	string IpAddress,
	bool WatchTopic);

public record AvatarUrls(string? Avatar, string? MoodBase)
{
	public bool HasMoods => !string.IsNullOrWhiteSpace(MoodBase);
	public bool HasAvatar => !HasMoods && !string.IsNullOrWhiteSpace(Avatar);
	public string ToMoodUrl(ForumPostMood mood) => MoodBase?.Replace("$", ((int)mood).ToString()) ?? "";
}
