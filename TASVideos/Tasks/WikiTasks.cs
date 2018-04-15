using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using TASVideos.Data;
using TASVideos.Data.Constants;
using TASVideos.Data.Entity;
using TASVideos.Data.Helpers;
using TASVideos.Models;
using TASVideos.Services;

namespace TASVideos.Tasks
{
	public class WikiTasks
	{
		private readonly ApplicationDbContext _db;
		private readonly ICacheService _cache;

		public WikiTasks(
			ApplicationDbContext db,
			ICacheService cache)
		{
			_db = db;
			_cache = cache;
		}

		private List<WikiPage> WikiCache
		{
			get
			{
				var cacheKey = "WikiCache";
				if (_cache.TryGetValue(cacheKey, out List<WikiPage> pages))
				{
					return pages;
				}

				pages = new List<WikiPage>();
				_cache.Set(cacheKey, pages, DurationConstants.OneWeekInSeconds);
				return pages;
			}
		}


		/// <summary>
		/// Loads all current wiki pages, intended to be run on startup to pre-load the cache
		/// </summary>
		public async Task LoadWikiCache()
		{
			var wikiPages = await _db.WikiPages
				.ThatAreCurrentRevisions()
				.ThatAreNotDeleted()
				.ToListAsync();

			WikiCache.AddRange(wikiPages);
		}

		/// <summary>
		/// Returns details about a Wiki page with the given <see cref="pageName" />
		/// If a <see cref="revisionId" /> is provided then that revision of the page will be returned
		/// Else the latest revision is returned
		/// </summary>
		/// <returns>A model representing the Wiki page if it exists else null</returns>
		public async Task<WikiPage> GetPage(string pageName, int? revisionId = null)
		{
			var cachedPage = WikiCache
				.FirstOrDefault(w => w.PageName == pageName
					&& (revisionId != null ? w.Revision == revisionId : w.ChildId == null));
			if (cachedPage != null)
			{
				return cachedPage;
			}

			pageName = pageName?.Trim('/');
			var result = await _db.WikiPages
				.ThatAreNotDeleted()
				.Where(wp => wp.PageName == pageName)
				.Where(wp => revisionId != null
					? wp.Revision == revisionId
					: wp.Child == null)
				.SingleOrDefaultAsync();

			if (result != null)
			{
				WikiCache.Add(result);
			}

			return result;
		}

		/// <summary>
		/// Returns details about a Wiki page with the given id
		/// </summary>
		/// <returns>A model representing the Wiki page if it exists else null</returns>
		public async Task<WikiPage> GetPageById(int dbid)
		{
			var cachedPage = WikiCache.FirstOrDefault(w => w.Id == dbid);
			if (cachedPage != null)
			{
				return cachedPage;
			}

			var result = await _db.WikiPages
				.ThatAreNotDeleted()
				.Where(wp => wp.Id == dbid)
				.SingleOrDefaultAsync();

			if (result != null)
			{
				WikiCache.Add(result);
			}

			return result;
		}

		/// <summary>
		/// Returns whether or not any revision of the given page exists
		/// </summary>
		public async Task<bool> PageExists(string pageName, bool includeDeleted = false)
		{
			var query = includeDeleted
				? _db.WikiPages
				: _db.WikiPages.ThatAreNotDeleted();

			return await query
				.AnyAsync(wp => wp.PageName == pageName);
		}

		/// <summary>
		/// Creates a new <see cref="WikiPage"/> with the given data
		/// If the given page does not exist this will be a new page set at revision 1
		/// If it is an existing page it will be a new revision of that page that
		/// will be considered to be the latest revision of this page
		/// <returns>The id of the page created</returns>
		/// <seealso cref="WikiPageReferral"/> entries are also updated
		/// </summary>
		public async Task<int> SavePage(WikiEditModel model)
		{
			var newRevision = new WikiPage
			{
				PageName = model.PageName,
				Markup = model.Markup,
				MinorEdit = model.MinorEdit,
				RevisionMessage = model.RevisionMessage
			};

			_db.WikiPages.Add(newRevision);

			var currentRevision = await _db.WikiPages
				.Where(wp => wp.PageName == model.PageName)
				.Where(wp => wp.Child == null)
				.SingleOrDefaultAsync();

			if (currentRevision != null)
			{
				currentRevision.Child = newRevision;
				newRevision.Revision = currentRevision.Revision + 1;
			}

			// Update Referrerals for this page
			var existingReferrals = await _db.WikiReferrals
				.Where(wr => wr.Referrer == model.PageName)
				.ToListAsync();

			_db.WikiReferrals.RemoveRange(existingReferrals);

			foreach (var newReferral in model.Referrals)
			{
				_db.WikiReferrals.Add(new WikiPageReferral
				{
					Referrer = model.PageName,
					Referral = newReferral.Link?.Split('|').FirstOrDefault(),
					Excerpt = newReferral.Excerpt
				});
			}

			await _db.SaveChangesAsync();

			WikiCache.Add(newRevision);
			var cachedCurrentRevision = WikiCache.FirstOrDefault(w => w.PageName == model.PageName && w.ChildId == null);
			if (cachedCurrentRevision != null)
			{
				cachedCurrentRevision.Child = newRevision;
				cachedCurrentRevision.ChildId = newRevision.Id;
			}

			return newRevision.Id;
		}

