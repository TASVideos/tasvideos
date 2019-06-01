using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.WikiEngine;

namespace TASVideos.Services
{
	public interface IWikiPages
	{
		/// <summary>
		/// Returns whether or not any revision of the given page exists
		/// </summary>
		Task<bool> Exists(string pageName, bool includeDeleted = false);

		/// <summary>
		/// Returns details about a Wiki page with the given <see cref="pageName" />
		/// If a <see cref="revisionId" /> is provided then that revision of the page will be returned
		/// Else the latest revision is returned
		/// </summary>
		/// <returns>A model representing the Wiki page if it exists else null</returns>
		Task<WikiPage> Page(string pageName, int? revisionId = null);

		/// <summary>
		/// Returns details about a Wiki page with the given id
		/// </summary>
		/// <returns>A model representing the Wiki page if it exists else null</returns>
		Task<WikiPage> Revision(int dbId);

		/// <summary>
		/// Creates a new revision of a wiki page
		/// </summary>
		Task Add(WikiPage revision);

		/// <summary>
		/// Renames the given wiki page to the destination name
		/// All revisions are renamed to the new page
		/// and <seealso cref="WikiPageReferral" /> entries are updated
		/// </summary>
		/// <returns>Whether or not the move was successful.
		/// If false, a conflict was detected and no data was modified</returns>
		Task<bool> Move(string originalName, string destinationName);

		/// <summary>
		/// Performs a soft delete on all revisions of the given page name,
		/// In addition <see cref="WikiPageReferral"/> entries are updated
		/// to remove entries where the given page name is a referrer
		/// </summary>
		/// <returns>The number of revisions that were deleted
		/// If -1, a conflict was detected and no data was modified</returns>
		Task<int> Delete(string pageName);

		/// <summary>
		/// Performs a soft delete on a single revision of a <see cref="WikiPage"/>
		/// If the revision is latest revisions, then <see cref="WikiPageReferral"/>
		/// will be removed where the given page name is a referrer
		/// </summary>
		Task Delete(string pageName, int revision);

		/// <summary>
		/// Restores all revisions of the given page.
		/// If a current revision is restored, <seealso cref="WikiPageReferral" /> entries are updated.
		/// </summary>
		/// /// <returns>Whether or not the undelete was successful.
		/// If false, a conflict was detected and no data was modified</returns>
		Task<bool> Undelete(string pageName);

		/// <summary>
		/// Clears the wiki cache
		/// </summary>
		Task FlushCache();

		/// <summary>
		/// Populates the cache with likely accessed latest revisions
		/// of wiki pages, such as publications and commonly accessed pages
		/// </summary>
		void PrePopulateCache();

		/// <summary>
		/// Filters the list of wiki pages to only pages that are nest beneath the given page.
		/// If no pageName is provided, then a master list of subpages is provided
		/// ex: /Foo/Bar, /Foo/Bar2 and /Foo/Bar/Baz are all subpages of /Foo
		/// </summary>
		/// <seealso cref="WikiPage"/>
		/// <param name="pageName">the name of the page to get sub pages from</param>
		IQueryable<WikiPage> ThatAreSubpagesOf(string pageName);

		/// <summary>
		/// Filters the list of wiki pages to only pages that are parents of the given page
		/// ex: /Foo is a parent of /Foo/Bar
		/// ex: /Foo and /Foo/Bar are parents of /Foo/Bar/Baz
		/// </summary>
		/// <seealso cref="WikiPage"/>
		/// <param name="pageName">the name of the page to get parent pages from</param>
		IQueryable<WikiPage> ThatAreParentsOf(string pageName);
	}

	// TODO: handle DbConcurrency exceptions
	public class WikiPages : IWikiPages
	{
		private readonly ApplicationDbContext _db;
		private readonly ICacheService _cache;

		public WikiPages(
			ApplicationDbContext db,
			ICacheService cache)
		{
			_db = db;
			_cache = cache;
		}

		private WikiPage this[string pageName]
		{
			get
			{
				_cache.TryGetValue($"{CacheKeys.CurrentWikiCache}-{pageName}", out WikiPage page);
				return page;
			}

			set => _cache.Set($"{CacheKeys.CurrentWikiCache}-{pageName}", value, Durations.OneYearInSeconds);
		}

		public async Task<bool> Exists(string pageName, bool includeDeleted = false)
		{
			var existingPage = this[pageName];

			if (existingPage != null)
			{
				return true;
			}

			var query = _db.WikiPages
				.ThatAreCurrentRevisions();

			if (!includeDeleted)
			{
				query = query.ThatAreNotDeleted();
			}

			var page = await query
				.SingleOrDefaultAsync(wp => wp.PageName == pageName);

			if (page.IsCurrent())
			{
				this[pageName] = page;
			}

			return page != null;
		}

