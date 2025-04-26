using System.Text.RegularExpressions;
using TASVideos.WikiEngine;

namespace TASVideos.Core.Services.Wiki;

public interface IWikiPages
{
	/// <summary>
	/// Returns whether any revision of the given page exists
	/// </summary>
	Task<bool> Exists(string? pageName, bool includeDeleted = false);

	/// <summary>
	/// Returns details about a Wiki page with the given <see cref="pageName" />
	/// If a <see cref="revisionId" /> is provided then that revision of the page will be returned
	/// Else the latest revision is returned
	/// </summary>
	/// <returns>A model representing the Wiki page if it exists else null</returns>
	ValueTask<IWikiPage?> Page(string? pageName, int? revisionId = null);

	/// <summary>
	/// Creates a new revision of a wiki page.
	/// If the created timestamp is less than the latest revision, the revision will not be added
	/// </summary>
	/// <return>The resulting wiki page revision if successfully added, null if it was unable to add</return>
	Task<IWikiPage?> Add(WikiCreateRequest addRequest);

	/// <summary>
	/// Renames the given wiki page to the destination name
	/// All revisions are renamed to the new page,
	/// and <seealso cref="WikiPageReferral" /> entries are updated
	/// </summary>
	/// <returns>Whether the move was successful.
	/// If false, a conflict was detected and no data was modified</returns>
	Task<bool> Move(string originalName, string destinationName);

	/// <summary>
	/// Moves the given page and all subpages as well
	/// </summary>
	Task<bool> MoveAll(string originalName, string destinationName);

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
	/// If the revision is the latest revision, then <see cref="WikiPageReferral"/>
	/// will be removed where the given page name is a referrer
	/// </summary>
	Task Delete(string pageName, int revision);

	/// <summary>
	/// Restores all revisions of the given page.
	/// If a current revision is restored, <seealso cref="WikiPageReferral" /> entries are updated.
	/// </summary>
	/// /// <returns>Whether undelete was successful.
	/// If false, a conflict was detected and no data was modified</returns>
	Task<bool> Undelete(string pageName);

	/// <summary>
	/// Clears the wiki cache
	/// </summary>
	Task FlushCache();

	/// <summary>
	/// Returns a collection of wiki pages that are not linked
	/// by any other wiki page. These pages are effectively "orphans"
	/// since they cannot be navigated to
	/// </summary>
	Task<IReadOnlyCollection<WikiOrphan>> Orphans();

	/// <summary>
	/// Returns a collection of wiki links that do not go to a page
	/// that exists. These links are considered broken and in need of fixing
	/// </summary>
	Task<IReadOnlyCollection<WikiPageReferral>> BrokenLinks();
}

// TODO: handle DbConcurrency exceptions
internal class WikiPages(ApplicationDbContext db, ICacheService cache) : IWikiPages
{
	private WikiResult? this[string pageName]
	{
		get
		{
			cache.TryGetValue($"{CacheKeys.CurrentWikiCache}-{pageName.ToLower()}", out WikiResult page);
			return page;
		}

		set => cache.Set($"{CacheKeys.CurrentWikiCache}-{pageName.ToLower()}", value, Durations.OneDay);
	}

	private void RemovePageFromCache(string pageName) =>
		cache.Remove($"{CacheKeys.CurrentWikiCache}-{pageName.ToLower()}");

	public async Task<IReadOnlyCollection<WikiOrphan>> Orphans() => await db.WikiPages
			.ThatAreNotDeleted()
			.ThatAreCurrent()
			.Where(wp => wp.PageName != "MediaPosts") // Linked by the navbar
			.Where(wp => !db.WikiReferrals.Any(wr => wr.Referral == wp.PageName))
			.Where(wp => !wp.PageName.StartsWith("InternalSystem")) // These by design aren't orphans they are directly used in the system
			.Where(wp => !wp.PageName.Contains("/")) // Subpages are linked by default by the parents, so we know they are not orphans
			.Select(wp => new WikiOrphan(
				wp.PageName,
				wp.LastUpdateTimestamp,
				wp.Author!.UserName))
			.ToListAsync();

