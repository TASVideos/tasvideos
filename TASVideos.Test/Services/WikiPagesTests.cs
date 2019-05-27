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

				return new List<WikiPage>();
			}
		}

		public void Remove(string key)
		{
			throw new System.NotImplementedException();
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
