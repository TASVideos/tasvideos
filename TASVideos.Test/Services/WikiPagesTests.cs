using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Services;

// ReSharper disable InconsistentNaming
namespace TASVideos.Test.Services
{
	// TODO: concurrency exceptions
	[TestClass]
	public class WikiPagesTests
	{
		private IWikiPages _wikiPages;
		private ApplicationDbContext _db;
		private StaticCache _cache;

		[TestInitialize]
		public void Initialize()
		{
			var options = new DbContextOptionsBuilder<ApplicationDbContext>()
				.UseInMemoryDatabase("TestDb")
				.Options;
			_db = new ApplicationDbContext(options, null);
			_db.Database.EnsureDeleted();
			_cache = new StaticCache();
			_wikiPages = new WikiPages(_db, _cache);
		}

		#region Exists

		[TestMethod]
		public async Task Exists_PageExists_ReturnsTrue()
		{
			string existingPage = "Exists";
			AddPage(existingPage);

			var actual = await _wikiPages.Exists(existingPage);
			Assert.AreEqual(1, _cache.PageCache.Count, "Cache should have  1 record");
			Assert.AreEqual(existingPage, _cache.PageCache.First().PageName, "Cache should match page checked");
			Assert.IsTrue(actual);
		}

		[TestMethod]
		public async Task Exists_PageDoesNotExist_ReturnsFalse()
		{
			string existingPage = "Exists";
			AddPage(existingPage);

			var actual = await _wikiPages.Exists("DoesNotExist", false);
			Assert.IsFalse(actual);
		}

		[TestMethod]
		public async Task Exists_OnlyDeletedExists_IncludeDeleted_ReturnsTrue()
		{
			string existingPage = "Exists";
			AddPage(existingPage, isDeleted: true);

			var actual = await _wikiPages.Exists(existingPage, includeDeleted: true);
			Assert.AreEqual(1, _cache.PageCache.Count, "Cache should have  1 record");
			Assert.AreEqual(existingPage, _cache.PageCache.First().PageName, "Cache should match page checked");
			Assert.IsTrue(actual);
		}