	public async Task<IReadOnlyCollection<WikiPageReferral>> BrokenLinks() => await db.WikiReferrals
		.Where(wr => !Regex.IsMatch(wr.Referral, "(^[0-9]+)([GMS])"))
		.Where(wr => wr.Referrer != "SandBox")
		.Where(wr => !wr.Referrer.StartsWith("HomePages/Bisqwit/InitialWikiPages")) // Historical pages with legacy links
		.Where(wr => !db.WikiPages.Any(wp => wp.ChildId == null && wp.IsDeleted == false && wp.PageName == wr.Referral))
		.Where(wr => !wr.Referral.StartsWith("Subs-"))
		.Where(wr => !wr.Referral.StartsWith("Movies-"))
		.Where(wr => !string.IsNullOrWhiteSpace(wr.Referral))
		.ToListAsync();

	public async Task<bool> Exists(string? pageName, bool includeDeleted = false)
	{
		if (string.IsNullOrWhiteSpace(pageName))
		{
			return false;
		}

		pageName = pageName.Trim('/');

		var existingPage = this[pageName];

		if (existingPage is not null)
		{
			return true;
		}

		var query = db.WikiPages
			.ThatAreCurrent();

		if (!includeDeleted)
		{
			query = query.ThatAreNotDeleted();
		}

		var page = await query
			.ToWikiResult()
			.FirstOrDefaultAsync(wp => wp.PageName == pageName);

		if (page is not null && page.IsCurrent())
		{
			this[pageName] = page;
		}

		return page is not null;
	}

	public async ValueTask<IWikiPage?> Page(string? pageName, int? revisionId = null)
	{
		if (string.IsNullOrWhiteSpace(pageName))
		{
			return null;
		}

		pageName = pageName.Trim('/');

		WikiResult? page = null;
		if (!revisionId.HasValue)
		{
			page = this[pageName];
		}

		if (page is not null)
		{
			return page;
		}

		page = await db.WikiPages
			.ForPage(pageName)
			.ThatAreNotDeleted()
			.OrderByDescending(wp => wp.Revision)
			.ToWikiResult()
			.FirstOrDefaultAsync(w => revisionId != null
				? w.Revision == revisionId
				: w.ChildId == null);

		if (page is not null && page.IsCurrent())
		{
			this[pageName] = page;
		}

		return page;
	}

	public async Task<IWikiPage?> Add(WikiCreateRequest addRequest)
	{
		if (string.IsNullOrWhiteSpace(addRequest.PageName))
		{
			throw new InvalidOperationException($"{nameof(addRequest.PageName)} must have a value.");
		}

		var currentRevision = await db.WikiPages
			.ForPage(addRequest.PageName)
			.ThatAreNotDeleted()
			.ThatAreCurrent()
			.SingleOrDefaultAsync();

		if (addRequest.CreateTimestamp != DateTime.MinValue
			&& currentRevision is not null
			&& addRequest.CreateTimestamp < currentRevision.CreateTimestamp)
		{
			return null;
		}

		var author = await db.Users.SingleOrDefaultAsync(u => u.Id == addRequest.AuthorId)
			?? throw new InvalidOperationException($"A user with the id of {addRequest.AuthorId}");
		var newRevision = addRequest.ToWikiPage(author);

		newRevision.CreateTimestamp = DateTime.UtcNow; // we want the actual save time recorded
		db.WikiPages.Add(newRevision);

		if (currentRevision is not null)
		{
			currentRevision.Child = newRevision;
		}

		// We cannot assume the "current" revision is the latest
		// We might have a deleted revision after it
		var maxRevision = await db.WikiPages
			.ForPage(addRequest.PageName)
			.MaxAsync(r => (int?)r.Revision);
		if (maxRevision.HasValue)
		{
			newRevision.Revision = maxRevision.Value + 1;
		}

		try
		{
			await GenerateReferrals(addRequest.PageName, addRequest.Markup);
		}
		catch (DbUpdateConcurrencyException)
		{
			return null;
		}

		ClearCache(addRequest.PageName);
		var result = newRevision.ToWikiResult();
		this[addRequest.PageName] = result;
		return result;
	}

