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
		private ICacheService _cache;

		[TestInitialize]
		public void Initialize()
		{
			var options = new DbContextOptionsBuilder<ApplicationDbContext>()
				.UseInMemoryDatabase("TestDb")
				.Options;
			_db = new ApplicationDbContext(options, null);
			_db.Database.EnsureDeleted();
			_cache = new NoCacheService();
			_wikiPages = new WikiPages(_db, _cache);
		}

		[TestMethod]
		public async Task Exists_PageExists_ReturnsTrue()
		{
			string existingPage = "Exists";
			AddPage(existingPage);

			var actual = await _wikiPages.Exists(existingPage, false);
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
			Assert.IsTrue(actual);
		}

		[TestMethod]
		public async Task Exists_OnlyDeletedExists_DoNotIncludeDeleted_ReturnFalse()
		{
			string existingPage = "Exists";
			AddPage(existingPage, isDeleted: true);

			var actual = await _wikiPages.Exists(existingPage, includeDeleted: false);
			Assert.IsFalse(actual);
		}

		private void AddPage(string name, bool isDeleted = false)
		{
			_db.Add(new WikiPage { PageName = name, IsDeleted = isDeleted });
			_db.SaveChanges();
			_wikiPages.FlushCache();
		}
	}
}
