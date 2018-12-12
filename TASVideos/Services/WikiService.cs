using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TASVideos.Data;
using TASVideos.Data.Constants;
using TASVideos.Data.Entity;
using TASVideos.Services.Dtos;
using TASVideos.WikiEngine;

namespace TASVideos.Services
{
	public interface IWikiService
	{
		/// <summary>
		/// Creates a new revision of a wiki page
		/// </summary>
		Task<WikiPage> CreateRevision(WikiCreateDto dto);

		/// <summary>
		/// Clears the wiki cache
		/// </summary>
		void ClearCache();
	}

	public class WikiService : IWikiService
	{
		private readonly ApplicationDbContext _db;
		private readonly ICacheService _cache;

		private List<WikiPage> WikiCache
		{
			get
			{
				var cacheKey = CacheKeys.WikiCache;
				if (_cache.TryGetValue(cacheKey, out List<WikiPage> pages))
				{
					return pages;
				}

				pages = new List<WikiPage>();
				_cache.Set(cacheKey, pages, Durations.OneWeekInSeconds);
				LoadWikiCache().Wait();
				return pages;
			}
		}

		public WikiService(
			ApplicationDbContext db,
			ICacheService cache)
		{
			_db = db;
			_cache = cache;
		}

		public void ClearCache()
		{
			_cache.Remove(CacheKeys.WikiCache);
		}

		public async Task<WikiPage> CreateRevision(WikiCreateDto dto)
		{
			var newRevision = new WikiPage
			{
				PageName = dto.PageName,
				Markup = dto.Markup,
				MinorEdit = dto.MinorEdit,
				RevisionMessage = dto.RevisionMessage
			};

			_db.WikiPages.Add(newRevision);

			var currentRevision = await _db.WikiPages
				.ForPage(dto.PageName)
				.ThatAreCurrentRevisions()
				.SingleOrDefaultAsync();

			if (currentRevision != null)
			{
				currentRevision.Child = newRevision;
				newRevision.Revision = currentRevision.Revision + 1;
			}

			await GenerateReferrals(dto.PageName, dto.Markup);
		
			var cachedCurrentRevision = WikiCache
				.ForPage(dto.PageName)
				.ThatAreCurrentRevisions()
				.FirstOrDefault();
			if (cachedCurrentRevision != null)
			{
				cachedCurrentRevision.Child = newRevision;
				cachedCurrentRevision.ChildId = newRevision.Id;
			}

			WikiCache.Add(newRevision);
			return newRevision;
		}

		// Loads all current wiki pages into the WikiCache
		private async Task LoadWikiCache()
		{
			var wikiPages = await _db.WikiPages
				.ThatAreCurrentRevisions()
				.ToListAsync();

			WikiCache.AddRange(wikiPages);
		}

		private async Task GenerateReferrals(string pageName, string markup)
		{
			var existingReferrals = await _db.WikiReferrals
				.ThatReferTo(pageName)
				.ToListAsync();

			_db.WikiReferrals.RemoveRange(existingReferrals);

			var referrers = Util.GetAllWikiLinks(markup)
				.Select(wl => new WikiPageReferral
				{
					Referrer = pageName,
					Referral = wl.Link,
					Excerpt =  wl.Excerpt
				});

			_db.WikiReferrals.AddRange(referrers);
			await _db.SaveChangesAsync();
		}
	}
}