	public async Task<bool> Move(string originalName, string destinationName)
	{
		if (string.IsNullOrWhiteSpace(destinationName))
		{
			throw new ArgumentException($"{destinationName} must have a value.");
		}

		originalName = originalName.Trim('/');
		destinationName = destinationName.Trim('/');

		// TODO: support moving a page to a deleted page
		// Revision ids would have to be adjusted, but it could be done
		if (await Exists(destinationName, includeDeleted: true))
		{
			throw new InvalidOperationException($"Cannot move {originalName} to {destinationName} because {destinationName} already exists.");
		}

		var existingRevisions = await db.WikiPages
			.ForPage(originalName)
			.ToListAsync();

		foreach (var revision in existingRevisions)
		{
			revision.PageName = destinationName;
		}

		// Update all Referrals.
		// Referrals can be safely updated since the new page still has the original content
		// and any links on them are still correctly referring to other pages
		var existingReferrals = await db.WikiReferrals
			.ForPage(originalName)
			.ToListAsync();

		foreach (var referral in existingReferrals)
		{
			referral.Referrer = destinationName;
		}

		try
		{
			await db.SaveChangesAsync();
		}
		catch (DbUpdateException)
		{
			// Either the original pages were modified or the destination page
			// was created during this call from another thread
			return false;
		}

		// Note that we cannot update Referrers since the wiki pages will
		// still physically refer to the original page. Those links are
		// broken, and it is important to keep them listed as broken, so they
		// can show up in the Broken Links module for editors to see and fix.
		// Anyone doing a move operation should know to check broken links afterward
		var cachedRevision = this[originalName];
		if (cachedRevision is not null)
		{
			RemovePageFromCache(originalName);
			cachedRevision.SetPageName(destinationName);
			cache.Set(destinationName, cachedRevision);
		}

		return true;
	}

	public async Task<bool> MoveAll(string originalName, string destinationName)
	{
		var pagesToMove = await db.WikiPages
			.Where(wp => wp.PageName.StartsWith(originalName))
			.ThatAreCurrent()
			.ToListAsync();
		bool allSucceeded = true;
		foreach (var page in pagesToMove)
		{
			var oldPage = page.PageName;
			var newPage = destinationName + page.PageName[originalName.Length..];
			var result = await Move(oldPage, newPage);

			if (!result)
			{
				allSucceeded = false;
			}
		}

		return allSucceeded;
	}

	public async Task<int> Delete(string pageName)
	{
		pageName = pageName.Trim('/');
		var revisions = await db.WikiPages
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
		var referrers = await db.WikiReferrals
			.ForPage(pageName)
			.ToListAsync();

		db.RemoveRange(referrers);

		try
		{
			await db.SaveChangesAsync();
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
		pageName = pageName.Trim('/');
		var wikiPage = await db.WikiPages
			.ThatAreNotDeleted()
			.Revision(pageName, revision)
			.SingleOrDefaultAsync();

		if (wikiPage is not null)
		{
			var isCurrent = wikiPage.IsCurrent();

			if (isCurrent)
			{
				cache.Remove(pageName);
			}

			wikiPage.IsDeleted = true;

			// Update referrers if latest revision
			if (wikiPage.Child is null)
			{
				var referrers = await db.WikiReferrals
					.ForPage(pageName)
					.ToListAsync();

				db.RemoveRange(referrers);
			}

			await db.SaveChangesAsync();

			if (isCurrent)
			{
				// Set the previous page as current, if there is one
				var newCurrent = db.WikiPages
					.Include(wp => wp.Author)
					.ThatAreNotDeleted()
					.ForPage(pageName)
					.OrderByDescending(wp => wp.Revision)
					.FirstOrDefault();

				if (newCurrent is not null)
				{
					newCurrent.ChildId = null;
					this[pageName] = newCurrent.ToWikiResult();
					await GenerateReferrals(pageName, newCurrent.Markup);
				}
			}
		}
	}

	public async Task<bool> Undelete(string pageName)
	{
		pageName = pageName.Trim('/');
		var allRevisions = await db.WikiPages
			.Include(r => r.Author)
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
				if (previous is not null)
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
			this[pageName] = current.ToWikiResult();
		}

		return true;
	}

	public async Task FlushCache()
	{
		var allPages = await db.WikiPages
			.Select(wp => wp.PageName)
			.Distinct()
			.ToListAsync();

		foreach (var page in allPages)
		{
			ClearCache(page);
		}
	}

	private void ClearCache(string pageName)
	{
		cache.Remove(CacheKeys.CurrentWikiCache + "-" + pageName.ToLower());
	}