		public async Task<WikiPage> Page(string pageName, int? revisionId = null)
		{
			pageName = (pageName ?? "").Trim('/');

			WikiPage page = null;
			if (!revisionId.HasValue)
			{
				page = this[pageName];
			}

			if (page != null)
			{
				return page;
			}

			page = await _db.WikiPages
				.ForPage(pageName)
				.ThatAreNotDeleted()
				.OrderByDescending(wp => wp.Revision)
				.FirstOrDefaultAsync(w => (revisionId != null
					? w.Revision == revisionId
					: w.ChildId == null));

			if (page.IsCurrent())
			{
				this[pageName] = page;
			}

			return page;
		}

		public async Task<WikiPage> Revision(int dbId)
		{
			var page = await _db.WikiPages
				.ThatAreNotDeleted()
				.FirstOrDefaultAsync(w => w.Id == dbId);

			if (page.IsCurrent())
			{
				this[page.PageName] = page;
			}

			return page;
		}

		public async Task Add(WikiPage revision)
		{
			_db.WikiPages.Add(revision);

			var currentRevision = await _db.WikiPages
				.ForPage(revision.PageName)
				.ThatAreNotDeleted()
				.ThatAreCurrentRevisions()
				.SingleOrDefaultAsync();

			if (currentRevision != null)
			{
				currentRevision.Child = revision;

				// We can assume the "current" revision is the latest
				// We might have a deleted revision after it
				var maxRevision = await _db.WikiPages
					.ForPage(revision.PageName)
					.MaxAsync(r => r.Revision);

				revision.Revision = maxRevision + 1;
			}

			await GenerateReferrals(revision.PageName, revision.Markup);
		
			ClearCache(revision.PageName);
			this[revision.PageName] = revision;
		}

		public async Task<bool> Move(string originalName, string destinationName)
		{
			if (string.IsNullOrWhiteSpace(destinationName))
			{
				throw new ArgumentException($"{destinationName} must have a value.");
			}

			// TODO: support moving a page to a deleted page
			// Revision ids would have to be adjusted but it could be done
			if (await Exists(destinationName, includeDeleted: true))
			{
				throw new InvalidOperationException($"Cannot move {originalName} to {destinationName} because {destinationName} already exists.");
			}

			var existingRevisions = await _db.WikiPages
				.ForPage(originalName)
				.ToListAsync();

			foreach (var revision in existingRevisions)
			{
				revision.PageName = destinationName;
			}

			// Update all Referrals
			// Referrals can be safely updated since the new page still has the original content 
			// and any links on them are still correctly referring to other pages
			var existingReferrals = await _db.WikiReferrals
				.ForPage(originalName)
				.ToListAsync();

			foreach (var referral in existingReferrals)
			{
				referral.Referrer = destinationName;
			}

			try
			{
				await _db.SaveChangesAsync();
			}
			catch (DbUpdateException)
			{
				// Either the original pages were modified or the destination page
				// was created during this call from another thread
				return false;
			}

			// Note that we can not update Referrers since the wiki pages will
			// still physically refer to the original page. Those links are
			// broken and it is important to keep them listed as broken so they
			// can show up in the Broken Links module for editors to see and fix.
			// Anyone doing a move operation should know to check broken links afterwards
			var cachedRevision = this[originalName];
			if (cachedRevision != null)
			{
				cachedRevision.PageName = destinationName;
			}

			return true;
		}

		public async Task<int> Delete(string pageName)
		{
			var revisions = await _db.WikiPages
				.ForPage(pageName)
				.ThatAreNotDeleted()
				.ToListAsync();

			foreach (var revision in revisions)
			{
				revision.IsDeleted = true;
				revision.ChildId = null;
			}

			// Remove referrals
			// Note: Pages that refer to this page will not be removed
			// It's important for them to remain and show as broken links
			var referrers = await _db.WikiReferrals
				.ForPage(pageName)
				.ToListAsync();

			_db.RemoveRange(referrers);

			try
			{
				await _db.SaveChangesAsync();
			}
			catch (DbUpdateConcurrencyException)
			{
				// revisions were modified by another thread during this call
				// Note that we aren't catching DbUpdateException
				// As there are no anticipated scenarios that could cause this
				return -1;
			}

			ClearCache(pageName);
			return revisions.Count;
		}

