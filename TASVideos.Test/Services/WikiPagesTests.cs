using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TASVideos.Data.Entity;
using TASVideos.Services;

// ReSharper disable InconsistentNaming
namespace TASVideos.Test.Services
{
	// TODO: update and concurrency exceptions on Add
	[TestClass]
	public class WikiPagesTests
	{
		private IWikiPages _wikiPages;
		private TestDbContext _db;
		private WikiTestCache _cache;

		[TestInitialize]
		public void Initialize()
		{
			_db = TestDbContext.Create();
			_cache = new WikiTestCache();
			_wikiPages = new WikiPages(_db, _cache);
		}

		#region Exists

		[TestMethod]
		public async Task Exists_PageExists_ReturnsTrue()
		{
			string existingPage = "Exists";
			AddPage(existingPage);

			var actual = await _wikiPages.Exists(existingPage);
			Assert.AreEqual(1, _cache.PageCache.Count, "Cache should have 1 record");
			Assert.AreEqual(existingPage, _cache.PageCache.First().PageName, "Cache should match page checked");
			Assert.IsTrue(actual);
		}

		[TestMethod]
		public async Task Exists_PageDoesNotExist_ReturnsFalse()
		{
			string existingPage = "Exists";
			AddPage(existingPage);

			var actual = await _wikiPages.Exists("DoesNotExist");
			Assert.IsFalse(actual);
		}

		[TestMethod]
		public async Task Exists_OnlyDeletedExists_IncludeDeleted_ReturnsTrue()
		{
			string existingPage = "Exists";
			AddPage(existingPage, isDeleted: true);

			var actual = await _wikiPages.Exists(existingPage, includeDeleted: true);
			Assert.AreEqual(0, _cache.PageCache.Count, "Cache should have no records");
			Assert.IsTrue(actual);
		}

		[TestMethod]
		public async Task Exists_OnlyDeletedExists_DoNotIncludeDeleted_ReturnFalse()
		{
			string existingPage = "Exists";
			AddPage(existingPage, isDeleted: true);

			var actual = await _wikiPages.Exists(existingPage);
			Assert.AreEqual(0, _cache.PageCache.Count, "Non-existent page was not cached.");
			Assert.IsFalse(actual);
		}

		[TestMethod]
		[DataRow(null)]
		[DataRow("")]
		[DataRow("\r \n \t")]
		public async Task Exists_NoPageName_ReturnsFalse(string pageName)
		{
			AddPage(pageName);

			var actual = await _wikiPages.Exists(pageName);
			Assert.IsFalse(actual);
		}

		[TestMethod]
		public async Task Exists_TrailingSlash_StillReturnsTrue()
		{
			string existingPage = "Exists";
			AddPage(existingPage);

			var actual = await _wikiPages.Exists("/" + existingPage + "/");
			Assert.AreEqual(1, _cache.PageCache.Count, "Cache should have 1 record");
			Assert.AreEqual(existingPage, _cache.PageCache.First().PageName, "Cache should match page checked");
			Assert.IsTrue(actual);
		}

		#endregion

		#region Page

		[TestMethod]
		public async Task Page_PageExists_ReturnsPage()
		{
			string existingPage = "Exists";
			AddPage(existingPage);

			var actual = await _wikiPages.Page(existingPage);
			Assert.AreEqual(1, _cache.PageCache.Count, "Cache should have  1 record");
			Assert.AreEqual(existingPage, _cache.PageCache.First().PageName, "Cache should match page checked");
			Assert.IsNotNull(actual);
			Assert.AreEqual(existingPage, actual.PageName);
		}

		[TestMethod]
		public async Task Page_PageDoesNotExist_ReturnsNull()
		{
			string existingPage = "Exists";
			AddPage(existingPage);

			var actual = await _wikiPages.Page("DoesNotExist");
			Assert.IsNull(actual);
		}

		[TestMethod]
		public async Task Page_PreviousRevision_ReturnsPage()
		{
			string existingPage = "Exists";
			_db.WikiPages.Add(new WikiPage { PageName = existingPage, Markup = "", Revision = 1, ChildId = 2 });
			_db.WikiPages.Add(new WikiPage { PageName = existingPage, Markup = "", Revision = 2, ChildId = null });
			_db.SaveChanges();

			var actual = await _wikiPages.Page(existingPage, 1);
			Assert.IsNotNull(actual);
		}

		[TestMethod]
		public async Task Page_OnlyDeletedExists_ReturnsNull()
		{
			string existingPage = "Exists";
			AddPage(existingPage, isDeleted: true);

			var actual = await _wikiPages.Page(existingPage);
			Assert.AreEqual(0, _cache.PageCache.Count, "Non-existent page was not cached.");
			Assert.IsNull(actual);
		}

		[TestMethod]
		public async Task Page_TrimsTrailingSlashes()
		{
			string existingPage = "Exists";
			AddPage(existingPage);

			var actual = await _wikiPages.Page("/" + existingPage + "/");
			Assert.IsNotNull(actual);
			Assert.AreEqual(existingPage, actual.PageName);
		}

		[TestMethod]
		public async Task Page_LatestRevisionIsDeleted_PreviousConsideredCurrent()
		{
			string pageName = "Page";
			var revision1 = new WikiPage { PageName = pageName, Revision = 1, IsDeleted = false, ChildId = null };
			var revision2 = new WikiPage { PageName = pageName, Revision = 2, IsDeleted = true, ChildId = null };
			_db.WikiPages.Add(revision1);
			_db.WikiPages.Add(revision2);
			_db.SaveChanges();
			_cache.PageCache.Add(revision1);

			var actual = await _wikiPages.Page(pageName);
			Assert.IsNotNull(actual);
			Assert.AreEqual(1, actual.Revision);
			Assert.IsNull(actual.ChildId);
			Assert.AreEqual(pageName, actual.PageName);
		}

		[TestMethod]
		public async Task Page_LatestTwoRevisionsDeleted_PreviousConsideredCurrent()
		{
			string pageName = "Page";
			var revision1 = new WikiPage { PageName = pageName, Revision = 1, IsDeleted = false, ChildId = null };
			var revision2 = new WikiPage { PageName = pageName, Revision = 2, IsDeleted = true, ChildId = null };
			var revision3 = new WikiPage { PageName = pageName, Revision = 3, IsDeleted = true, ChildId = null };
			_db.WikiPages.Add(revision1);
			_db.WikiPages.Add(revision2);
			_db.WikiPages.Add(revision3);
			_db.SaveChanges();
			_cache.PageCache.Add(revision1);

			var actual = await _wikiPages.Page(pageName);
			Assert.IsNotNull(actual);
			Assert.AreEqual(1, actual.Revision);
			Assert.IsNull(actual.ChildId);
			Assert.AreEqual(pageName, actual.PageName);
		}