		/// <summary>
		/// Returns a revision history for the <see cref="WikiPage"/>
		/// with the given <see cref="pageName" />
		/// </summary>
		public async Task<WikiHistoryModel> GetPageHistory(string pageName)
		{
			pageName = pageName.Trim('/');

			return new WikiHistoryModel
			{
				PageName = pageName,
				Revisions = await _db.WikiPages
					.ThatAreNotDeleted()
					.Where(wp => wp.PageName == pageName)
					.OrderBy(wp => wp.Revision)
					.Select(wp => new WikiHistoryModel.WikiRevisionModel
					{
						Revision = wp.Revision,
						CreateTimeStamp = wp.CreateTimeStamp,
						CreateUserName = wp.CreateUserName,
						MinorEdit = wp.MinorEdit,
						RevisionMessage = wp.RevisionMessage
					})
					.ToListAsync()
			};
		}

		public async Task<UserWikiEditHistoryModel> GetEditHistoryForUser(string userName)
		{
			return new UserWikiEditHistoryModel
			{
				UserName = userName,
				Edits = await _db.WikiPages
					.ThatAreNotDeleted()
					.Where(wp => wp.CreateUserName == userName)
					.OrderByDescending(wp => wp.CreateTimeStamp)
					.ProjectTo<UserWikiEditHistoryModel.EditEntry>()
					.ToListAsync()
			};
		}

		/// <summary>
		/// Renames the given wiki page to the destination page
		/// All revisions are renamed to the new page
		/// and <seealso cref="WikiPageReferral" /> entries are updated
		/// </summary>
		public async Task MovePage(WikiMoveModel model)
		{
			// TODO: support moving a page to a deleted page
			// Revision ids would have to be adjusted but it coudl be done
			if (await PageExists(model.DestinationPageName, includeDeleted: true))
			{
				throw new InvalidOperationException($"Cannot move {model.OriginalPageName} to {model.DestinationPageName} because {model.DestinationPageName} already exists.");
			}

			var existingRevisions = await _db.WikiPages
				.Where(wp => wp.PageName == model.OriginalPageName)
				.ToListAsync();

			foreach (var revision in existingRevisions)
			{
				revision.PageName = model.DestinationPageName;

				var cachedRevision = WikiCache.FirstOrDefault(w => w.Id == revision.Id);
				if (cachedRevision != null)
				{
					cachedRevision.PageName = model.DestinationPageName;
				}
			}

			await _db.SaveChangesAsync();

			// Update all Referrals
			// Referrals can be safely updated since the new page still has the original content 
			// and any links on them are still correctly referring to other pages
			var existingReferrals = await _db.WikiReferrals
				.Where(wr => wr.Referral == model.OriginalPageName)
				.ToListAsync();

			foreach (var referral in existingReferrals)
			{
				referral.Referral = model.DestinationPageName;
			}

			await _db.SaveChangesAsync();

			// Note that we can not update Referrers since the wiki pages will still
			// Physically refer to the original page. Those links are broken and it is
			// Important to keep them listed as broken so they can show up in the Broken Links module
			// for editors to see and fix. Anyone doing a move operation should know to check broken links
			// afterwards
		}

		/// <summary>
		/// Returns a list of all pages that are considered subpages
		/// of the page with the given <see cref="pageName"/>
		/// </summary>
		public async Task<IEnumerable<string>> GetSubPages(string pageName)
		{
			pageName = pageName.Trim('/');
			return await _db.WikiPages
				.ThatAreNotDeleted()
				.ThatAreCurrentRevisions()
				.Where(wp => wp.PageName != pageName)
				.Where(wp => wp.PageName.StartsWith(pageName))
				.Select(wp => wp.PageName)
				.ToListAsync();
		}

		/// <summary>
		/// Returns a list of all pages that are considered parents
		/// of the page with the given <see cref="pageName"/>
		/// </summary>
		public async Task<IEnumerable<string>> GetParents(string pageName)
		{
			pageName = pageName.Trim('/');
			return await _db.WikiPages
				.ThatAreNotDeleted()
				.ThatAreCurrentRevisions()
				.Where(wp => wp.PageName != pageName)
				.Where(wp => pageName.StartsWith(wp.PageName))
				.Select(wp => wp.PageName)
				.ToListAsync();
		}

