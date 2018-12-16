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
		Task<WikiPage> Create(WikiCreateDto dto);

		/// <summary>
		/// Returns whether or not any revision of the given page exists
		/// </summary>
		bool Exists(string pageName, bool includeDeleted = false);

		/// <summary>
		/// Returns details about a Wiki page with the given <see cref="pageName" />
		/// If a <see cref="revisionId" /> is provided then that revision of the page will be returned
		/// Else the latest revision is returned
		/// </summary>
		/// <returns>A model representing the Wiki page if it exists else null</returns>
		WikiPage Revision(string pageName, int? revisionId = null);

		/// <summary>
		/// Returns details about a Wiki page with the given id
		/// </summary>
		/// <returns>A model representing the Wiki page if it exists else null</returns>
		WikiPage Revision(int dbId);

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

		public bool Exists(string pageName, bool includeDeleted = false)
		{
			var query = includeDeleted
				? WikiCache
				: WikiCache.ThatAreNotDeleted();

			return query
				.Any(wp => wp.PageName == pageName);
		}

		public WikiPage Revision(string pageName, int? revisionId = null)
		{
			return WikiCache
				.ForPage(pageName)
				.ThatAreNotDeleted()
				.FirstOrDefault(w => (revisionId != null ? w.Revision == revisionId : w.ChildId == null));
		}

		public WikiPage Revision(int dbId)
		{
			return WikiCache
				.ThatAreNotDeleted()
				.FirstOrDefault(w => w.Id == dbId);
		}

		public async Task<WikiPage> Create(WikiCreateDto dto)
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

		public async Task PreLoadCache()
		{
			var wikiPages = await _db.WikiPages
				.ThatAreCurrentRevisions()
				.ToListAsync();

			WikiCache.AddRange(wikiPages);
		}

		public void ClearCache()
		{
			_cache.Remove(CacheKeys.WikiCache);
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
