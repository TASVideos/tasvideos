using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TASVideos.Data.Entity;

// ReSharper disable InconsistentNaming
namespace TASVideos.Test.Data.QueryableExtensions
{
	[TestClass]
	public class WikiPageExtensionTests
	{
		private TestDbContext _db;

		[TestInitialize]
		public void Initialize()
		{
			_db = TestDbContext.Create();
		}

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

			var result = _db.WikiPages.ThatAreSubpagesOf(testPageName).ToList();
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

			var result = _db.WikiPages.ThatAreSubpagesOf(testPage).ToList();
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

			var result = _db.WikiPages.ThatAreSubpagesOf(testPage).ToList();
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

			var result = _db.WikiPages.ThatAreSubpagesOf(testPage).ToList();
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

			var result = _db.WikiPages.ThatAreSubpagesOf("/" + testPage + "/").ToList();
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

			var result = _db.WikiPages.ThatAreParentsOf(testName).ToList();
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

			var result = _db.WikiPages.ThatAreParentsOf(parent).ToList();
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

			var result = _db.WikiPages.ThatAreParentsOf(parent + "/Child1").ToList();
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

			var result = _db.WikiPages.ThatAreParentsOf(testName).ToList();
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

			var result = _db.WikiPages.ThatAreParentsOf("/" + childPage + "/").ToList();
			Assert.AreEqual(1, result.Count);
		}

		#endregion

		#region IsCurrent

		[TestMethod]
		public void IsCurrent_NullSafe()
		{
			var actual = ((WikiPage)null).IsCurrent();
			Assert.IsFalse(actual);
		}

		[TestMethod]
		public void IsCurrent_Current_ReturnsTrue()
		{
			var actual = new WikiPage { ChildId = null, IsDeleted = false }.IsCurrent();
			Assert.IsTrue(actual);
		}

		[TestMethod]
		public void IsCurrent_ChildId_ReturnsFalse()
		{
			var actual = new WikiPage { ChildId = 1, IsDeleted = false }.IsCurrent();
			Assert.IsFalse(actual);
		}

		[TestMethod]
		public void IsCurrent_Deleted_ReturnsFalse()
		{
			var actual = new WikiPage { ChildId = null, IsDeleted = true }.IsCurrent();
			Assert.IsFalse(actual);
		}

		#endregion
	}
}