		[TestMethod]
		public async Task Exists_OnlyDeletedExists_DoNotIncludeDeleted_ReturnFalse()
		{
			string existingPage = "Exists";
			AddPage(existingPage, isDeleted: true);

			var actual = await _wikiPages.Exists(existingPage, includeDeleted: false);
			Assert.AreEqual(0, _cache.PageCache.Count, "Non-existent page was not cached.");
			Assert.IsFalse(actual);
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
			Assert.IsNull(latest.Child);

			Assert.AreEqual(1, _cache.PageCache.Count);
			Assert.AreEqual(3, _cache.PageCache.Single().Revision);

			Assert.AreEqual(1, _db.WikiReferrals.Count());
			Assert.AreEqual(pageName, _db.WikiReferrals.Single().Referrer);
			Assert.AreEqual(revision3Link, _db.WikiReferrals.Single().Referral);
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
		public async Task Revision_PullsFromCache_IfAvailable()
		{
			string existingPage = "InCache";
			var page = new WikiPage { Id = 111, PageName = existingPage };
			_cache.PageCache.Add(page);

			var actual = await _wikiPages.Revision(111);
			Assert.IsNotNull(actual);
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
			await _wikiPages.Move("Does not exist", "Also does not exist");
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

			await _wikiPages.Move(existingPageName, newPageName);
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

			await _wikiPages.Move(existingPageName, newPageName);
			Assert.AreEqual(2, _db.WikiPages.Count());
			Assert.IsTrue(_db.WikiPages.All(wp => wp.PageName == newPageName));

			Assert.AreEqual(1, _cache.PageCache.Count);
			Assert.AreEqual(newPageName, _cache.PageCache.Single().PageName);
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
			Assert.AreEqual(1, _db.WikiPages.ThatAreDeleted().ThatAreCurrentRevisions().Count());
			var current = _db.WikiPages
				.ThatAreNotDeleted()
				.ThatAreCurrentRevisions()
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

		#endregion

		#region Undelete

		[TestMethod]
		public async Task Undelete_PageDoesNotExist_NothingHappens()
		{
			await _wikiPages.Undelete("Does not exist");
			Assert.AreEqual(0, _db.WikiPages.Count());
			Assert.AreEqual(0, _cache.PageCache.Count);
		}

		[TestMethod]
		public async Task Undelete_ExistingPageThatIsNotDeleted_NothingHappens()
		{
			string pageName = "Exists";
			AddPage(pageName, isDeleted: false, cache: true);

			await _wikiPages.Undelete(pageName);
			Assert.AreEqual(1, _db.WikiPages.Count());
			Assert.AreEqual(1, _cache.PageCache.Count);
			Assert.IsFalse(_db.WikiPages.Single().IsDeleted);
		}

		[TestMethod]
		public async Task Undelete_DeletedPage_UndeletesPage()
		{
			string pageName = "Deleted";
			AddPage(pageName, isDeleted: true);
			_cache.PageCache.Clear();

			await _wikiPages.Undelete(pageName);

			Assert.AreEqual(1, _db.WikiPages.ThatAreNotDeleted().Count());
			// TODO: Assert.AreEqual(1, _cache.PageCache.Count);
			// TODO: check referrers are updated
		}

		// Undelete - page exists - 2 revisions - not in cache - page now exists, latest is in cache, referrers updated to latest
		// Undelete - page exists - only 1 revision of 2 is deleted, undeletes revision 1, and restores child ids

		#endregion

		#region ThatAreSubpagesOf
		
		[TestMethod]
		[DataRow(null)]
		[DataRow("")]
		public void ThatAreSubpagesOf_NoPage_ConsideredRoot_AllReturned(string testPageName)
		{
			var pages = new[]
			{
				new WikiPage { PageName = "Parent1" },
				new WikiPage { PageName = "Parent2" },
				new WikiPage { PageName = "Parent2/Child1" },
				new WikiPage { PageName = "Parent2/Child1" }
			};
			_db.WikiPages.AddRange(pages);
			_db.SaveChanges();

			var result = _wikiPages.ThatAreSubpagesOf(testPageName).ToList();
			Assert.AreEqual(pages.Length, result.Count);
		}

		[TestMethod]
		public void ThatAreSubpagesOf_ReturnsAllDescendants()
		{
			string testPage = "TestPage";
			string anotherPage = "AnotherPage";
			var pages = new[]
			{
				new WikiPage { PageName = testPage },
				new WikiPage { PageName = testPage + "/Child" },
				new WikiPage { PageName = testPage + "/Child/Descendant" },
				new WikiPage { PageName = anotherPage },
				new WikiPage { PageName = anotherPage + "/Child" },
				new WikiPage { PageName = anotherPage + "/Child/Descendant" },
			};
			_db.WikiPages.AddRange(pages);
			_db.SaveChanges();

			var result = _wikiPages.ThatAreSubpagesOf(testPage).ToList();
			Assert.AreEqual(2, result.Count);
		}

		[TestMethod]
		public void ThatAreSubpagesOf_PageDoesNotExist_NoChildren_EmptyListReturned()
		{
			string testPage = "TestPage";
			string anotherPage = "AnotherPage";
			var pages = new[]
			{
				new WikiPage { PageName = testPage },
				new WikiPage { PageName = anotherPage },
				new WikiPage { PageName = anotherPage + "/Child" },
				new WikiPage { PageName = anotherPage + "/Child/Descendant" },
			};
			_db.WikiPages.AddRange(pages);
			_db.SaveChanges();

			var result = _wikiPages.ThatAreSubpagesOf(testPage).ToList();
			Assert.AreEqual(0, result.Count);
		}

		[TestMethod]
		public void ThatAreSubpagesOf_PageDoesNotExist_ChildrenDo_ReturnsChildren()
		{
			string testPage = "TestPage";
			string anotherPage = "AnotherPage";
			var pages = new[]
			{
				new WikiPage { PageName = testPage + "/Child" },
				new WikiPage { PageName = testPage + "/Child/Descendant" },
				new WikiPage { PageName = anotherPage },
				new WikiPage { PageName = anotherPage + "/Child" },
				new WikiPage { PageName = anotherPage + "/Child/Descendant" },
			};
			_db.WikiPages.AddRange(pages);
			_db.SaveChanges();

			var result = _wikiPages.ThatAreSubpagesOf(testPage).ToList();
			Assert.AreEqual(2, result.Count);
		}

		[TestMethod]
		public void ThatAreSubpagesOf_TrailingSlashesTrimmed()
		{
			string testPage = "TestPage";

			var pages = new[]
			{
				new WikiPage { PageName = testPage },
				new WikiPage { PageName = testPage + "/Child" },
				new WikiPage { PageName = testPage + "/Child/Descendant" },
			};
			_db.WikiPages.AddRange(pages);
			_db.SaveChanges();

			var result = _wikiPages.ThatAreSubpagesOf("/" + testPage + "/").ToList();
			Assert.AreEqual(2, result.Count);
		}

		#endregion

		#region ThatAreParentsOf

		[TestMethod]
		[DataRow(null)]
		[DataRow("")]
		public void ThatAreParentsOf(string testName)
		{
			var pages = new[]
			{
				new WikiPage { PageName = "Parent1" },
				new WikiPage { PageName = "Parent2" },
				new WikiPage { PageName = "Parent2/Child1" },
				new WikiPage { PageName = "Parent2/Child2" }
			};
			_db.WikiPages.AddRange(pages);
			_db.SaveChanges();

			var result = _wikiPages.ThatAreParentsOf(testName).ToList();
			Assert.AreEqual(0, result.Count);
		}

		[TestMethod]
		public void ThatAreParentsOf_NoParents_NothingIsReturned()
		{
			string parent = "Parent1";
			var pages = new[]
			{
				new WikiPage { PageName = parent + "/Child1" }
			};
			_db.WikiPages.AddRange(pages);
			_db.SaveChanges();

			var result = _wikiPages.ThatAreParentsOf(parent).ToList();
			Assert.AreEqual(0, result.Count);
		}

		[TestMethod]
		public void ThatAreParentsOf_PageDoesNotExist_ParentsStillReturned()
		{
			string parent = "Parent1";
			var pages = new[]
			{
				new WikiPage { PageName = parent }
			};
			_db.WikiPages.AddRange(pages);
			_db.SaveChanges();

			var result = _wikiPages.ThatAreParentsOf(parent + "/Child1").ToList();
			Assert.AreEqual(1, result.Count);
		}

		[TestMethod]
		public void ThatAreParentsOf_AncestorsReturned()
		{
			string testName = "Parent2/Child1/Descendant1";
			var pages = new[]
			{
				new WikiPage { PageName = "Parent1" },
				new WikiPage { PageName = "Parent2" },
				new WikiPage { PageName = "Parent2/Child1" },
				new WikiPage { PageName = testName }
			};
			_db.WikiPages.AddRange(pages);
			_db.SaveChanges();

			var result = _wikiPages.ThatAreParentsOf(testName).ToList();
			Assert.AreEqual(2, result.Count);
			Assert.IsTrue(result.All(wp => wp.PageName.StartsWith("Parent2")));
		}

		[TestMethod]
		public void ThatAreParentsOf_TrailingSlashesTrimmed()
		{
			string testPage = "TestPage";
			string childPage = testPage + "/Child";

			var pages = new[]
			{
				new WikiPage { PageName = testPage },
				new WikiPage { PageName = childPage },
			};
			_db.WikiPages.AddRange(pages);
			_db.SaveChanges();

			var result = _wikiPages.ThatAreParentsOf("/" + childPage + "/").ToList();
			Assert.AreEqual(1, result.Count);
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

		private int AddPage(string name, bool isDeleted = false, bool cache = false)
		{
			var wp = new WikiPage { PageName = name, IsDeleted = isDeleted };
			_db.Add(wp);
			_db.SaveChanges();
			_wikiPages.FlushCache();

			if (cache)
			{
				_cache.PageCache.Add(wp);
			}

			return wp.Id;
		}
	}

	internal class StaticCache : ICacheService
	{
		private readonly Dictionary<string, object> _cache = new Dictionary<string, object>();

		public List<WikiPage> PageCache
		{
			get
			{
				var result = _cache.TryGetValue(CacheKeys.WikiCache, out object list);
				if (result)
				{
					return list as List<WikiPage>;
				}

				list = new List<WikiPage>();
				_cache.Add(CacheKeys.WikiCache, list);
				return (List<WikiPage>) list;
			}
		}

		public void Remove(string key)
		{
			throw new NotImplementedException();
		}

		public void Set(string key, object data, int? cacheTime = null)
		{
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