		[TestMethod]
		public async Task Page_MultipleCurrent_PickMostRecent()
		{
			// This scenario should never happen, but if it does, we want to get the latest revision
			string page = "Duplicate";
			_db.WikiPages.Add(new WikiPage { PageName = page, Revision = 1, IsDeleted = false, ChildId = null });
			_db.WikiPages.Add(new WikiPage { PageName = page, Revision = 2, IsDeleted = false, ChildId = null });
			_db.SaveChanges();

			var actual = await _wikiPages.Page(page);
			Assert.IsNotNull(actual);
			Assert.AreEqual(2, actual.Revision);
		}

		[TestMethod]
		[DataRow(null)]
		[DataRow("")]
		[DataRow("\r \n \t")]
		public async Task Page_NoPageName_ReturnsNull(string pageName)
		{
			AddPage(pageName);

			var actual = await _wikiPages.Page(pageName);
			Assert.IsNull(actual);
		}

		#endregion

		#region Add

		[TestMethod]
		public async Task Add_NewPage()
		{
			string newPage = "New Page";
			string anotherPage = "AnotherPage";
			await _wikiPages.Add(new WikiPage { PageName = newPage, Markup = $"[{anotherPage}]" });

			Assert.AreEqual(1, _db.WikiPages.Count());
			Assert.AreEqual(newPage, _db.WikiPages.Single().PageName);
			Assert.AreEqual(1, _db.WikiPages.Single().Revision);
			Assert.IsNull(_db.WikiPages.Single().ChildId);

			Assert.AreEqual(1, _cache.PageCache.Count);
			Assert.AreEqual(newPage, _cache.PageCache.Single().PageName);
			Assert.AreEqual(1, _cache.PageCache.Single().Revision);
			Assert.IsNull(_cache.PageCache.Single().ChildId);

			Assert.AreEqual(1, _db.WikiReferrals.Count());
			Assert.AreEqual(anotherPage, _db.WikiReferrals.Single().Referral);
			Assert.AreEqual(newPage, _db.WikiReferrals.Single().Referrer);
		}

		[TestMethod]
		public async Task Add_RevisionToExistingPage()
		{
			string oldLink = "OldPage";
			string newLink = "NewLink";
			string existingPageName = "Existing Page";
			_db.WikiReferrals.Add(new WikiPageReferral { Excerpt = $"[{oldLink}]", Referral = oldLink, Referrer = existingPageName });
			var existingPage = new WikiPage { PageName = existingPageName, Markup = $"[{oldLink}]" };
			_db.WikiPages.Add(existingPage);
			_db.SaveChanges();
			_cache.PageCache.Add(existingPage);

			await _wikiPages.Add(new WikiPage { PageName = existingPageName, Markup = $"[{newLink}]" });

			Assert.AreEqual(2, _db.WikiPages.Count());
			var previous = _db.WikiPages.SingleOrDefault(wp => wp.PageName == existingPageName && wp.ChildId != null);
			var current = _db.WikiPages.SingleOrDefault(wp => wp.PageName == existingPageName && wp.ChildId == null);

			Assert.IsNotNull(previous);
			Assert.IsNotNull(current);
			Assert.AreEqual(1, previous.Revision);
			Assert.AreEqual(current.Id, previous.ChildId);
			Assert.AreEqual(2, current.Revision);
			Assert.IsNull(current.ChildId);

			Assert.AreEqual(1, _cache.PageCache.Count);
			Assert.AreEqual(current.Id, _cache.PageCache.Single().Id);

			Assert.AreEqual(1, _db.WikiReferrals.Count());
			Assert.AreEqual(existingPageName, _db.WikiReferrals.Single().Referrer);
			Assert.AreEqual(newLink, _db.WikiReferrals.Single().Referral);
		}

		[TestMethod]
		public async Task Add_RevisionToPageWithLatestRevisionDeleted()
		{
			// Revision 1 - Not deleted, no child id
			// Revision 2 - Deleted, no child id
			string pageName = "Page";
			string revision1Link = "Link1";
			string revision2Link = "Link2";
			string revision3Link = "Link3";
			var revision1 = new WikiPage { PageName = pageName, Revision = 1, IsDeleted = false, ChildId = null, Markup = $"[{revision1Link}]" };
			var revision2 = new WikiPage { PageName = pageName, Revision = 2, IsDeleted = true, ChildId = null, Markup = $"[{revision2Link}]" };
			_db.WikiPages.Add(revision1);
			_db.WikiPages.Add(revision2);
			_db.WikiReferrals.Add(new WikiPageReferral { Referrer = pageName, Referral = revision1Link });
			_db.SaveChanges();
			_cache.PageCache.Add(revision1);

			await _wikiPages.Add(new WikiPage { PageName = pageName, Markup = $"[{revision3Link}]" });

			Assert.AreEqual(3, _db.WikiPages.Count());

			var first = _db.WikiPages.OrderBy(wp => wp.Id).First();
			var latest = _db.WikiPages.OrderByDescending(wp => wp.Id).First();

			Assert.AreEqual(1, first.Revision);
			Assert.AreEqual(latest.Id, first.ChildId);
			Assert.AreEqual(3, latest.Revision);
			Assert.IsNull(latest.ChildId);

			Assert.AreEqual(1, _cache.PageCache.Count);
			Assert.AreEqual(3, _cache.PageCache.Single().Revision);

			Assert.AreEqual(1, _db.WikiReferrals.Count());
			Assert.AreEqual(pageName, _db.WikiReferrals.Single().Referrer);
			Assert.AreEqual(revision3Link, _db.WikiReferrals.Single().Referral);
		}