	private async Task GenerateReferrals(string pageName, string markup)
	{
		var existingReferrals = await db.WikiReferrals
			.ForPage(pageName)
			.ToListAsync();

		db.WikiReferrals.RemoveRange(existingReferrals);

		var referrers = Util.GetReferrals(markup)
			.Select(wl => new WikiPageReferral
			{
				Referrer = pageName,
				Referral = wl.Link,
				Excerpt = wl.Excerpt
			});

		db.WikiReferrals.AddRange(referrers);
		await db.SaveChangesAsync();
	}
}

public static class WikiPageExtensions
{
	/// <summary>
	/// Returns a System page with the given page suffix
	/// <example>SystemPage("Languages") will return the page System/Languages</example>
	/// </summary>
	public static ValueTask<IWikiPage?> SystemPage(this IWikiPages pages, string pageName, int? revisionId = null)
	{
		return pages.Page("System/" + pageName, revisionId);
	}

	public static async Task<IWikiPage?> PublicationPage(this IWikiPages pages, int publicationId)
	{
		return await pages.Page(WikiHelper.ToPublicationWikiPageName(publicationId));
	}

	public static async Task<IWikiPage?> SubmissionPage(this IWikiPages pages, int submissionId)
	{
		return await pages.Page(WikiHelper.ToSubmissionWikiPageName(submissionId));
	}

	public static WikiPage ToWikiPage(this WikiCreateRequest revision, User user)
	{
		return new WikiPage
		{
			PageName = revision.PageName,
			Markup = revision.Markup,
			RevisionMessage = revision.RevisionMessage,
			AuthorId = revision.AuthorId,
			CreateTimestamp = revision.CreateTimestamp,
			MinorEdit = revision.MinorEdit,
			Revision = 1,
			Author = user
		};
	}

	public static IQueryable<WikiResult> ToWikiResult(this IQueryable<WikiPage> query)
	{
		return query.Select(wp => new WikiResult
		{
			PageName = wp.PageName,
			Markup = wp.Markup,
			Revision = wp.Revision,
			RevisionMessage = wp.RevisionMessage,
			AuthorId = wp.AuthorId,
			AuthorName = wp.Author!.UserName,
			CreateTimestamp = wp.CreateTimestamp,
			MinorEdit = wp.MinorEdit,
			ChildId = wp.ChildId,
			IsDeleted = wp.IsDeleted
		});
	}

	public static WikiResult ToWikiResult(this WikiPage wp)
	{
		if (wp.Author is null)
		{
			throw new ArgumentNullException($"{nameof(wp.Author)} cannot be null.");
		}

		return new WikiResult
		{
			PageName = wp.PageName,
			Markup = wp.Markup,
			Revision = wp.Revision,
			RevisionMessage = wp.RevisionMessage,
			AuthorId = wp.AuthorId,
			AuthorName = wp.Author.UserName,
			CreateTimestamp = wp.CreateTimestamp,
			MinorEdit = wp.MinorEdit,
			ChildId = wp.ChildId,
			IsDeleted = wp.IsDeleted
		};
	}
}

public class WikiCreateRequest
{
	public string PageName { get; init; } = "";
	public string Markup { get; init; } = "";
	public string? RevisionMessage { get; init; }
	public int AuthorId { get; init; }
	public bool MinorEdit { get; init; }
	public DateTime CreateTimestamp { get; init; } = DateTime.UtcNow;
}

public class WikiResult : IWikiPage
{
	private string _pageName = "";

	public string PageName { get => _pageName; init => _pageName = value; }
	public string Markup { get; init; } = "";
	public int Revision { get; init; }
	public string? RevisionMessage { get; init; }
	public int? AuthorId { get; init; }
	public string? AuthorName { get; init; }
	public bool IsCurrent() => !ChildId.HasValue && !IsDeleted;
	public DateTime CreateTimestamp { get; init; }
	public bool MinorEdit { get; init; }

	internal int? ChildId { get; init; }
	internal bool IsDeleted { get; init; }

	internal void SetPageName(string newPageName)
	{
		_pageName = newPageName;
	}
}

public record WikiOrphan(string PageName, DateTime LastUpdateTimestamp, string? LastUpdateUserName);
