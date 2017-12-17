using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Models;

namespace TASVideos.Tasks
{
    public class WikiTasks
    {
		private readonly ApplicationDbContext _db;

		public WikiTasks(ApplicationDbContext db)
		{
			_db = db;
		}

		/// <summary>
		/// Returns details about a Wiki page with the given <see cref="pageName" />
		/// If a <see cref="revisionId" /> is provided then that revision of the page will be returned
		/// Else the latest revision is returned
		/// </summary>
		/// <returns>A model representing the Wiki page if it exists else null</returns>
		public async Task<WikiPage> GetPage(string pageName, int? revisionId = null) // TODO: ability to pass in a particular revision of a page
		{
			pageName = pageName?.Trim('/');
			return await _db.WikiPages
				.Where(wp => wp.PageName == pageName)
				.Where(wp => revisionId != null
					? wp.Revision == revisionId
					: wp.Child == null)
				.SingleOrDefaultAsync();
		}

		/// <summary>
		/// Returns details about a Wiki page with the given id
		/// </summary>
		/// <returns>A model representing the Wiki page if it exists else null</returns>
		public async Task<WikiPage> GetPage(int dbid)
		{
			return await _db.WikiPages
				.Where(wp => wp.Id == dbid)
				.SingleOrDefaultAsync();
		}

		/// <summary>
		/// Returns whether or not any revision of the given page exists
		/// </summary>
		public async Task<bool> PageExists(string pageName)
		{
			return await _db.WikiPages


				.AnyAsync(wp => wp.PageName == pageName);
		}

		// TODO: document
		public async Task SavePage(WikiEditModel model)
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
		}

		// TODO: document
		public async Task<WikiHistoryModel> GetPageHistory(string pageName)
		{
			pageName = pageName.Trim('/');

			return new WikiHistoryModel
			{
				PageName = pageName,
				Revisions = await _db.WikiPages
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

		// TODO: document
		public async Task MovePage(WikiMoveModel model)
		{
			var existingRevisions = await _db.WikiPages
				.Where(wp => wp.PageName == model.OriginalPageName)
				.ToListAsync();

			foreach (var revision in existingRevisions)
			{
				revision.PageName = model.DestinationPageName;
			}

			await _db.SaveChangesAsync();
		}

		// TODO: document
		public async Task<IEnumerable<string>> GetSubPages(string pageName)
		{
			pageName = pageName.Trim('/');
			return await _db.WikiPages
				.ThatAreCurrentRevisions()
				.Where(wp => wp.PageName != pageName)
				.Where(wp => wp.PageName.StartsWith(pageName))
				.Select(wp => wp.PageName)
				.ToListAsync();
		}

		// TODO: document
		// Gets all pages that refer to the given page
		public async Task<IEnumerable<WikiPageReferral>> GetReferrers(string pageName)
		{
			pageName = pageName.Trim('/');
			return await _db.WikiReferrals
				.Where(wr => wr.Referral == pageName)
				.ToListAsync();
		}

		public async Task<WikiDiffModel> GetLatestPageDiff(string pageName)
		{
			var revisions = await _db.WikiPages
				.Where(wp => wp.PageName == pageName)
				.OrderByDescending(wp => wp.Revision)
				.Take(2)
				.ToListAsync();

			if (!revisions.Any())
			{
				throw new InvalidOperationException($"Page \"{pageName}\" could not be found");
			}

			if (revisions.Count == 1) // Must have only 1 revision
			{
				return new WikiDiffModel
				{
					PageName = pageName,
					LeftRevision = 0,
					LeftMarkup = "",
					RightRevision = 1,
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
				.ThatAreCurrentRevisions()
				.Where(wp => !_db.WikiReferrals.Any(wr => wr.Referral == wp.PageName))
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
			return await _db.WikiReferrals
				.Where(wr => !_db.WikiPages.Any(wp => wp.PageName == wr.Referral))
				.ToListAsync();
		}
	}
}