		[TestMethod]
		public async Task Add_RevisionToPageWithLatestTwoRevisionsDeleted()
		{
			// Revision 1 - Not deleted, no child id
			// Revision 2 - Deleted, no child id
			string pageName = "Page";
			string revision1Link = "Link1";
			string revision2Link = "Link2";
			string revision3Link = "Link3";
			string revision4Link = "Link4";
			var revision1 = new WikiPage { PageName = pageName, Revision = 1, IsDeleted = false, ChildId = null, Markup = $"[{revision1Link}]" };
			var revision2 = new WikiPage { PageName = pageName, Revision = 2, IsDeleted = true, ChildId = null, Markup = $"[{revision2Link}]" };
			var revision3 = new WikiPage { PageName = pageName, Revision = 3, IsDeleted = true, ChildId = null, Markup = $"[{revision3Link}]" };
			_db.WikiPages.Add(revision1);
			_db.WikiPages.Add(revision2);
			_db.WikiPages.Add(revision3);
			_db.WikiReferrals.Add(new WikiPageReferral { Referrer = pageName, Referral = revision1Link });
			_db.SaveChanges();
			_cache.PageCache.Add(revision1);

			await _wikiPages.Add(new WikiPage { PageName = pageName, Markup = $"[{revision4Link}]" });

			Assert.AreEqual(4, _db.WikiPages.Count());

			var first = _db.WikiPages.OrderBy(wp => wp.Id).First();
			var latest = _db.WikiPages.OrderByDescending(wp => wp.Id).First();

			Assert.AreEqual(1, first.Revision);
			Assert.AreEqual(latest.Id, first.ChildId);
			Assert.AreEqual(4, latest.Revision);
			Assert.IsNull(latest.ChildId);

			Assert.AreEqual(1, _cache.PageCache.Count);
			Assert.AreEqual(4, _cache.PageCache.Single().Revision);

			Assert.AreEqual(1, _db.WikiReferrals.Count());
			Assert.AreEqual(pageName, _db.WikiReferrals.Single().Referrer);
			Assert.AreEqual(revision4Link, _db.WikiReferrals.Single().Referral);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public async Task Add_Null_Throws()
		{
			await _wikiPages.Add(null);
		}

		[TestMethod]
		[DataRow(null)]
		[DataRow("")]
		[DataRow("\r \n \t")]
		[ExpectedException(typeof(InvalidOperationException))]
		public async Task Add_NoPageName_Throws(string pageName)
		{
			await _wikiPages.Add(new WikiPage { PageName = pageName });
		}

		#endregion

		#region Revision

		[TestMethod]
		public async Task Revision_Exists_ReturnsPage()
		{
			string existingPage = "Exists";
			AddPage(existingPage);
			AddPage(existingPage);
			var id = AddPage(existingPage);

			var actual = await _wikiPages.Revision(id);
			Assert.IsNotNull(actual);
			Assert.AreEqual(1, _cache.PageCache.Count);
			Assert.AreEqual(existingPage, actual.PageName);
		}

		[TestMethod]
		public async Task Revision_DoesNotExist_ReturnsNull()
		{
			var actual = await _wikiPages.Revision(int.MinValue);
			Assert.IsNull(actual);
		}

		[TestMethod]
		public async Task Revision_OldRevision_DoesNotAddToCache()
		{
			string existingPage = "InCache";
			var page1 = new WikiPage { Id = 1, PageName = existingPage, ChildId = 2 };
			var page2 = new WikiPage { Id = 2, PageName = existingPage };
			_db.WikiPages.Add(page1);
			_db.WikiPages.Add(page2);
			_db.SaveChanges();

			var actual = await _wikiPages.Revision(1);
			Assert.IsNotNull(actual);
			Assert.AreEqual(0, _cache.PageCache.Count);
		}

		#endregion

		#region Move

		[TestMethod]
		[DataRow(null)]
		[DataRow("")]
		[DataRow("\n \r \t")]
		[ExpectedException(typeof(ArgumentException))]
		public async Task Move_EmptyDestination_Throws(string destination)
		{
			await _wikiPages.Move("Test", destination);
		}

		[TestMethod]
		[ExpectedException(typeof(InvalidOperationException))]
		public async Task Move_DestinationExists_Throws()
		{
			string existingPage = "InCache";
			AddPage(existingPage);
			await _wikiPages.Move("Original Page", existingPage);
		}

		[TestMethod]
		public async Task Move_OriginalDoesNotExist_NothingHappens()
		{
			var actual = await _wikiPages.Move("Does not exist", "Also does not exist");
			Assert.IsTrue(actual, "Page not found is considered successful");
			Assert.AreEqual(0, _db.WikiPages.Count());
			Assert.AreEqual(0, _cache.PageCache.Count);
		}

		[TestMethod]
		public async Task Move_SingleRevision()
		{
			string existingPageName = "ExistingPage";
			string newPageName = "NewPageName";
			string link = "AnotherPage";
			var existingPage = new WikiPage { PageName = existingPageName, Markup = $"[{link}]" };
			_db.WikiPages.Add(existingPage);
			_db.WikiReferrals.Add(new WikiPageReferral { Referrer = existingPageName, Referral = link });
			_db.SaveChanges();
			_cache.PageCache.Add(existingPage);

			var actual = await _wikiPages.Move(existingPageName, newPageName);
			Assert.IsTrue(actual);
			Assert.AreEqual(1, _db.WikiPages.Count());
			Assert.AreEqual(newPageName, _db.WikiPages.Single().PageName);
			Assert.AreEqual(1, _cache.PageCache.Count);
			Assert.AreEqual(newPageName, _cache.PageCache.Single().PageName);

			Assert.AreEqual(1, _db.WikiReferrals.Count());
			Assert.AreEqual(newPageName, _db.WikiReferrals.Single().Referrer);
			Assert.AreEqual(link, _db.WikiReferrals.Single().Referral);
		}

		[TestMethod]
		public async Task Move_MultipleRevisions()
		{
			string existingPageName = "ExistingPage";
			string newPageName = "NewPageName";
			var previousRevision = new WikiPage { Id = 1, PageName = existingPageName, ChildId = 2 };
			var existingPage = new WikiPage { Id = 2, PageName = existingPageName, ChildId = null };
			_db.WikiPages.Add(previousRevision);
			_db.WikiPages.Add(existingPage);
			_cache.PageCache.Add(existingPage);
			_db.SaveChanges();

			var actual = await _wikiPages.Move(existingPageName, newPageName);
			Assert.IsTrue(actual);
			Assert.AreEqual(2, _db.WikiPages.Count());
			Assert.IsTrue(_db.WikiPages.All(wp => wp.PageName == newPageName));

			Assert.AreEqual(1, _cache.PageCache.Count);
			Assert.AreEqual(newPageName, _cache.PageCache.Single().PageName);
		}

		[TestMethod]
		public async Task Move_UpdateException_DoesNotMove()
		{
			string origPageName = "Orig";
			string origLink = "Link";
			var origPage = new WikiPage { PageName = origPageName, Markup = $"[{origLink}]" };
			
			_db.WikiPages.Add(origPage);
			_db.WikiReferrals.Add(new WikiPageReferral { Referrer = origPageName, Referral = origLink });
			_db.SaveChanges();
			_cache.Set(origPageName, origPage);

			string destPageName = "Dest";

			_db.CreateUpdateConflict();

			var actual = await _wikiPages.Move(origPageName, destPageName);
			Assert.IsFalse(actual, "The move was unsuccessful");
			
			// Moved page does not exist
			Assert.AreEqual(0, _db.WikiPages.Count(wp => wp.PageName == destPageName));

			// Cache does not have the moved page
			Assert.AreEqual(0, _cache.PageCache.Count(wp => wp.PageName == destPageName));

			// Referrers not updated
			Assert.AreEqual(0, _db.WikiReferrals.Count(wr => wr.Referrer == destPageName));
		}

		[TestMethod]
		public async Task Move_ConcurrencyException_DoesNotMove()
		{
			string origPageName = "Orig";
			string origLink = "Link";
			var origPage = new WikiPage { PageName = origPageName, Markup = $"[{origLink}]" };
			
			_db.WikiPages.Add(origPage);
			_db.WikiReferrals.Add(new WikiPageReferral { Referrer = origPageName, Referral = origLink });
			_db.SaveChanges();
			_cache.Set(origPageName, origPage);

			string destPageName = "Dest";

			_db.CreateConcurrentUpdateConflict();

			var actual = await _wikiPages.Move(origPageName, destPageName);
			Assert.IsFalse(actual, "The move was unsuccessful");
			
			// Moved page does not exist
			Assert.AreEqual(0, _db.WikiPages.Count(wp => wp.PageName == destPageName));

			// Cache does not have the moved page
			Assert.AreEqual(0, _cache.PageCache.Count(wp => wp.PageName == destPageName));

			// Referrers not updated
			Assert.AreEqual(0, _db.WikiReferrals.Count(wr => wr.Referrer == destPageName));
		}

		[TestMethod]
		public async Task Move_DestinationPage_TrimsSlashes()
		{
			string existingPageName = "ExistingPage";
			string newPageName = "NewPageName";
			string link = "AnotherPage";
			var existingPage = new WikiPage { PageName = existingPageName, Markup = $"[{link}]" };
			_db.WikiPages.Add(existingPage);
			_db.WikiReferrals.Add(new WikiPageReferral { Referrer = existingPageName, Referral = link });
			_db.SaveChanges();
			_cache.PageCache.Add(existingPage);

			var actual = await _wikiPages.Move(existingPageName, "/" + newPageName + "/");
			Assert.IsTrue(actual);
			Assert.AreEqual(1, _db.WikiPages.Count());
			Assert.AreEqual(newPageName, _db.WikiPages.Single().PageName);
			Assert.AreEqual(1, _cache.PageCache.Count);
			Assert.AreEqual(newPageName, _cache.PageCache.Single().PageName);

			Assert.AreEqual(1, _db.WikiReferrals.Count());
			Assert.AreEqual(newPageName, _db.WikiReferrals.Single().Referrer);
			Assert.AreEqual(link, _db.WikiReferrals.Single().Referral);
		}

		[TestMethod]
		public async Task Move_OriginalPage_TrimsSlashes()
		{
			string existingPageName = "ExistingPage";
			string newPageName = "NewPageName";
			string link = "AnotherPage";
			var existingPage = new WikiPage { PageName = existingPageName, Markup = $"[{link}]" };
			_db.WikiPages.Add(existingPage);
			_db.WikiReferrals.Add(new WikiPageReferral { Referrer = existingPageName, Referral = link });
			_db.SaveChanges();
			_cache.PageCache.Add(existingPage);

			var actual = await _wikiPages.Move("/" + existingPageName + "/", newPageName);
			Assert.IsTrue(actual);
			Assert.AreEqual(1, _db.WikiPages.Count());
			Assert.AreEqual(newPageName, _db.WikiPages.Single().PageName);
			Assert.AreEqual(1, _cache.PageCache.Count);
			Assert.AreEqual(newPageName, _cache.PageCache.Single().PageName);

			Assert.AreEqual(1, _db.WikiReferrals.Count());
			Assert.AreEqual(newPageName, _db.WikiReferrals.Single().Referrer);
			Assert.AreEqual(link, _db.WikiReferrals.Single().Referral);
		}

		#endregion

		#region Delete Page

		[TestMethod]
		public async Task DeletePage_PageDoesNotExist_NothingHappens()
		{
			string doesNotExist = "DoesNotExist";
			await _wikiPages.Delete(doesNotExist);

			Assert.AreEqual(0, _db.WikiPages.Count());
			Assert.AreEqual(0, _cache.PageCache.Count);
		}

		[TestMethod]
		public async Task DeletePage_1Revision_RevisionDeleted()
		{
			string pageName = "Exists";
			string link = "AnotherPage";
			var existingPage = new WikiPage { PageName = pageName, Markup = $"[{link}]" };
			_db.WikiPages.Add(existingPage);
			_db.WikiReferrals.Add(new WikiPageReferral { Referrer = pageName, Referral = link });
			_db.SaveChanges();
			_cache.PageCache.Add(existingPage);

			var actual = await _wikiPages.Delete(pageName);

			Assert.AreEqual(1, actual);
			Assert.AreEqual(1, _db.WikiPages.Count());
			Assert.IsTrue(_db.WikiPages.Single().IsDeleted);
			Assert.IsNull(_db.WikiPages.Single().ChildId);
			Assert.AreEqual(0, _cache.PageCache.Count);
			Assert.AreEqual(0, _db.WikiReferrals.Count());
		}

		[TestMethod]
		public async Task DeletePage_2Revisions_AllRevisionsDeleted()
		{
			string pageName = "Exists";
			string link = "AnotherPage";
			var revision1 = new WikiPage { PageName = pageName, Revision = 1 };
			var revision2 = new WikiPage { PageName = pageName, Revision = 2, Markup = $"[{link}]" };
			
			_db.WikiPages.Add(revision1);
			_db.WikiPages.Add(revision2);
			_db.SaveChanges();
			revision1.ChildId = revision2.Id;
			_db.WikiReferrals.Add(new WikiPageReferral { Referrer = pageName, Referral = link });
			_db.SaveChanges();
			_cache.PageCache.Add(revision2);

			var actual = await _wikiPages.Delete(pageName);

			Assert.AreEqual(2, actual);
			Assert.AreEqual(2, _db.WikiPages.Count());
			Assert.IsTrue(_db.WikiPages.All(wp => wp.IsDeleted));
			Assert.IsTrue(_db.WikiPages.All(wp => wp.ChildId == null));
			Assert.AreEqual(0, _cache.PageCache.Count);
			Assert.AreEqual(0, _db.WikiReferrals.Count());
		}

		[TestMethod]
		public async Task DeletePage_ConcurrencyConflict_DoesNotDelete()
		{
			string pageName = "Exists";
			string link = "AnotherPage";
			var existingPage = new WikiPage { PageName = pageName, Markup = $"[{link}]" };
			_db.WikiPages.Add(existingPage);
			_db.WikiReferrals.Add(new WikiPageReferral { Referrer = pageName, Referral = link });
			_db.SaveChanges();
			_cache.PageCache.Add(existingPage);

			_db.CreateConcurrentUpdateConflict();

			var actual = await _wikiPages.Delete(pageName);

			Assert.AreEqual(-1, actual);
			Assert.AreEqual(0, _db.WikiPages.ThatAreDeleted().Count());
			Assert.AreEqual(1, _cache.PageCache.Count);
			Assert.AreEqual(1, _db.WikiReferrals.Count());
		}

		[TestMethod]
		public async Task DeletePage_TrimsSlashes()
		{
			string pageName = "Exists";
			string link = "AnotherPage";
			var existingPage = new WikiPage { PageName = pageName, Markup = $"[{link}]" };
			_db.WikiPages.Add(existingPage);
			_db.WikiReferrals.Add(new WikiPageReferral { Referrer = pageName, Referral = link });
			_db.SaveChanges();
			_cache.PageCache.Add(existingPage);

			var actual = await _wikiPages.Delete("/" + pageName + "/");

			Assert.AreEqual(1, actual);
			Assert.AreEqual(1, _db.WikiPages.Count());
			Assert.IsTrue(_db.WikiPages.Single().IsDeleted);
			Assert.IsNull(_db.WikiPages.Single().ChildId);
			Assert.AreEqual(0, _cache.PageCache.Count);
			Assert.AreEqual(0, _db.WikiReferrals.Count());
		}

		#endregion

		#region Delete Revision

		[TestMethod]
		public async Task DeleteRevision_PreviousRevision_DeletesOnlyThatRevision()
		{
			string existingPageName = "Exists";
			var currentRevision = new WikiPage { PageName = existingPageName, Revision = 2, ChildId = null };
			var previousRevision = new WikiPage { PageName = existingPageName, Revision = 1, Child = currentRevision };
			_db.WikiPages.Add(previousRevision);
			_db.WikiPages.Add(currentRevision);
			_db.SaveChanges();
			_cache.PageCache.Add(currentRevision);
			
			await _wikiPages.Delete(existingPageName, 1);

			Assert.AreEqual(1, _db.WikiPages.ThatAreNotDeleted().Count());
			Assert.AreEqual(existingPageName, _db.WikiPages.ThatAreNotDeleted().Single().PageName);
			Assert.AreEqual(1, _db.WikiPages.ThatAreDeleted().Single().Revision);
			Assert.AreEqual(2, _db.WikiPages.ThatAreNotDeleted().Single().Revision);

			Assert.AreEqual(1, _cache.PageCache.Count);
			Assert.AreEqual(existingPageName, _cache.PageCache.Single().PageName);
			Assert.AreEqual(2, _cache.PageCache.Single().Revision);
		}

		[TestMethod]
		public async Task DeleteRevision_DoesNotExist_NothingHappens()
		{
			string pageName = "Exists";
			AddPage(pageName, cache: true);

			await _wikiPages.Delete(pageName, int.MaxValue);

			Assert.AreEqual(1, _db.WikiPages.ThatAreNotDeleted().Count());
			Assert.AreEqual(1, _cache.PageCache.Count);
		}

		[TestMethod]
		public async Task DeleteRevision_AlreadyDeleted_NothingHappens()
		{
			string pageName = "Exists";
			AddPage(pageName, isDeleted: true);

			await _wikiPages.Delete(pageName, 1);

			Assert.AreEqual(0, _db.WikiPages.ThatAreNotDeleted().Count());
			Assert.AreEqual(1, _db.WikiPages.ThatAreDeleted().Count());
			Assert.AreEqual(0, _cache.PageCache.Count);
		}

		[TestMethod]
		public async Task DeleteRevision_DeletingCurrent_SetsPreviousToCurrent()
		{
			string existingPageName = "Exists";
			string oldLink = "OldPage";
			string newLink = "NewPage";
			var currentRevision = new WikiPage { PageName = existingPageName, Revision = 2, ChildId = null, Markup = $"[{newLink}]" };
			var previousRevision = new WikiPage { PageName = existingPageName, Revision = 1, Child = currentRevision, Markup = $"[{oldLink}]" };
			_db.WikiPages.Add(previousRevision);
			_db.WikiPages.Add(currentRevision);
			_db.SaveChanges();
			_cache.PageCache.Add(currentRevision);

			await _wikiPages.Delete(existingPageName, 2);

			// Revision 1 should be Current
			Assert.AreEqual(1, _db.WikiPages.ThatAreNotDeleted().Count());
			Assert.AreEqual(1, _db.WikiPages.ThatAreDeleted().Count());
			var current = _db.WikiPages
				.ThatAreNotDeleted()
				.WithNoChildren()
				.Single();

			Assert.AreEqual(existingPageName, current.PageName);
			Assert.AreEqual(1, current.Revision);
			Assert.IsNull(current.ChildId);
			Assert.IsFalse(current.IsDeleted);

			// Revision 2 should be deleted
			Assert.AreEqual(1, _db.WikiPages.ThatAreDeleted().Count());
			var deleted = _db.WikiPages.ThatAreDeleted().Single();
			Assert.AreEqual(2, deleted.Revision);
			Assert.IsNull(deleted.ChildId);

			// Revision 1 should be in cache
			Assert.AreEqual(1, _cache.PageCache.Count);
			Assert.AreEqual(1, _cache.PageCache.Single().Revision);

			// Referrers should be based on Revision 1
			Assert.AreEqual(1, _db.WikiReferrals.Count());
			var referrer = _db.WikiReferrals.Single();
			Assert.AreEqual(oldLink, referrer.Referral);
		}

		[TestMethod]
		public async Task DeleteRevision_DeleteCurrentWhenAlreadyALaterDeletedRevision()
		{
			string pageName = "Exists";
			string revision1Link = "Link1";
			string revision2Link = "Link2";
			string revision3Link = "Link3";
			var revision1 = new WikiPage { PageName = pageName, Revision = 1, Markup = $"[{revision1Link}]" };
			var revision2 = new WikiPage { PageName = pageName, Revision = 2, Markup = $"[{revision2Link}]" };
			var revision3 = new WikiPage { PageName = pageName, Revision = 3, IsDeleted = true, Markup = $"[{revision3Link}]" };
			_db.WikiPages.Add(revision1);
			_db.WikiPages.Add(revision2);
			_db.WikiPages.Add(revision3);
			_db.SaveChanges();
			revision1.ChildId = revision2.Id;
			_db.SaveChanges();
			_cache.PageCache.Add(revision2);

			await _wikiPages.Delete(pageName, 2);

			// Revision 1 should be Current
			Assert.AreEqual(1, _db.WikiPages.ThatAreNotDeleted().Count());
			Assert.AreEqual(2, _db.WikiPages.ThatAreDeleted().Count());
			var current = _db.WikiPages
				.ThatAreNotDeleted()
				.WithNoChildren()
				.Single();

			Assert.AreEqual(pageName, current.PageName);
			Assert.AreEqual(1, current.Revision);
			Assert.IsNull(current.ChildId);
			Assert.IsFalse(current.IsDeleted);

			// Revision 2 should be deleted
			Assert.AreEqual(2, _db.WikiPages.ThatAreDeleted().Count());
			var deleted2 = _db.WikiPages.Single(wp => wp.Revision == 2);
			Assert.AreEqual(2, deleted2.Revision);
			Assert.IsNull(deleted2.ChildId);

			// Revision 3 should be deleted
			var deleted3 = _db.WikiPages.Single(wp => wp.Revision == 3);
			Assert.AreEqual(3, deleted3.Revision);
			Assert.IsNull(deleted3.ChildId);

			// Revision 1 should be in cache
			Assert.AreEqual(1, _cache.PageCache.Count);
			Assert.AreEqual(1, _cache.PageCache.Single().Revision);

			// Referrers should be based on Revision 1
			Assert.AreEqual(1, _db.WikiReferrals.Count());
			var referrer = _db.WikiReferrals.Single();
			Assert.AreEqual(revision1Link, referrer.Referral);
		}

		[TestMethod]
		public async Task DeleteRevision_DeleteCurrentWhenPreviousRevisionAlreadyDeletedRevision()
		{
			string pageName = "Exists";
			string revision1Link = "Link1";
			string revision2Link = "Link2";
			string revision3Link = "Link3";
			var revision1 = new WikiPage { PageName = pageName, Revision = 1, Markup = $"[{revision1Link}]" };
			var revision2 = new WikiPage { PageName = pageName, Revision = 2, IsDeleted = true, Markup = $"[{revision2Link}]" };
			var revision3 = new WikiPage { PageName = pageName, Revision = 3, Markup = $"[{revision3Link}]" };
			_db.WikiPages.Add(revision1);
			_db.WikiPages.Add(revision2);
			_db.WikiPages.Add(revision3);
			_db.SaveChanges();
			revision1.ChildId = revision3.Id;
			_db.SaveChanges();
			_cache.PageCache.Add(revision3);

			await _wikiPages.Delete(pageName, 3);

			// Revision 1 should be Current
			Assert.AreEqual(1, _db.WikiPages.ThatAreNotDeleted().Count());
			Assert.AreEqual(2, _db.WikiPages.ThatAreDeleted().Count());
			var current = _db.WikiPages
				.ThatAreNotDeleted()
				.WithNoChildren()
				.Single();

			Assert.AreEqual(pageName, current.PageName);
			Assert.AreEqual(1, current.Revision);

			// Revision 2 should be deleted
			Assert.AreEqual(2, _db.WikiPages.ThatAreDeleted().Count());
			var deleted2 = _db.WikiPages.Single(wp => wp.Revision == 2);
			Assert.AreEqual(2, deleted2.Revision);
			Assert.IsNull(deleted2.ChildId);

			// Revision 3 should be deleted
			var deleted3 = _db.WikiPages.Single(wp => wp.Revision == 3);
			Assert.AreEqual(3, deleted3.Revision);
			Assert.IsNull(deleted3.ChildId);

			// Revision 1 should be in cache
			Assert.AreEqual(1, _cache.PageCache.Count);
			Assert.AreEqual(1, _cache.PageCache.Single().Revision);

			// Referrers should be based on Revision 1
			Assert.AreEqual(1, _db.WikiReferrals.Count());
			var referrer = _db.WikiReferrals.Single();
			Assert.AreEqual(revision1Link, referrer.Referral);
		}

		[TestMethod]
		public async Task DeleteRevision_TrimsSlashes()
		{
			string existingPageName = "Exists";
			var currentRevision = new WikiPage { PageName = existingPageName, Revision = 2, ChildId = null };
			var previousRevision = new WikiPage { PageName = existingPageName, Revision = 1, Child = currentRevision };
			_db.WikiPages.Add(previousRevision);
			_db.WikiPages.Add(currentRevision);
			_db.SaveChanges();
			_cache.PageCache.Add(currentRevision);
			
			await _wikiPages.Delete("/" + existingPageName + "/", 1);

			Assert.AreEqual(1, _db.WikiPages.ThatAreNotDeleted().Count());
			Assert.AreEqual(existingPageName, _db.WikiPages.ThatAreNotDeleted().Single().PageName);
			Assert.AreEqual(1, _db.WikiPages.ThatAreDeleted().Single().Revision);
			Assert.AreEqual(2, _db.WikiPages.ThatAreNotDeleted().Single().Revision);

			Assert.AreEqual(1, _cache.PageCache.Count);
			Assert.AreEqual(existingPageName, _cache.PageCache.Single().PageName);
			Assert.AreEqual(2, _cache.PageCache.Single().Revision);
		}

		#endregion

		#region Undelete

		[TestMethod]
		public async Task Undelete_PageDoesNotExist_NothingHappens()
		{
			var actual = await _wikiPages.Undelete("Does not exist");
			Assert.IsTrue(actual, "Page does not exist is considered successful");
			Assert.AreEqual(0, _db.WikiPages.Count());
			Assert.AreEqual(0, _cache.PageCache.Count);
		}

		[TestMethod]
		public async Task Undelete_ExistingPageThatIsNotDeleted_NothingHappens()
		{
			string pageName = "Exists";
			AddPage(pageName, isDeleted: false, cache: true);

			var actual = await _wikiPages.Undelete(pageName);
			Assert.IsTrue(actual, "Page already exists considered successful");
			Assert.AreEqual(1, _db.WikiPages.Count());
			Assert.AreEqual(1, _cache.PageCache.Count);
			Assert.IsFalse(_db.WikiPages.Single().IsDeleted);
		}

		[TestMethod]
		public async Task Undelete_DeletedPage_UndeletesPage()
		{
			string pageName = "Deleted";
			string link = "AnotherPage";
			var page = new WikiPage { PageName = pageName, Markup = $"[{link}]", IsDeleted = true };
			_db.WikiPages.Add(page);
			_db.SaveChanges();
			_cache.PageCache.Clear();

			var actual = await _wikiPages.Undelete(pageName);
			Assert.IsTrue(actual);
			Assert.AreEqual(1, _db.WikiPages.ThatAreNotDeleted().Count());
			Assert.AreEqual(1, _cache.PageCache.Count);
			Assert.AreEqual(1, _db.WikiReferrals.Count());
			Assert.AreEqual(pageName, _db.WikiReferrals.Single().Referrer);
			Assert.AreEqual(link, _db.WikiReferrals.Single().Referral);
		}

		[TestMethod]
		public async Task Undelete_OnlyLatestIsDeleted_SetsLatestToCurrent()
		{
			string pageName = "Exists";
			string oldLink = "OldLink";
			string newLink = "NewLink";
			var revision1 = new WikiPage { PageName = pageName, Revision = 1, Markup = $"[{oldLink}]" };
			var revision2 = new WikiPage { PageName = pageName, Revision = 2, Markup = $"[{newLink}]", IsDeleted = true };
			_db.WikiPages.Add(revision1);
			_db.WikiPages.Add(revision2);
			_db.WikiReferrals.Add(new WikiPageReferral { Referrer = pageName, Referral = oldLink });
			_db.SaveChanges();
			_cache.PageCache.Add(revision1);

			var actual = await _wikiPages.Undelete(pageName);
			Assert.IsTrue(actual);

			// Both are not deleted
			Assert.AreEqual(2, _db.WikiPages.ThatAreNotDeleted().Count());

			var newRevision1 = _db.WikiPages.Single(wp => wp.Revision == 1);
			var newRevision2 = _db.WikiPages.Single(wp => wp.Revision == 2);

			// Revision 1 is no longer current
			Assert.AreEqual(newRevision2.Id, newRevision1.ChildId);
			Assert.AreEqual(pageName, newRevision1.PageName);

			// Revision 2 is current
			Assert.AreEqual(pageName, newRevision2.PageName);
			Assert.IsNull(newRevision2.ChildId);
			
			// Revision 2 is in cache
			Assert.AreEqual(1, _cache.PageCache.Count);
			var cached = _cache.PageCache.Single();
			Assert.AreEqual(pageName, cached.PageName);
			Assert.AreEqual(2, cached.Revision);
			Assert.IsFalse(cached.IsDeleted);

			// Referrals are for revision 2
			Assert.AreEqual(1, _db.WikiReferrals.Count());
			var referral = _db.WikiReferrals.Single();
			Assert.AreEqual(pageName, referral.Referrer);
			Assert.AreEqual(newLink, referral.Referral);
		}

		[TestMethod]
		public async Task Undelete_Last2Deleted_SetsLatestToCurrent()
		{
			string pageName = "Exists";
			string link1 = "Link1";
			string link2 = "Link2";
			string link3 = "Link3";
			var revision1 = new WikiPage { PageName = pageName, Revision = 1, Markup = $"[{link1}]" };
			var revision2 = new WikiPage { PageName = pageName, Revision = 2, Markup = $"[{link2}]", IsDeleted = true };
			var revision3 = new WikiPage { PageName = pageName, Revision = 3, Markup = $"[{link3}]", IsDeleted = true };
			_db.WikiPages.Add(revision1);
			_db.WikiPages.Add(revision2);
			_db.WikiPages.Add(revision3);
			_db.WikiReferrals.Add(new WikiPageReferral { Referrer = pageName, Referral = link1 });
			_db.SaveChanges();
			_cache.PageCache.Add(revision1);

			var actual = await _wikiPages.Undelete(pageName);
			Assert.IsTrue(actual);

			// All not deleted
			Assert.AreEqual(3, _db.WikiPages.ThatAreNotDeleted().Count());

			var newRevision1 = _db.WikiPages.Single(wp => wp.Revision == 1);
			var newRevision2 = _db.WikiPages.Single(wp => wp.Revision == 2);
			var newRevision3 = _db.WikiPages.Single(wp => wp.Revision == 3);

			// Revision 1 is no longer current
			Assert.AreEqual(pageName, newRevision1.PageName);
			Assert.AreEqual(newRevision2.Id, newRevision1.ChildId);

			// Revision 2 is not current
			Assert.AreEqual(pageName, newRevision2.PageName);
			Assert.AreEqual(newRevision3.Id, newRevision2.ChildId);

			// Revision 3 is current
			Assert.AreEqual(pageName, newRevision3.PageName);
			Assert.IsNull(newRevision3.ChildId);
			
			// Revision 3 is in cache
			Assert.AreEqual(1, _cache.PageCache.Count);
			var cached = _cache.PageCache.Single();
			Assert.AreEqual(pageName, cached.PageName);
			Assert.AreEqual(3, cached.Revision);
			Assert.IsFalse(cached.IsDeleted);

			// Referrals are for revision 3
			Assert.AreEqual(1, _db.WikiReferrals.Count());
			var referral = _db.WikiReferrals.Single();
			Assert.AreEqual(pageName, referral.Referrer);
			Assert.AreEqual(link3, referral.Referral);
		}

		[TestMethod]
		public async Task Undelete_2DeletedRevisions_BothUndeleted()
		{
			string pageName = "Deleted";
			string link1 = "AnotherPage";
			string link2 = "YetAnotherPage";
			var revision1 = new WikiPage { PageName = pageName, Revision = 1, Markup = $"[{link1}]", IsDeleted = true };
			var revision2 = new WikiPage { PageName = pageName, Revision = 2, Markup = $"[{link2}]", IsDeleted = true };
			_db.WikiPages.Add(revision1);
			_db.WikiPages.Add(revision2);
			_db.SaveChanges();
			_cache.PageCache.Clear();

			var actual = await _wikiPages.Undelete(pageName);
			Assert.IsTrue(actual);

			// Both are not deleted
			Assert.AreEqual(2, _db.WikiPages.ThatAreNotDeleted().Count());

			var newRevision1 = _db.WikiPages.Single(wp => wp.Revision == 1);
			var newRevision2 = _db.WikiPages.Single(wp => wp.Revision == 2);

			// Revision 1 is not current
			Assert.AreEqual(newRevision2.Id, newRevision1.ChildId);
			Assert.AreEqual(pageName, newRevision1.PageName);

			// Revision 2 is current
			Assert.AreEqual(pageName, newRevision2.PageName);
			Assert.IsNull(newRevision2.ChildId);
			
			// Revision 2 is in cache
			Assert.AreEqual(1, _cache.PageCache.Count);
			var cached = _cache.PageCache.Single();
			Assert.AreEqual(pageName, cached.PageName);
			Assert.AreEqual(2, cached.Revision);
			Assert.IsFalse(cached.IsDeleted);

			// Referrals are for revision 2
			Assert.AreEqual(1, _db.WikiReferrals.Count());
			var referral = _db.WikiReferrals.Single();
			Assert.AreEqual(pageName, referral.Referrer);
			Assert.AreEqual(link2, referral.Referral);
		}

		[TestMethod]
		public async Task Undelete_ConcurrencyConflict_DoesNotUndelete()
		{
			string pageName = "Deleted";
			string link = "AnotherPage";
			var page = new WikiPage { PageName = pageName, Markup = $"[{link}]", IsDeleted = true };
			_db.WikiPages.Add(page);
			_db.SaveChanges();
			_cache.PageCache.Clear();
			_db.CreateConcurrentUpdateConflict();

			var actual = await _wikiPages.Undelete(pageName);

			Assert.IsFalse(actual);
			Assert.AreEqual(0, _db.WikiPages.ThatAreNotDeleted().Count());
			Assert.AreEqual(0, _cache.PageCache.Count);
			Assert.AreEqual(0, _db.WikiReferrals.Count());
		}

		[TestMethod]
		public async Task Undelete_TrimsSlashes()
		{
			string pageName = "Deleted";
			string link = "AnotherPage";
			var page = new WikiPage { PageName = pageName, Markup = $"[{link}]", IsDeleted = true };
			_db.WikiPages.Add(page);
			_db.SaveChanges();
			_cache.PageCache.Clear();

			var actual = await _wikiPages.Undelete("/" + pageName + "/");
			Assert.IsTrue(actual);
			Assert.AreEqual(1, _db.WikiPages.ThatAreNotDeleted().Count());
			Assert.AreEqual(1, _cache.PageCache.Count);
			Assert.AreEqual(1, _db.WikiReferrals.Count());
			Assert.AreEqual(pageName, _db.WikiReferrals.Single().Referrer);
			Assert.AreEqual(link, _db.WikiReferrals.Single().Referral);
		}

		#endregion

		#region SystemPage

		[TestMethod]
		[DataRow(null)]
		[DataRow("")]
		[DataRow("/")]
		public async Task SystemPage_EmptyChecks(string pageName)
		{
			var actual = await _wikiPages.SystemPage(pageName);
			Assert.IsNull(actual);
		}

		[TestMethod]
		public async Task SystemPage_Exists_ReturnsPage()
		{
			var suffix = "Exists";
			var systemPageName = "System/" + suffix;
			var page = new WikiPage { PageName = systemPageName };
			_db.WikiPages.Add(page);
			_db.SaveChanges();

			var actual = await _wikiPages.SystemPage(suffix);
			Assert.IsNotNull(actual);
			Assert.AreEqual(systemPageName, actual.PageName);
		}

		[TestMethod]
		public async Task SystemPage_DoesNotExists_ReturnsNull()
		{
			var suffix = "Exists";
			var systemPageName = "System/" + suffix;
			var page = new WikiPage { PageName = systemPageName };
			_db.WikiPages.Add(page);
			_db.SaveChanges();

			var actual = await _wikiPages.SystemPage("Does not exist");
			Assert.IsNull(actual);
		}

		[TestMethod]
		public async Task SystemPage_Empty_ReturnsSystem()
		{
			var page = new WikiPage { PageName = "System" };
			_db.WikiPages.Add(page);
			_db.SaveChanges();

			var actual = await _wikiPages.SystemPage("");
			Assert.IsNotNull(actual);
		}

		#endregion

		#region Orphans

		[TestMethod]
		public async Task Orphans_NoPages_ReturnsEmptyList()
		{
			var actual = await _wikiPages.Orphans();
			Assert.IsNotNull(actual);
			Assert.AreEqual(0, actual.Count());
		}

		[TestMethod]
		public async Task Orphans_DeletedPage_NotAnOrphan()
		{
			AddPage("Deleted", true);
			var actual = await _wikiPages.Orphans();

			Assert.IsNotNull(actual);
			Assert.AreEqual(0, actual.Count());
		}

		[TestMethod]
		public async Task Orphans_NoOrphans_ReturnsEmptyList()
		{
			// Two pages, that properly link each other
			string parent = "Parent";
			string child = "Child";
			AddPage(parent);
			AddPage(child);
			_db.WikiReferrals.Add(new WikiPageReferral { Referrer = parent, Referral = child });
			_db.WikiReferrals.Add(new WikiPageReferral { Referrer = child, Referral = parent });
			_db.SaveChanges();

			var actual = await _wikiPages.Orphans();

			Assert.IsNotNull(actual);
			Assert.AreEqual(0, actual.Count());
		}

		[TestMethod]
		public async Task Orphans_PageWithNoReferrers_ReturnsAsOrphan()
		{
			string orphan = "Orphan";
			AddPage(orphan);

			var actual = await _wikiPages.Orphans();

			Assert.IsNotNull(actual);
			var orphans = actual.ToList();
			Assert.AreEqual(1, orphans.Count);
			Assert.AreEqual(orphan, orphans.Single().PageName);
		}

		[TestMethod]
		public async Task Orphans_ReferrersExistButNotForPage_ReturnsAsOrphan()
		{
			string orphan = "Orphan";
			AddPage(orphan);
			_db.WikiReferrals.Add(new WikiPageReferral { Referrer = "Parent", Referral = "Not" + orphan });
			_db.SaveChanges();

			var actual = await _wikiPages.Orphans();

			Assert.IsNotNull(actual);
			var orphans = actual.ToList();
			Assert.AreEqual(1, orphans.Count);
			Assert.AreEqual(orphan, orphans.Single().PageName);
		}

		[TestMethod]
		public async Task Orphans_Subpages_NotConsideredOrphans()
		{
			string parent = "Parent";
			AddPage(parent);
			AddPage(parent  + "/Child");

			var actual = await _wikiPages.Orphans();

			// Parent should be an orphan but not child
			Assert.IsNotNull(actual);
			var orphans = actual.ToList();
			Assert.AreEqual(1, orphans.Count);
			Assert.AreEqual(parent, orphans.Single().PageName);
		}

		[TestMethod]
		[DataRow("MediaPosts")]
		[DataRow("System")]
		[DataRow("InternalSystem")]
		public async Task Orphans_CorePages_NotConsideredOrphans(string page)
		{
			AddPage(page);
			var actual = await _wikiPages.Orphans();

			Assert.IsNotNull(actual);
			Assert.AreEqual(0, actual.Count());
		}

		#endregion

		#region Broken Links

		[TestMethod]
		public async Task BrokenLinks_NoReferrers_ReturnsEmptyList()
		{
			var actual = await _wikiPages.BrokenLinks();
			Assert.IsNotNull(actual);
			Assert.AreEqual(0, actual.Count());
		}

		[TestMethod]
		public async Task BrokenLinks_NoBrokenLinks_ReturnsEmptyList()
		{
			string page = "Parent";
			AddPage(page);
			_db.WikiReferrals.Add(new WikiPageReferral { Referral = "Parent", Referrer = "AnotherPage" });
			_db.SaveChanges();

			var actual = await _wikiPages.BrokenLinks();

			Assert.IsNotNull(actual);
			Assert.AreEqual(0, actual.Count());
		}

		[TestMethod]
		public async Task BrokenLinks_BrokenLink_ReturnsBrokenLink()
		{
			string page = "PageWithLink";
			string doesNotExist = "DoesNotExist";
			AddPage(page);
			_db.WikiReferrals.Add(new WikiPageReferral
			{
				Referrer = page,
				Referral = doesNotExist,
			});
			_db.SaveChanges();

			var actual = await _wikiPages.BrokenLinks();

			Assert.IsNotNull(actual);
			var brokenLinks = actual.ToList();
			Assert.AreEqual(1, brokenLinks.Count);
			var brokenLink = brokenLinks.Single();
			Assert.AreEqual(page, brokenLink.Referrer);
			Assert.AreEqual(doesNotExist, brokenLink.Referral);
		}

		[TestMethod]
		[DataRow("FrontPage")]
		[DataRow("Players-List")]
		[DataRow("Subs-")]
		[DataRow("Movies-")]
		[DataRow("/forum")]
		[DataRow("/userfiles")]
		public async Task BrokenLinks_CorePages_NotConsideredBrokenLinks(string referral)
		{
			_db.WikiReferrals.Add(new WikiPageReferral { Referrer = "Page", Referral = referral });
			var actual = await _wikiPages.BrokenLinks();

			Assert.IsNotNull(actual);
			Assert.AreEqual(0, actual.Count());
		}

		#endregion

		private int AddPage(string name, bool isDeleted = false, bool cache = false)
		{
			var wp = new WikiPage { PageName = name, IsDeleted = isDeleted };
			_db.Add(wp);
			_db.SaveChanges();

			if (cache)
			{
				_cache.Set(wp.PageName, wp);
			}

			return wp.Id;
		}
	}

	internal class WikiTestCache : ICacheService
	{
		private readonly Dictionary<string, object> _cache = new Dictionary<string, object>();

		public List<WikiPage> PageCache { get; set; } = new List<WikiPage>();

		public void Remove(string key)
		{
			var page = PageCache.SingleOrDefault(p => p.PageName == key.Split('-').Last());
			if (page != null)
			{
				PageCache.Remove(page);
			}

			_cache.Remove(key);
		}

		public void Set(string key, object data, int? cacheTime = null)
		{
			if (data is WikiPage page)
			{
				// This is to ensure that reference equality fails
				// In a real world scenario, we would not expect the cached version
				// to be the same copy as those returned by EF queries
				PageCache.Add(new WikiPage
				{
					Id = page.Id,
					PageName = page.PageName,
					Markup = page.Markup,
					Revision = page.Revision,
					MinorEdit = page.MinorEdit,
					RevisionMessage = page.RevisionMessage,
					ChildId = page.ChildId,
					Child = page.Child,
					IsDeleted = page.IsDeleted
				});
			}

			_cache[key] = data;
		}

		public bool TryGetValue<T>(string key, out T value)
		{
			var result = _cache.TryGetValue(key, out object cached);
			value = (T)cached;
			return result;
		}
	}
}