		public async Task Delete(string pageName, int revision)
		{
			var wikiPage = await _db.WikiPages
				.ThatAreNotDeleted()
				.Revision(pageName, revision)
				.SingleOrDefaultAsync();

			if (wikiPage != null)
			{
				var isCurrent = wikiPage.IsCurrent();

				if (isCurrent)
				{
					_cache.Remove(pageName);
				}

				wikiPage.IsDeleted = true;

				// Update referrers if latest revision
				if (wikiPage.Child == null)
				{
					var referrers = await _db.WikiReferrals
						.ForPage(pageName)
						.ToListAsync();

					_db.RemoveRange(referrers);
				}

				await _db.SaveChangesAsync();

				if (isCurrent)
				{
					// Set the previous page as current, if there is one
					var newCurrent = _db.WikiPages
						.ThatAreNotDeleted()
						.ForPage(pageName)
						.OrderByDescending(wp => wp.Revision)
						.FirstOrDefault();

					if (newCurrent != null)
					{
						newCurrent.ChildId = null;
						this[pageName] = newCurrent;
						await GenerateReferrals(pageName, newCurrent.Markup);
					}
				}
			}
		}

		public async Task<bool> Undelete(string pageName)
		{
			var allRevisions = await _db.WikiPages
				.ForPage(pageName)
				.ToListAsync();

			var revisions = allRevisions
				.ThatAreDeleted()
				.ToList();

			if (revisions.Any())
			{
				foreach (var revision in revisions)
				{
					revision.IsDeleted = false;
					var previous = allRevisions
						.FirstOrDefault(r => r.Revision == revision.Revision - 1);
					if (previous != null)
					{
						previous.ChildId = revision.Id;
					}
				}

				var current = revisions
					.OrderByDescending(r => r.Revision)
					.First();

				try
				{
					// Calls SaveChanges()
					await GenerateReferrals(pageName, current.Markup);
				}
				catch (DbUpdateConcurrencyException)
				{
					// revisions were modified by another thread during this call
					// Note that we aren't catching DbUpdateException
					// As there are no anticipated scenarios that could cause this
					return false;
				}

				ClearCache(pageName);
				this[pageName] = current;
			}

			return true;
		}

		public async Task FlushCache()
		{
			var allPages = await _db.WikiPages
				.Select(wp => wp.PageName)
				.Distinct()
				.ToListAsync();

			foreach (var page in allPages)
			{
				ClearCache(page);
			}
		}

		public void PrePopulateCache()
		{
			var currentPages = _db.WikiPages
				.ThatAreNotDeleted()
				.ThatAreCurrentRevisions()
				.ToList();

			foreach (var page in currentPages)
			{
				this[page.PageName] = page;
			}
		}

		// TODO: consider caching these
		public IQueryable<WikiPage> ThatAreSubpagesOf(string pageName)
		{
			pageName = (pageName ?? "").Trim('/');
			var query = _db.WikiPages
				.ThatAreNotDeleted()
				.ThatAreCurrentRevisions()
				.Where(wp => wp.PageName != pageName);

			if (!string.IsNullOrWhiteSpace(pageName))
			{
				query = query.Where(wp => wp.PageName.StartsWith(pageName + "/"));
			}

			return query;
		}

		// TODO: consider caching these
		public IQueryable<WikiPage> ThatAreParentsOf(string pageName)
		{
			pageName = (pageName ?? "").Trim('/');
			if (string.IsNullOrWhiteSpace(pageName)
				|| !pageName.Contains('/')) // Easy optimization, pages without a / have no parents
			{
				return Enumerable.Empty<WikiPage>().AsQueryable();
			}

			return _db.WikiPages
				.ThatAreNotDeleted()
				.ThatAreCurrentRevisions()
				.Where(wp => wp.PageName != pageName)
				.Where(wp => pageName.StartsWith(wp.PageName));
		}

		private void ClearCache(string pageName)
		{
			_cache.Remove(CacheKeys.CurrentWikiCache + "-" + pageName);
		}

		private async Task GenerateReferrals(string pageName, string markup)
		{
			var existingReferrals = await _db.WikiReferrals
				.ForPage(pageName)
				.ToListAsync();

			_db.WikiReferrals.RemoveRange(existingReferrals);

			var referrers = Util.GetAllInternalLinks(markup ?? "")
				.Select(wl => new WikiPageReferral
				{
					Referrer = pageName,
					Referral = wl.Link?.Trim('/'), // TODO: is it correct for GetAllInternalLinks to have slashes on Referrals and not Referrers?
					Excerpt = wl.Excerpt
				});

			_db.WikiReferrals.AddRange(referrers);
			await _db.SaveChangesAsync();
		}
	}

	public static class WikiPageExtensions
	{
		/// <summary>
		/// Returns a System page with the given page suffix
		/// <example>SystemPage("Languages") will return the page System/Languages</example>
		/// </summary>
		public static Task<WikiPage> SystemPage(this IWikiPages pages, string pageName, int? revisionId = null)
		{
			return pages.Page("System/" + pageName, revisionId);
		}
	}
}
