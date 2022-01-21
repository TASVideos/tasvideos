using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Data.Entity.Forum;

namespace TASVideos.Core.Services
{
	public interface IForumService
	{
		Task<PostPositionDto?> GetPostPosition(int postId, bool seeRestricted);
		Task<ICollection<ForumCategoryDisplayDto>> GetAllCategories();
		void CacheLatestPost(int forumId, int topicId, LatestPost post);
		void ClearCache();
		Task CreatePoll(ForumTopic topic, PollCreateDto poll);
		Task<int> CreatePost(PostCreateDto post);
	}

	internal class ForumService : IForumService
	{
		internal const string LatestPostCacheKey = "Forum-LatestPost-Mapping";
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

			foreach (var forum in dto.SelectMany(c => c.Forums))
			{
				forum.LastPost = latestPostMappings[forum.Id];
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

		public void ClearCache()
		{
			_cacheService.Remove(LatestPostCacheKey);
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
				Text = post.Text,
				PosterMood = post.Mood,

				// New posts are always bbcode = true, html = false
				EnableHtml = false,
				EnableBbCode = true
			};
			_db.ForumPosts.Add(forumPost);
			await _db.SaveChangesAsync();
			CacheLatestPost(post.ForumId, post.TopicId, new LatestPost(forumPost.Id, forumPost.CreateTimestamp, post.PosterName));

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
	}
}
