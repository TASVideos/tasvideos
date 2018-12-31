using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Data.Helpers;
using TASVideos.Models;
using TASVideos.Services;
using TASVideos.ViewComponents;

namespace TASVideos.Tasks
{
	public class WikiTasks
	{
		private readonly ApplicationDbContext _db;
		private readonly IWikiPages _wikiPages;

		public WikiTasks(
			ApplicationDbContext db,
			IWikiPages wikiPages)
		{
			_db = db;
			_wikiPages = wikiPages;
		}

		/// <summary>
		/// Returns a list of all pages that are considered subpages
		/// of the page with the given <see cref="pageName"/>
		/// </summary>
		public IEnumerable<string> GetSubPages(string pageName)
		{
			pageName = pageName.Trim('/');
			return _wikiPages
				.ThatAreNotDeleted()
				.ThatAreCurrentRevisions()
				.Where(wp => wp.PageName != pageName)
				.Where(wp => wp.PageName.StartsWith(pageName + "/"))
				.Select(wp => wp.PageName)
				.ToList();
		}

		/// <summary>
		/// Returns a list of all pages that are considered parents
		/// of the page with the given <see cref="pageName"/>
		/// </summary>
		public IEnumerable<string> GetParents(string pageName)
		{
			pageName = pageName.Trim('/');
			return _wikiPages
				.ThatAreNotDeleted()
				.ThatAreCurrentRevisions()
				.Where(wp => wp.PageName != pageName)
				.Where(wp => pageName.StartsWith(wp.PageName))
				.Select(wp => wp.PageName)
				.ToList();
		}

		/// <summary>
		/// Returns the data necessary to generate a diff of the
		/// latest revision of the wiki page with the given <see cref="pageName"/>
		/// compared against the previous revision
		/// </summary>
		/// <exception cref="InvalidOperationException">thrown if the given <see cref="pageName" /> does not exist</exception>
		public async Task<WikiDiffModel> GetLatestPageDiff(string pageName)
		{
			var revisions = await _db.WikiPages
				.ForPage(pageName)
				.ThatAreNotDeleted()
				.OrderByDescending(wp => wp.Revision)
				.Take(2)
				.ToListAsync();

			if (!revisions.Any())
			{
				throw new InvalidOperationException($"Page \"{pageName}\" could not be found");
			}

			// If count is 1, it must be a new page with no history, so compare against nothing
			if (revisions.Count == 1)
			{
				return new WikiDiffModel
				{
					PageName = pageName,
					LeftRevision = revisions.First().Revision - 1,
					LeftMarkup = "",
					RightRevision = revisions.First().Revision,
					RightMarkup = revisions.First().Markup
				};
			}
			
			return new WikiDiffModel
			{
				PageName = pageName,
				LeftRevision = revisions[1].Revision,
				LeftMarkup = revisions[1].Markup,
				RightRevision = revisions[0].Revision,
				RightMarkup = revisions[0].Markup
			};
		}

		/// <summary>
		/// Returns the data necessary to generate a diff between
		/// two revisions of the given page
		/// </summary>
		/// <exception cref="InvalidOperationException">If the given <see cref="pageName"/> does not exists
		/// or if the given <see cref="fromRevision"/> or <see cref="toRevision"/> do not exist
		/// </exception>
		public async Task<WikiDiffModel> GetPageDiff(string pageName, int fromRevision, int toRevision)
		{
			var revisions = await _db.WikiPages
				.ForPage(pageName)
				.Where(wp => wp.Revision == fromRevision
					|| wp.Revision == toRevision)
				.ToListAsync();

			if (fromRevision > 0 && revisions.Count != 2)
			{
				throw new InvalidOperationException($"Page \"{pageName}\" or revisions {fromRevision}-{toRevision} could not be found");
			}

			return new WikiDiffModel
			{
				PageName = pageName,
				LeftRevision = fromRevision,
				RightRevision = toRevision,
				LeftMarkup = fromRevision > 0 ? revisions.Single(wp => wp.Revision == fromRevision).Markup : "",
				RightMarkup = revisions.Single(wp => wp.Revision == toRevision).Markup
			};
		}

		/// <summary>
		/// Returns a list of all <see cref="WikiPage"/> records that at not linked by another page
		/// </summary>
		public async Task<IEnumerable<WikiOrphanModel>> GetAllOrphans()
		{
			return await _db.WikiPages
				.ThatAreNotDeleted()
				.ThatAreCurrentRevisions()
				.Where(wp => wp.PageName != "MediaPosts") // Linked by the navbar
				.Where(wp => !_db.WikiReferrals.Any(wr => wr.Referral == wp.PageName))
				.Where(wp => !wp.PageName.StartsWith("System/")
					&& !wp.PageName.StartsWith("InternalSystem")) // These by design aren't orphans they are directly used in the system
				.Where(wp => !wp.PageName.Contains("/")) // Subpages are linked by default by the parents, so we know they are not orphans
				.Select(wp => new WikiOrphanModel
				{
					PageName = wp.PageName,
					LastUpdateTimeStamp = wp.LastUpdateTimeStamp,
					LastUpdateUserName = wp.LastUpdateUserName ?? wp.CreateUserName
				})
				.ToListAsync();
		}