		/// <summary>
		/// Returns a list of all wiki pages that have a link (reference)
		/// to the given <see cref="pageName"/>
		/// </summary>
		public async Task<IEnumerable<WikiPageReferral>> GetReferrers(string pageName)
		{
			pageName = pageName.Trim('/');
			return await _db.WikiReferrals
				.Where(wr => wr.Referral == pageName)
				.ToListAsync();
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
				.ThatAreNotDeleted()
				.Where(wp => wp.PageName == pageName)
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
				.Where(wp => wp.PageName == pageName)
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
				.Where(wp => !_db.WikiReferrals.Any(wr => wr.Referral == wp.PageName))
				.Where(wp => !wp.PageName.StartsWith("System/")) // These by design aren't orphans they are directly used in the system
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
					.Where(wr => !_db.WikiPages.Any(wp => wp.PageName == wr.Referral))
					.ToListAsync())
				.Where(wr => !SubmissionHelper.IsSubmissionLink(wr.Referral).HasValue)
				.Where(wr => !SubmissionHelper.IsPublicationLink(wr.Referral).HasValue);
		}

		/// <summary>
		/// Returns a list of information about <see cref="WikiPage"/> entries
		/// ordered by timestamp with the given criteria
		/// </summary>
		public async Task<IEnumerable<WikiTextChangelogModel>> GetWikiChangeLog(int limit, bool includeMinorEdits)
		{
			var query = _db.WikiPages
				.ThatAreNotDeleted()
				.OrderByDescending(wp => wp.CreateTimeStamp)
				.Take(limit);

			if (!includeMinorEdits)
			{
				query = query.Where(wp => !wp.MinorEdit);
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
		/// Performs a soft delete on all revisions of the given page name,
		/// In addition <see cref="WikiPageReferral"/> entries are updated
		/// to remove entries where the given page name is a referrer
		/// </summary>
		public async Task DeleteWikiPage(string pageName)
		{
			var revisions = await _db.WikiPages
				.Where(wp => wp.PageName == pageName)
				.ThatAreNotDeleted()
				.ToListAsync();

			foreach (var revision in revisions)
			{
				revision.IsDeleted = true;
			}

			var cachedRevisions = WikiCache
				.Where(w => w.PageName == pageName)
				.AsQueryable()
				.ThatAreNotDeleted()
				.ToList();

			foreach (var cachedRevision in cachedRevisions)
			{
				cachedRevision.IsDeleted = true;
			}

			// Remove referrers
			var referrers = await _db.WikiReferrals
				.Where(wp => wp.Referrer == pageName)
				.ToListAsync();

			_db.RemoveRange(referrers);

			await _db.SaveChangesAsync();
		}

		/// <summary>
		/// Performs a soft delete on a single revision of a <see cref="WikiPage"/>
		/// If the revision is latest revisions, then <see cref="WikiPageReferral"/>
		/// will be removed where the given page name is a referrer
		/// </summary>
		public async Task DeleteWikiPageRevision(string pageName, int revision)
		{
			var wikiPage = await _db.WikiPages
				.ThatAreNotDeleted()
				.SingleOrDefaultAsync(wp => wp.PageName == pageName && wp.Revision == revision);

			if (wikiPage != null)
			{
				wikiPage.IsDeleted = true;

				var cachedRevision = WikiCache
					.AsQueryable()
					.ThatAreNotDeleted()
					.SingleOrDefault(w => w.PageName == pageName && w.Revision == revision);

				if (cachedRevision != null)
				{
					cachedRevision.IsDeleted = true;
				}

				// Update referrers if latest revision
				if (wikiPage.Child == null)
				{
					var referrers = await _db.WikiReferrals
						.Where(wp => wp.Referrer == pageName)
						.ToListAsync();

					_db.RemoveRange(referrers);
				}

				await _db.SaveChangesAsync();
			}
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
					HasExistingRevisions = _db.WikiPages.Any(wp => !wp.IsDeleted && wp.PageName == record.Key)
				})
				.ToListAsync();

			return results;
		}

		/// <summary>
		/// Undeletes all revisions of the given page
		/// </summary>
		public async Task UndeletePage(string pageName)
		{
			var revisions = await _db.WikiPages
				.ThatAreDeleted()
				.Where(wp => wp.PageName == pageName)
				.ToListAsync();

			foreach (var revision in revisions)
			{
				revision.IsDeleted = false;

				var cachedRevision = WikiCache
					.FirstOrDefault(w => w.Id == revision.Id);

				if (cachedRevision != null)
				{
					cachedRevision.IsDeleted = false;
				}
			}

			await _db.SaveChangesAsync();
		}
	}
}
