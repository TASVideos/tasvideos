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

			var actual = await _wikiPages.Exists(existingPage, false);
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

		// TODO: Current has a later deleted revision, but still considered current

		#endregion

		#region Add

		[TestMethod]
		public async Task Add_NewPage()
		{
			string newPage = "New Page";
			await _wikiPages.Add(new WikiPage { PageName = newPage, Markup = "" });

			Assert.AreEqual(1, _db.WikiPages.Count());
			Assert.AreEqual(newPage, _db.WikiPages.Single().PageName);
			Assert.AreEqual(1, _db.WikiPages.Single().Revision);
			Assert.IsNull(_db.WikiPages.Single().ChildId);

			Assert.AreEqual(1, _cache.PageCache.Count);
			Assert.AreEqual(newPage, _cache.PageCache.Single().PageName);
			Assert.AreEqual(1, _cache.PageCache.Single().Revision);
			Assert.IsNull(_cache.PageCache.Single().ChildId);
		}

		[TestMethod]
		public async Task Add_RevisionToExistingPage()
		{
			string existingPageName = "Existing Page";
			var existingPage = new WikiPage { PageName = existingPageName, Markup = "" };
			_db.WikiPages.Add(existingPage);
			_db.SaveChanges();
			_cache.PageCache.Add(existingPage);

			await _wikiPages.Add(new WikiPage { PageName = existingPageName, Markup = "" });

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
		}

		#endregion

		#region Revision

		[TestMethod]
		public async Task Revision_Exists_ReturnsPage()
		{
			string existingPage = "Exists";
			var page = new WikiPage { PageName = existingPage };
			_db.WikiPages.Add(page);
			_db.SaveChanges();

			var actual = await _wikiPages.Revision(page.Id);
			Assert.IsNotNull(actual);
			Assert.AreEqual(1, _cache.PageCache.Count);
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
			_db.WikiPages.Add(new WikiPage { PageName = existingPage });
			_db.SaveChanges();
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
			var existingPage = new WikiPage { PageName = existingPageName };
			_db.WikiPages.Add(existingPage);
			_cache.PageCache.Add(existingPage);
			_db.SaveChanges();

			await _wikiPages.Move(existingPageName, newPageName);
			Assert.AreEqual(1, _db.WikiPages.Count());
			Assert.AreEqual(newPageName, _db.WikiPages.Single().PageName);
			Assert.AreEqual(1, _cache.PageCache.Count);
			Assert.AreEqual(newPageName, _cache.PageCache.Single().PageName);
			// TODO: test that referrers are updated
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
			// TODO: test that referrers are updated
		}

		#endregion

		#region Delete Page

		// TODO: Delete page that does not exist - nothing happens, nothing in cache, 0 is returned
		// TODO: Delete existing page single revision - gone, nothing in cache, 1 is returned, referrers updated
		// TODO: Delete existing page multiple revisions - all gone, nothing in cache, revision count returned, referrers updated

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
			string existingPageName = "Exists";
			var currentRevision = new WikiPage { PageName = existingPageName, Revision = 2, ChildId = null };
			var previousRevision = new WikiPage { PageName = existingPageName, Revision = 1, Child = currentRevision };
			_db.WikiPages.Add(previousRevision);
			_db.WikiPages.Add(currentRevision);
			_db.SaveChanges();
			_cache.PageCache.Add(currentRevision);

			await _wikiPages.Delete(existingPageName, int.MaxValue);
			Assert.AreEqual(2, _db.WikiPages.ThatAreNotDeleted().Count());
			Assert.AreEqual(1, _cache.PageCache.Count);
		}

		[TestMethod]
		public async Task DeleteRevision_DeletingCurrent_SetsPreviousToCurrent()
		{
			string existingPageName = "Exists";
			var currentRevision = new WikiPage { PageName = existingPageName, Revision = 2, ChildId = null };
			var previousRevision = new WikiPage { PageName = existingPageName, Revision = 1, Child = currentRevision };
			_db.WikiPages.Add(previousRevision);
			_db.WikiPages.Add(currentRevision);
			_db.SaveChanges();
			_cache.PageCache.Add(currentRevision);

			await _wikiPages.Delete(existingPageName, 2);

			// TODO:
			// Latest revision shoudl be deleted. Previous should be "current"
			// Cache should have "current"
			// Referrers should be updated
			Assert.AreEqual(1, _db.WikiPages.ThatAreNotDeleted().Count());
		}

		#endregion

		#region Undelete

		// Undelete - page does not exist - nothing happens, cache not updated
		// Undelete - page exists - 1 revision - not in cache - page now exists, is in cache, referrers updated
		// Undelete - page exists - 2 revisions - not in cache - page now exists, latest is in cache, referrers updated to latest

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
		public void ThatAreParentsOf_AnscestorsReturned()
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

		// Page is not system page - returns false
		// Page is System itself - true is returned
		// Page is a system page - true is returned

		#endregion

		private void AddPage(string name, bool isDeleted = false)
		{
			_db.Add(new WikiPage { PageName = name, IsDeleted = isDeleted });
			_db.SaveChanges();
			_wikiPages.FlushCache();
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
				return list as List<WikiPage>;
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