		/// <summary>
		/// Returns a list of all <see cref="WikiPageReferral"/> records that do not link to any existing page
		/// </summary>
		public async Task<IEnumerable<WikiPageReferral>> GetAllBrokenLinks()
		{
			return (await _db.WikiReferrals
					.Where(wr => wr.Referrer != "SandBox")
					.Where(wr => wr.Referral != "Players-List")
					.Where(wr => !_db.WikiPages.Any(wp => wp.PageName == wr.Referral))
					.Where(wr => !wr.Referral.StartsWith("Subs-"))
					.Where(wr => !wr.Referral.StartsWith("Movies-"))
					.Where(wr => !wr.Referral.StartsWith("/forum"))
					.Where(wr => !wr.Referral.StartsWith("/userfiles"))
					.Where(wr => !string.IsNullOrWhiteSpace(wr.Referral))
					.Where(wr => wr.Referral != "FrontPage")
					.ToListAsync())
				.Where(wr => !SubmissionHelper.IsSubmissionLink(wr.Referral).HasValue)
				.Where(wr => !SubmissionHelper.IsPublicationLink(wr.Referral).HasValue)
				.Where(wr => !SubmissionHelper.IsGamePageLink(wr.Referral).HasValue);
		}

		/// <summary>
		/// Returns a list of information about <see cref="WikiPage"/> entries
		/// ordered by timestamp with the given criteria
		/// </summary>
		public async Task<IEnumerable<WikiTextChangelogModel>> GetWikiChangeLog(int limit, bool includeMinorEdits)
		{
			var query = _db.WikiPages
				.ThatAreNotDeleted()
				.ByMostRecent()
				.Take(limit);

			if (!includeMinorEdits)
			{
				query = query.ExcludingMinorEdits();
			}

			return await query
				.Select(wp => new WikiTextChangelogModel
				{
					PageName = wp.PageName,
					Revision = wp.Revision,
					Author = wp.CreateUserName,
					CreateTimestamp = wp.CreateTimeStamp,
					MinorEdit = wp.MinorEdit,
					RevisionMessage = wp.RevisionMessage
				})
				.ToListAsync();
		}

		/// <summary>
		/// Returns a list of all deleted pages for the purpose of display
		/// </summary>
		public async Task<IEnumerable<DeletedWikiPageDisplayModel>> GetDeletedPages()
		{
			var results = await _db.WikiPages
				.ThatAreDeleted()
				.GroupBy(tkey => tkey.PageName)
				.Select(record => new DeletedWikiPageDisplayModel
				{
					PageName = record.Key,
					RevisionCount = record.Count(),

					// https://github.com/aspnet/EntityFrameworkCore/issues/3103
					// EF Core 2.1 bug, this no longer works, "Must be reducible node exception
					// HasExistingRevisions = _db.WikiPages.Any(wp => !wp.IsDeleted && wp.PageName == record.Key)
				})
				.ToListAsync();

			// Workaround for EF Core 2.1 issue
			// https://github.com/aspnet/EntityFrameworkCore/issues/3103
			// Since we know the cache is up to date we can do the logic there and avoid n+1 trips to the db
			await _wikiPages.PreLoadCache();
			foreach (var result in results)
			{
				result.HasExistingRevisions = _wikiPages.Any(wp => wp.PageName == result.PageName);
			}

			return results;
		}

		public async Task<IEnumerable<GameSubpageModel>> GetGameResourcesSubPages()
		{
			using (await _db.Database.BeginTransactionAsync())
			{
				var systems = await _db.GameSystems.ToListAsync();
				var gameResourceSystems = systems.Select(s => "GameResources/" + s.Code);

				var pages = _wikiPages
					.ThatAreNotDeleted()
					.ThatAreCurrentRevisions()
					.Where(wp => gameResourceSystems.Contains(wp.PageName))
					.Select(wp => wp.PageName)
					.ToList();

				return
					(from s in systems
					join wp in pages on s.Code equals wp.Split('/').Last()
					select new GameSubpageModel
					{
						SystemCode = s.Code,
						SystemDescription = s.DisplayName,
						PageLink = "GameResources/" + s.Code
					})
					.ToList();
			}
		}
	}
}
