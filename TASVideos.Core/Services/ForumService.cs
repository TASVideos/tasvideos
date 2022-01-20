using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TASVideos.Data;

namespace TASVideos.Core.Services
{
	public interface IForumService
	{
		Task<ICollection<ForumCategoryDisplayDto>> GetAllCategories();
		void CacheLatestPost(int forumId, int topicId, LatestPost post);
	}

	internal class ForumService : IForumService
	{
		private const string LatestPostCacheKey = "Forum-LatestPost-Mapping";
		private readonly ApplicationDbContext _db;
		private readonly ICacheService _cacheService;

		public ForumService(ApplicationDbContext db, ICacheService cacheService)
		{
			_db = db;
			_cacheService = cacheService;
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
				_cacheService.Set(LatestPostCacheKey, dict);
			}
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

			_cacheService.Set(LatestPostCacheKey, dict);

			return dict;
		}
	}
}
