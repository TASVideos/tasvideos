using TASVideos.Core.Services.Wiki;
using TASVideos.Data.Entity;

namespace TASVideos.Core.Tests.Services;

[TestClass]
public class WikiPagesTests : TestDbBase
{
	private readonly WikiPages _wikiPages;
	private readonly WikiTestCache _cache;

	public WikiPagesTests()
	{
		_cache = new WikiTestCache();
		_wikiPages = new WikiPages(_db, _cache);
	}

	#region Exists

	[TestMethod]
	public async Task Exists_PageExists_ReturnsTrue()
	{
		const string existingPage = "Exists";
		AddPage(existingPage);

		var actual = await _wikiPages.Exists(existingPage);
		Assert.AreEqual(1, _cache.PageCache().Count, "Cache should have 1 record");
		Assert.AreEqual(existingPage, _cache.PageCache().First().PageName, "Cache should match page checked");
		Assert.IsTrue(actual);
	}

	[TestMethod]
	public async Task Exists_PageDoesNotExist_ReturnsFalse()
	{
		const string existingPage = "Exists";
		AddPage(existingPage);

		var actual = await _wikiPages.Exists("DoesNotExist");
		Assert.IsFalse(actual);
	}

	[TestMethod]
	public async Task Exists_OnlyDeletedExists_IncludeDeleted_ReturnsTrue()
	{
		const string existingPage = "Exists";
		AddPage(existingPage, isDeleted: true);
		AddPage(existingPage, isDeleted: true);

		var actual = await _wikiPages.Exists(existingPage, includeDeleted: true);
		Assert.AreEqual(0, _cache.PageCache().Count, "Cache should have no records");
		Assert.IsTrue(actual);
	}

	[TestMethod]
	public async Task Exists_OnlyDeletedExists_DoNotIncludeDeleted_ReturnFalse()
	{
		const string existingPage = "Exists";
		AddPage(existingPage, isDeleted: true);

		var actual = await _wikiPages.Exists(existingPage);
		Assert.AreEqual(0, _cache.PageCache().Count, "Non-existent page was not cached.");
		Assert.IsFalse(actual);
	}

	[TestMethod]
	[DataRow(null)]
	[DataRow("")]
	[DataRow("\r \n \t")]
	public async Task Exists_NoPageName_ReturnsFalse(string pageName)
	{
		var actual = await _wikiPages.Exists(pageName);
		Assert.IsFalse(actual);
	}

	[TestMethod]
	public async Task Exists_TrailingSlash_StillReturnsTrue()
	{
		const string existingPage = "Exists";
		AddPage(existingPage);

		var actual = await _wikiPages.Exists("/" + existingPage + "/");
		Assert.AreEqual(1, _cache.PageCache().Count, "Cache should have 1 record");
		Assert.AreEqual(existingPage, _cache.PageCache().First().PageName, "Cache should match page checked");
		Assert.IsTrue(actual);
	}

	#endregion

	#region Page

	[TestMethod]
	public async Task Page_PageExists_ReturnsPage()
	{
		const string existingPage = "Exists";
		AddPage(existingPage);

		var actual = await _wikiPages.Page(existingPage);
		Assert.AreEqual(1, _cache.PageCache().Count, "Cache should have  1 record");
		Assert.AreEqual(existingPage, _cache.PageCache().First().PageName, "Cache should match page checked");
		Assert.IsNotNull(actual);
		Assert.AreEqual(existingPage, actual.PageName);
	}

	[TestMethod]
	public async Task Page_CaseInsensitive_CachesCorrectPage()
	{
		const string existingPage = "Exists";
		AddPage(existingPage, cache: true);

		var actual = await _wikiPages.Page(existingPage.ToUpper());
		Assert.AreEqual(1, _cache.PageCache().Count, "Cache should have  1 record");
		Assert.AreEqual(existingPage, _cache.PageCache().First().PageName, "Cache should match page checked");
		Assert.IsNotNull(actual);
		Assert.AreEqual(existingPage, actual.PageName);
	}

	[TestMethod]
	public async Task Page_PageDoesNotExist_ReturnsNull()
	{
		const string existingPage = "Exists";
		AddPage(existingPage);

		var actual = await _wikiPages.Page("DoesNotExist");
		Assert.IsNull(actual);
	}

	[TestMethod]
	public async Task Page_PreviousRevision_ReturnsPage()
	{
		const string existingPage = "Exists";
		_db.WikiPages.Add(new WikiPage { Id = 1, PageName = existingPage, Markup = "", Revision = 1, ChildId = 2 });
		_db.WikiPages.Add(new WikiPage { Id = 2, PageName = existingPage, Markup = "", Revision = 2, ChildId = null });
		await _db.SaveChangesAsync();

		var actual = await _wikiPages.Page(existingPage, 1);
		Assert.IsNotNull(actual);
	}

	[TestMethod]
	public async Task Page_OnlyDeletedExists_ReturnsNull()
	{
		const string existingPage = "Exists";
		AddPage(existingPage, isDeleted: true);

		var actual = await _wikiPages.Page(existingPage);
		Assert.AreEqual(0, _cache.PageCache().Count, "Non-existent page was not cached.");
		Assert.IsNull(actual);
	}

	[TestMethod]
	public async Task Page_TrimsTrailingSlashes()
	{
		const string existingPage = "Exists";
		AddPage(existingPage);

		var actual = await _wikiPages.Page("/" + existingPage + "/");
		Assert.IsNotNull(actual);
		Assert.AreEqual(existingPage, actual.PageName);
	}

	[TestMethod]
	public async Task Page_LatestRevisionIsDeleted_PreviousConsideredCurrent()
	{
		const string pageName = "Page";
		var author = _db.AddUser(1, "_").Entity;
		var revision1 = new WikiPage { PageName = pageName, Revision = 1, IsDeleted = false, ChildId = null, Author = author };
		var revision2 = new WikiPage { PageName = pageName, Revision = 2, IsDeleted = true, ChildId = null };
		_db.WikiPages.Add(revision1);
		_db.WikiPages.Add(revision2);
		await _db.SaveChangesAsync();
		_cache.AddPage(revision1);

		var actual = await _wikiPages.Page(pageName);
		Assert.IsNotNull(actual);
		Assert.AreEqual(1, actual.Revision);
		Assert.IsTrue(actual.IsCurrent());
		Assert.AreEqual(pageName, actual.PageName);
	}

	[TestMethod]
	public async Task Page_LatestTwoRevisionsDeleted_PreviousConsideredCurrent()
	{
		const string pageName = "Page";
		var author = _db.AddUser(1, "_").Entity;
		var revision1 = new WikiPage { PageName = pageName, Revision = 1, IsDeleted = false, ChildId = null, Author = author };
		var revision2 = new WikiPage { PageName = pageName, Revision = 2, IsDeleted = true, ChildId = null };
		var revision3 = new WikiPage { PageName = pageName, Revision = 3, IsDeleted = true, ChildId = null };
		_db.WikiPages.Add(revision1);
		_db.WikiPages.Add(revision2);
		_db.WikiPages.Add(revision3);
		await _db.SaveChangesAsync();
		_cache.AddPage(revision1);

		var actual = await _wikiPages.Page(pageName);
		Assert.IsNotNull(actual);
		Assert.AreEqual(1, actual.Revision);
		Assert.IsTrue(actual.IsCurrent());
		Assert.AreEqual(pageName, actual.PageName);
	}

	[TestMethod]
	public async Task Page_MultipleCurrent_PickMostRecent()
	{
		// This scenario should never happen, but if it does, we want to get the latest revision
		const string page = "Duplicate";
		_db.WikiPages.Add(new WikiPage { PageName = page, Revision = 1, IsDeleted = false, ChildId = null });
		_db.WikiPages.Add(new WikiPage { PageName = page, Revision = 2, IsDeleted = false, ChildId = null });
		await _db.SaveChangesAsync();

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
		var actual = await _wikiPages.Page(pageName);
		Assert.IsNull(actual);
	}

	#endregion

	#region Add

	[TestMethod]
	public async Task Add_NewPage()
	{
		const string newPage = "New Page";
		const string anotherPage = "AnotherPage";
		const int authorId = 1;
		const string authorName = "TestAuthor";
		_db.AddUser(authorId, authorName);
		await _db.SaveChangesAsync();

		var result = await _wikiPages.Add(new WikiCreateRequest { PageName = newPage, Markup = $"[{anotherPage}]", AuthorId = authorId });

		Assert.IsNotNull(result);
		Assert.AreEqual(authorName, result.AuthorName);
		Assert.AreEqual(newPage, result.PageName);
		Assert.AreEqual(1, _db.WikiPages.Count());
		Assert.AreEqual(newPage, _db.WikiPages.Single().PageName);
		Assert.AreEqual(1, _db.WikiPages.Single().Revision);
		Assert.IsNull(_db.WikiPages.Single().ChildId);

		var pageCache = _cache.PageCache();
		Assert.AreEqual(1, pageCache.Count);
		Assert.AreEqual(newPage, pageCache.Single().PageName);
		Assert.AreEqual(1, pageCache.Single().Revision);
		Assert.IsNull(pageCache.Single().ChildId);

		Assert.AreEqual(1, _db.WikiReferrals.Count());
		Assert.AreEqual(anotherPage, _db.WikiReferrals.Single().Referral);
		Assert.AreEqual(newPage, _db.WikiReferrals.Single().Referrer);
	}

	[TestMethod]
	public async Task Add_UserDoesNotExist_Throws()
	{
		var revision = new WikiCreateRequest { PageName = "Test", AuthorId = int.MaxValue };
		await Assert.ThrowsExactlyAsync<InvalidOperationException>(() => _wikiPages.Add(revision));
	}

	[TestMethod]
	public async Task Add_OverridesTimestampWithCurrent()
	{
		const int authorId = 1;
		const string authorName = "TestAuthor";
		_db.AddUser(authorId, authorName);
		await _db.SaveChangesAsync();
		var origTime = DateTime.UtcNow.AddHours(-1);
		var revision = new WikiCreateRequest { PageName = "Test", CreateTimestamp = origTime, AuthorId = authorId };

		var result = await _wikiPages.Add(revision);

		Assert.IsNotNull(result);
		Assert.AreEqual(authorName, result.AuthorName);
		Assert.IsTrue(result.CreateTimestamp > origTime);
	}

	[TestMethod]
	public async Task Add_NewPage_TimestampConflict_ReturnsNull()
	{
		const int authorId = 1;
		const string authorName = "TestAuthor";
		_db.AddUser(authorId, authorName);
		await _db.SaveChangesAsync();
		const string pageName = "TestPage";
		var firstToStartEditing = new WikiCreateRequest
		{
			PageName = pageName,
			CreateTimestamp = DateTime.UtcNow.AddMinutes(-2),
			AuthorId = authorId
		};
		var secondToStartEditing = new WikiCreateRequest
		{
			PageName = pageName,
			CreateTimestamp = DateTime.UtcNow.AddMinutes(-1),
			AuthorId = authorId
		};

		await _wikiPages.Add(secondToStartEditing);

		var result = await _wikiPages.Add(firstToStartEditing);
		Assert.IsNull(result);
	}

	[TestMethod]
	public async Task Add_RevisionToExistingPage()
	{
		const int authorId = 1;
		const string authorName = "TestAuthor";
		_db.AddUser(authorId, authorName);
		const string oldLink = "OldPage";
		const string newLink = "NewLink";
		const string existingPageName = "Existing Page";
		_db.WikiReferrals.Add(new WikiPageReferral
		{
			Excerpt = $"[{oldLink}]",
			Referral = oldLink,
			Referrer = existingPageName
		});
		var existingPage = new WikiPage
		{
			PageName = existingPageName,
			Markup = $"[{oldLink}]",
			AuthorId = authorId
		};
		_db.WikiPages.Add(existingPage);
		await _db.SaveChangesAsync();
		_cache.AddPage(existingPage);

		var result = await _wikiPages.Add(new WikiCreateRequest
		{
			PageName = existingPageName,
			Markup = $"[{newLink}]",
			CreateTimestamp = DateTime.UtcNow,
			AuthorId = authorId
		});

		Assert.IsNotNull(result);
		Assert.AreEqual(existingPageName, result.PageName);
		Assert.AreEqual(authorName, result.AuthorName);
		Assert.AreEqual(2, _db.WikiPages.Count());
		var previous = _db.WikiPages.SingleOrDefault(wp => wp.PageName == existingPageName && wp.ChildId != null);
		var current = _db.WikiPages.SingleOrDefault(wp => wp.PageName == existingPageName && wp.ChildId == null);

		Assert.IsNotNull(previous);
		Assert.IsNotNull(current);
		Assert.AreEqual(1, previous.Revision);
		Assert.AreEqual(current.Id, previous.ChildId);
		Assert.AreEqual(2, current.Revision);
		Assert.IsNull(current.ChildId);

		Assert.AreEqual(1, _cache.PageCache().Count);
		Assert.AreEqual(current.Revision, _cache.PageCache().Single().Revision);

		Assert.AreEqual(1, _db.WikiReferrals.Count());
		Assert.AreEqual(existingPageName, _db.WikiReferrals.Single().Referrer);
		Assert.AreEqual(newLink, _db.WikiReferrals.Single().Referral);
	}

	[TestMethod]
	public async Task Add_RevisionToPageWithLatestRevisionDeleted()
	{
		const int authorId = 1;
		const string authorName = "TestAuthor";
		_db.AddUser(authorId, authorName);

		// Revision 1 - Not deleted, no child id
		// Revision 2 - Deleted, no child id
		const string pageName = "Page";
		const string revision1Link = "Link1";
		const string revision2Link = "Link2";
		const string revision3Link = "Link3";
		var revision1 = new WikiPage { PageName = pageName, Revision = 1, IsDeleted = false, ChildId = null, Markup = $"[{revision1Link}]", AuthorId = authorId };
		var revision2 = new WikiPage { PageName = pageName, Revision = 2, IsDeleted = true, ChildId = null, Markup = $"[{revision2Link}]", AuthorId = authorId };
		_db.WikiPages.Add(revision1);
		_db.WikiPages.Add(revision2);
		_db.WikiReferrals.Add(new WikiPageReferral { Referrer = pageName, Referral = revision1Link });
		await _db.SaveChangesAsync();
		_cache.AddPage(revision1);

		var result = await _wikiPages.Add(new WikiCreateRequest
		{
			PageName = pageName,
			Markup = $"[{revision3Link}]",
			CreateTimestamp = DateTime.UtcNow,
			AuthorId = authorId
		});

		Assert.IsNotNull(result);
		Assert.AreEqual(3, _db.WikiPages.Count());

		var first = _db.WikiPages.OrderBy(wp => wp.Id).First();
		var latest = _db.WikiPages.OrderByDescending(wp => wp.Id).First();

		Assert.AreEqual(1, first.Revision);
		Assert.AreEqual(latest.Id, first.ChildId);
		Assert.AreEqual(3, latest.Revision);
		Assert.IsNull(latest.ChildId);

		Assert.AreEqual(1, _cache.PageCache().Count);
		Assert.AreEqual(latest.Markup, _cache.PageCache().Single().Markup);

		Assert.AreEqual(1, _db.WikiReferrals.Count());
		Assert.AreEqual(pageName, _db.WikiReferrals.Single().Referrer);
		Assert.AreEqual(revision3Link, _db.WikiReferrals.Single().Referral);
	}

	[TestMethod]
	public async Task Add_RevisionToPageWithLatestTwoRevisionsDeleted()
	{
		const int authorId = 1;
		const string authorName = "TestAuthor";
		_db.AddUser(authorId, authorName);

		// Revision 1 - Not deleted, no child id
		// Revision 2 - Deleted, no child id
		const string pageName = "Page";
		const string revision1Link = "Link1";
		const string revision2Link = "Link2";
		const string revision3Link = "Link3";
		const string revision4Link = "Link4";
		var revision1 = new WikiPage { PageName = pageName, Revision = 1, IsDeleted = false, ChildId = null, Markup = $"[{revision1Link}]", AuthorId = authorId };
		var revision2 = new WikiPage { PageName = pageName, Revision = 2, IsDeleted = true, ChildId = null, Markup = $"[{revision2Link}]", AuthorId = authorId };
		var revision3 = new WikiPage { PageName = pageName, Revision = 3, IsDeleted = true, ChildId = null, Markup = $"[{revision3Link}]", AuthorId = authorId };
		_db.WikiPages.Add(revision1);
		_db.WikiPages.Add(revision2);
		_db.WikiPages.Add(revision3);
		_db.WikiReferrals.Add(new WikiPageReferral { Referrer = pageName, Referral = revision1Link });
		await _db.SaveChangesAsync();
		_cache.AddPage(revision1);

		var result = await _wikiPages.Add(new WikiCreateRequest { PageName = pageName, Markup = $"[{revision4Link}]", CreateTimestamp = DateTime.UtcNow, AuthorId = authorId });

		Assert.IsNotNull(result);
		Assert.AreEqual(4, _db.WikiPages.Count());

		var first = _db.WikiPages.OrderBy(wp => wp.Id).First();
		var latest = _db.WikiPages.OrderByDescending(wp => wp.Id).First();

		Assert.AreEqual(1, first.Revision);
		Assert.AreEqual(latest.Id, first.ChildId);
		Assert.AreEqual(4, latest.Revision);
		Assert.IsNull(latest.ChildId);

		Assert.AreEqual(1, _cache.PageCache().Count);
		Assert.AreEqual(4, _cache.PageCache().Single().Revision);

		Assert.AreEqual(1, _db.WikiReferrals.Count());
		Assert.AreEqual(pageName, _db.WikiReferrals.Single().Referrer);
		Assert.AreEqual(revision4Link, _db.WikiReferrals.Single().Referral);
	}

	[TestMethod]
	public async Task Add_ConcurrencyError_ReturnsNull()
	{
		const int authorId = 1;
		_db.AddUser(authorId, "_");
		await _db.SaveChangesAsync();
		var revision = new WikiCreateRequest { PageName = "Test", AuthorId = 1 };
		_db.CreateConcurrentUpdateConflict();

		var result = await _wikiPages.Add(revision);
		Assert.IsNull(result);
	}

	[TestMethod]
	[DataRow(null)]
	[DataRow("")]
	[DataRow("\r \n \t")]
	public async Task Add_NoPageName_Throws(string pageName)
	{
		await Assert.ThrowsExactlyAsync<InvalidOperationException>(() => _wikiPages.Add(new WikiCreateRequest { PageName = pageName }));
	}

	[TestMethod]
	public async Task Add_SelfReference_DoesNotCrash()
	{
		var author = _db.AddUser(1, "Test").Entity;
		var wiki = new WikiPage { PageName = "TestPage", AuthorId = author.Id };
		author.WikiRevisions.Add(wiki);
		wiki.Author = author;
		await _db.SaveChangesAsync();

		var result = await _wikiPages.Add(new WikiCreateRequest
		{
			PageName = wiki.PageName,
			AuthorId = wiki.AuthorId!.Value
		});

		Assert.IsNotNull(result);
	}

	[TestMethod]
	public async Task Add_DifferentAuthors()
	{
		const int author1Id = 1;
		const string author1Name = "Test User 1";
		_db.AddUser(author1Id, author1Name);
		const int author2Id = 2;
		const string author2Name = "Test User 2";
		_db.AddUser(author2Id, author2Name);
		await _db.SaveChangesAsync();

		const string pageName = "TestPage";

		var result1 = await _wikiPages.Add(new WikiCreateRequest
		{
			PageName = pageName,
			AuthorId = author1Id
		});
		var result2 = await _wikiPages.Add(new WikiCreateRequest
		{
			PageName = pageName,
			AuthorId = author2Id
		});

		Assert.IsNotNull(result1);
		Assert.AreEqual(pageName, result1.PageName);
		Assert.AreEqual(author1Id, result1.AuthorId);
		Assert.AreEqual(author1Name, result1.AuthorName);

		Assert.IsNotNull(result2);
		Assert.AreEqual(pageName, result2.PageName);
		Assert.AreEqual(author2Id, result2.AuthorId);
		Assert.AreEqual(author2Name, result2.AuthorName);
	}

	[TestMethod]
	public async Task Add_WhenOnlyDeletedRevision_SuccessfullyAdds()
	{
		const string existingPage = "New Page";
		const int authorId = 1;
		const string authorName = "TestAuthor";
		_db.AddUser(authorId, authorName);
		_db.WikiPages.Add(new WikiPage
		{
			AuthorId = authorId,
			PageName = existingPage,
			IsDeleted = true
		});
		await _db.SaveChangesAsync();

		var result = await _wikiPages.Add(new WikiCreateRequest { PageName = existingPage, AuthorId = authorId });
		Assert.IsNotNull(result);
		Assert.AreEqual(2, result.Revision);
		Assert.AreEqual(existingPage, result.PageName);
		Assert.AreEqual(authorId, result.AuthorId);
	}

	#endregion

	#region Move

	[TestMethod]
	[DataRow(null)]
	[DataRow("")]
	[DataRow("\n \r \t")]
	public async Task Move_EmptyDestination_Throws(string destination)
	{
		await Assert.ThrowsExactlyAsync<ArgumentException>(() => _wikiPages.Move("Test", destination));
	}

	[TestMethod]
	public async Task Move_DestinationExists_Throws()
	{
		const string existingPage = "InCache";
		AddPage(existingPage);
		await Assert.ThrowsExactlyAsync<InvalidOperationException>(() => _wikiPages.Move("Original Page", existingPage));
	}

	[TestMethod]
	public async Task Move_OriginalDoesNotExist_NothingHappens()
	{
		var actual = await _wikiPages.Move("Does not exist", "Also does not exist");
		Assert.IsTrue(actual, "Page not found is considered successful");
		Assert.AreEqual(0, _db.WikiPages.Count());
		Assert.AreEqual(0, _cache.PageCache().Count);
	}

	[TestMethod]
	public async Task Move_SingleRevision()
	{
		var author = _db.AddUser(1, "_").Entity;
		const string existingPageName = "ExistingPage";
		const string newPageName = "NewPageName";
		const string link = "AnotherPage";
		var existingPage = new WikiPage { PageName = existingPageName, Markup = $"[{link}]", Author = author, AuthorId = author.Id };
		_db.WikiPages.Add(existingPage);
		_db.WikiReferrals.Add(new WikiPageReferral { Referrer = existingPageName, Referral = link });
		await _db.SaveChangesAsync();
		_cache.AddPage(existingPage);

		var actual = await _wikiPages.Move(existingPageName, newPageName);
		Assert.IsTrue(actual);
		Assert.AreEqual(1, _db.WikiPages.Count());
		Assert.AreEqual(newPageName, _db.WikiPages.Single().PageName);
		Assert.AreEqual(1, _cache.PageCache().Count);
		Assert.AreEqual(newPageName, _cache.PageCache().Single().PageName);

		Assert.AreEqual(1, _db.WikiReferrals.Count());
		Assert.AreEqual(newPageName, _db.WikiReferrals.Single().Referrer);
		Assert.AreEqual(link, _db.WikiReferrals.Single().Referral);
	}

	[TestMethod]
	public async Task Move_MultipleRevisions()
	{
		var author = _db.AddUser(1, "_").Entity;
		const string existingPageName = "ExistingPage";
		const string newPageName = "NewPageName";
		var previousRevision = new WikiPage { Id = 1, PageName = existingPageName, ChildId = 2, Author = author, AuthorId = author.Id, Revision = 1 };
		var existingPage = new WikiPage { Id = 2, PageName = existingPageName, ChildId = null, Author = author, AuthorId = author.Id, Revision = 2 };
		_db.WikiPages.Add(previousRevision);
		_db.WikiPages.Add(existingPage);
		_cache.AddPage(existingPage);
		await _db.SaveChangesAsync();

		var actual = await _wikiPages.Move(existingPageName, newPageName);
		Assert.IsTrue(actual);
		Assert.AreEqual(2, _db.WikiPages.Count());
		Assert.IsTrue(_db.WikiPages.All(wp => wp.PageName == newPageName));

		Assert.AreEqual(1, _cache.PageCache().Count);
		Assert.AreEqual(newPageName, _cache.PageCache().Single().PageName);
	}

	[TestMethod]
	public async Task Move_UpdateException_DoesNotMove()
	{
		var author = _db.AddUser(1, "_").Entity;
		const string origPageName = "Orig";
		const string origLink = "Link";
		var origPage = new WikiPage { PageName = origPageName, Markup = $"[{origLink}]", Author = author, AuthorId = author.Id };

		_db.WikiPages.Add(origPage);
		_db.WikiReferrals.Add(new WikiPageReferral { Referrer = origPageName, Referral = origLink });
		await _db.SaveChangesAsync();
		_cache.Set(origPageName, origPage.ToWikiResult());

		const string destPageName = "Dest";

		_db.CreateUpdateConflict();

		var actual = await _wikiPages.Move(origPageName, destPageName);
		Assert.IsFalse(actual, "The move was unsuccessful");

		// Moved page does not exist
		Assert.AreEqual(0, _db.WikiPages.Count(wp => wp.PageName == destPageName));

		// Cache does not have the moved page
		Assert.AreEqual(0, _cache.PageCache().Count(wp => wp.PageName == destPageName));

		// Referrers not updated
		Assert.AreEqual(0, _db.WikiReferrals.Count(wr => wr.Referrer == destPageName));
	}

	[TestMethod]
	public async Task Move_ConcurrencyException_DoesNotMove()
	{
		var author = _db.AddUser(1, "_").Entity;
		const string origPageName = "Orig";
		const string origLink = "Link";
		var origPage = new WikiPage { PageName = origPageName, Markup = $"[{origLink}]", Author = author, AuthorId = author.Id };

		_db.WikiPages.Add(origPage);
		_db.WikiReferrals.Add(new WikiPageReferral { Referrer = origPageName, Referral = origLink });
		await _db.SaveChangesAsync();
		_cache.Set(origPageName, origPage.ToWikiResult());

		const string destPageName = "Dest";

		_db.CreateConcurrentUpdateConflict();

		var actual = await _wikiPages.Move(origPageName, destPageName);
		Assert.IsFalse(actual, "The move was unsuccessful");

		// Moved page does not exist
		Assert.AreEqual(0, _db.WikiPages.Count(wp => wp.PageName == destPageName));

		// Cache does not have the moved page
		Assert.AreEqual(0, _cache.PageCache().Count(wp => wp.PageName == destPageName));

		// Referrers not updated
		Assert.AreEqual(0, _db.WikiReferrals.Count(wr => wr.Referrer == destPageName));
	}

	[TestMethod]
	public async Task Move_DestinationPage_TrimsSlashes()
	{
		var author = _db.AddUser(1, "_").Entity;
		const string existingPageName = "ExistingPage";
		const string newPageName = "NewPageName";
		const string link = "AnotherPage";
		var existingPage = new WikiPage { PageName = existingPageName, Markup = $"[{link}]", Author = author, AuthorId = author.Id };
		_db.WikiPages.Add(existingPage);
		_db.WikiReferrals.Add(new WikiPageReferral { Referrer = existingPageName, Referral = link });
		await _db.SaveChangesAsync();
		_cache.AddPage(existingPage);

		var actual = await _wikiPages.Move(existingPageName, "/" + newPageName + "/");
		Assert.IsTrue(actual);
		Assert.AreEqual(1, _db.WikiPages.Count());
		Assert.AreEqual(newPageName, _db.WikiPages.Single().PageName);
		Assert.AreEqual(1, _cache.PageCache().Count);
		Assert.AreEqual(newPageName, _cache.PageCache().Single().PageName);

		Assert.AreEqual(1, _db.WikiReferrals.Count());
		Assert.AreEqual(newPageName, _db.WikiReferrals.Single().Referrer);
		Assert.AreEqual(link, _db.WikiReferrals.Single().Referral);
	}

	[TestMethod]
	public async Task Move_OriginalPage_TrimsSlashes()
	{
		var author = _db.AddUser(1, "_").Entity;
		const string existingPageName = "ExistingPage";
		const string newPageName = "NewPageName";
		const string link = "AnotherPage";
		var existingPage = new WikiPage { PageName = existingPageName, Markup = $"[{link}]", Author = author, AuthorId = author.Id };
		_db.WikiPages.Add(existingPage);
		_db.WikiReferrals.Add(new WikiPageReferral { Referrer = existingPageName, Referral = link });
		await _db.SaveChangesAsync();
		_cache.AddPage(existingPage);

		var actual = await _wikiPages.Move("/" + existingPageName + "/", newPageName);
		Assert.IsTrue(actual);
		Assert.AreEqual(1, _db.WikiPages.Count());
		Assert.AreEqual(newPageName, _db.WikiPages.Single().PageName);
		Assert.AreEqual(1, _cache.PageCache().Count);
		Assert.AreEqual(newPageName, _cache.PageCache().Single().PageName);

		Assert.AreEqual(1, _db.WikiReferrals.Count());
		Assert.AreEqual(newPageName, _db.WikiReferrals.Single().Referrer);
		Assert.AreEqual(link, _db.WikiReferrals.Single().Referral);
	}

	#endregion

	#region MoveAll

	[TestMethod]
	public async Task MoveAll_MultiplePages()
	{
		var author = _db.AddUser(1, "_").Entity;
		const string existingPageName = "ExistingPage";
		const string newPageName = "NewPageName";
		const string subPage = "Sub";
		const string link = "AnotherPage";
		const string newSubPage = $"{newPageName}/{subPage}";
		var existingPage = new WikiPage { PageName = existingPageName, Markup = $"[{link}]", Author = author, AuthorId = author.Id };
		var existingSubPage = new WikiPage { PageName = $"{existingPageName}/{subPage}", Author = author, AuthorId = author.Id };
		_db.WikiPages.Add(existingPage);
		_db.WikiPages.Add(existingSubPage);
		_db.WikiReferrals.Add(new WikiPageReferral { Referrer = existingPageName, Referral = link });
		await _db.SaveChangesAsync();
		_cache.AddPage(existingPage);
		_cache.AddPage(existingSubPage);

		var actual = await _wikiPages.MoveAll(existingPageName, newPageName);
		Assert.IsTrue(actual);
		Assert.AreEqual(2, _db.WikiPages.Count());
		Assert.AreEqual(1, _db.WikiPages.Count(wp => wp.PageName == newPageName));
		Assert.AreEqual(1, _db.WikiPages.Count(wp => wp.PageName == newSubPage));
		Assert.AreEqual(2, _cache.PageCache().Count);
		Assert.AreEqual(1, _cache.PageCache().Count(c => c.PageName == newPageName));
		Assert.AreEqual(1, _cache.PageCache().Count(c => c.PageName == newSubPage));

		Assert.AreEqual(1, _db.WikiReferrals.Count());
		Assert.AreEqual(newPageName, _db.WikiReferrals.Single().Referrer);
		Assert.AreEqual(link, _db.WikiReferrals.Single().Referral);
	}

	#endregion

	#region Delete Page

	[TestMethod]
	public async Task DeletePage_PageDoesNotExist_NothingHappens()
	{
		await _wikiPages.Delete("DoesNotExist");

		Assert.AreEqual(0, _db.WikiPages.Count());
		Assert.AreEqual(0, _cache.PageCache().Count);
	}

	[TestMethod]
	public async Task DeletePage_1Revision_RevisionDeleted()
	{
		const string pageName = "Exists";
		const string link = "AnotherPage";
		var author = _db.AddUser(1, "_").Entity;
		var existingPage = new WikiPage
		{
			PageName = pageName,
			Markup = $"[{link}]",
			AuthorId = author.Id,
			Author = author
		};
		_db.WikiPages.Add(existingPage);
		_db.WikiReferrals.Add(new WikiPageReferral { Referrer = pageName, Referral = link });
		await _db.SaveChangesAsync();
		_cache.AddPage(existingPage);

		var actual = await _wikiPages.Delete(pageName);

		Assert.AreEqual(1, actual);
		Assert.AreEqual(1, _db.WikiPages.Count());
		Assert.IsTrue(_db.WikiPages.Single().IsDeleted);
		Assert.IsNull(_db.WikiPages.Single().ChildId);
		Assert.AreEqual(0, _cache.PageCache().Count);
		Assert.AreEqual(0, _db.WikiReferrals.Count());
	}

	[TestMethod]
	public async Task DeletePage_2Revisions_AllRevisionsDeleted()
	{
		const string pageName = "Exists";
		const string link = "AnotherPage";
		var author = _db.AddUser(1, "_").Entity;
		var revision1 = new WikiPage { PageName = pageName, Revision = 1 };
		var revision2 = new WikiPage { PageName = pageName, Revision = 2, Markup = $"[{link}]", AuthorId = author.Id, Author = author };

		_db.WikiPages.Add(revision1);
		_db.WikiPages.Add(revision2);
		await _db.SaveChangesAsync();
		revision1.ChildId = revision2.Id;
		_db.WikiReferrals.Add(new WikiPageReferral { Referrer = pageName, Referral = link });
		await _db.SaveChangesAsync();
		_cache.AddPage(revision2);

		var actual = await _wikiPages.Delete(pageName);

		Assert.AreEqual(2, actual);
		Assert.AreEqual(2, _db.WikiPages.Count());
		Assert.IsTrue(_db.WikiPages.All(wp => wp.IsDeleted));
		Assert.IsTrue(_db.WikiPages.All(wp => wp.ChildId == null));
		Assert.AreEqual(0, _cache.PageCache().Count);
		Assert.AreEqual(0, _db.WikiReferrals.Count());
	}

	[TestMethod]
	public async Task DeletePage_ConcurrencyConflict_DoesNotDelete()
	{
		const string pageName = "Exists";
		const string link = "AnotherPage";
		var author = _db.AddUser(1, "_").Entity;
		var existingPage = new WikiPage { PageName = pageName, Markup = $"[{link}]", AuthorId = author.Id, Author = author };
		_db.WikiPages.Add(existingPage);
		_db.WikiReferrals.Add(new WikiPageReferral { Referrer = pageName, Referral = link });
		await _db.SaveChangesAsync();
		_cache.AddPage(existingPage);

		_db.CreateConcurrentUpdateConflict();

		var actual = await _wikiPages.Delete(pageName);

		Assert.AreEqual(-1, actual);
		Assert.AreEqual(0, _db.WikiPages.ThatAreDeleted().Count());
		Assert.AreEqual(1, _cache.PageCache().Count);
		Assert.AreEqual(1, _db.WikiReferrals.Count());
	}

	[TestMethod]
	public async Task DeletePage_TrimsSlashes()
	{
		const string pageName = "Exists";
		const string link = "AnotherPage";
		var author = _db.AddUser(1, "_").Entity;
		var existingPage = new WikiPage { PageName = pageName, Markup = $"[{link}]", AuthorId = author.Id, Author = author };
		_db.WikiPages.Add(existingPage);
		_db.WikiReferrals.Add(new WikiPageReferral { Referrer = pageName, Referral = link });
		await _db.SaveChangesAsync();
		_cache.AddPage(existingPage);

		var actual = await _wikiPages.Delete("/" + pageName + "/");

		Assert.AreEqual(1, actual);
		Assert.AreEqual(1, _db.WikiPages.Count());
		Assert.IsTrue(_db.WikiPages.Single().IsDeleted);
		Assert.IsNull(_db.WikiPages.Single().ChildId);
		Assert.AreEqual(0, _cache.PageCache().Count);
		Assert.AreEqual(0, _db.WikiReferrals.Count());
	}

	#endregion

	#region Delete Revision

	[TestMethod]
	public async Task DeleteRevision_PreviousRevision_DeletesOnlyThatRevision()
	{
		var author = _db.AddUser(1, "_").Entity;
		const string existingPageName = "Exists";
		var currentRevision = new WikiPage { PageName = existingPageName, Revision = 2, ChildId = null, Author = author, AuthorId = author.Id };
		var previousRevision = new WikiPage { PageName = existingPageName, Revision = 1, Child = currentRevision, Author = author, AuthorId = author.Id };
		_db.WikiPages.Add(previousRevision);
		_db.WikiPages.Add(currentRevision);
		await _db.SaveChangesAsync();
		_cache.AddPage(currentRevision);

		await _wikiPages.Delete(existingPageName, 1);

		Assert.AreEqual(1, _db.WikiPages.ThatAreNotDeleted().Count());
		Assert.AreEqual(existingPageName, _db.WikiPages.ThatAreNotDeleted().Single().PageName);
		Assert.AreEqual(1, _db.WikiPages.ThatAreDeleted().Single().Revision);
		Assert.AreEqual(2, _db.WikiPages.ThatAreNotDeleted().Single().Revision);

		var pageCache = _cache.PageCache();
		Assert.AreEqual(1, pageCache.Count);
		Assert.AreEqual(existingPageName, pageCache.Single().PageName);
		Assert.AreEqual(2, pageCache.Single().Revision);
	}

	[TestMethod]
	public async Task DeleteRevision_DoesNotExist_NothingHappens()
	{
		const string pageName = "Exists";
		AddPage(pageName, cache: true);

		await _wikiPages.Delete(pageName, int.MaxValue);

		Assert.AreEqual(1, _db.WikiPages.ThatAreNotDeleted().Count());
		Assert.AreEqual(1, _cache.PageCache().Count);
	}

	[TestMethod]
	public async Task DeleteRevision_AlreadyDeleted_NothingHappens()
	{
		const string pageName = "Exists";
		AddPage(pageName, isDeleted: true);

		await _wikiPages.Delete(pageName, 1);

		Assert.AreEqual(0, _db.WikiPages.ThatAreNotDeleted().Count());
		Assert.AreEqual(1, _db.WikiPages.ThatAreDeleted().Count());
		Assert.AreEqual(0, _cache.PageCache().Count);
	}

	[TestMethod]
	public async Task DeleteRevision_DeletingCurrent_SetsPreviousToCurrent()
	{
		const int authorId = 1;
		var author = _db.AddUser(authorId, "Test User").Entity;

		const string existingPageName = "Exists";
		const string oldLink = "OldPage";
		const string newLink = "NewPage";
		var currentRevision = new WikiPage
		{
			PageName = existingPageName,
			Revision = 2,
			ChildId = null,
			Markup = $"[{newLink}]",
			AuthorId = authorId,
			Author = author
		};
		var previousRevision = new WikiPage
		{
			PageName = existingPageName,
			Revision = 1,
			Child = currentRevision,
			Markup = $"[{oldLink}]",
			AuthorId = authorId,
			Author = author
		};
		_db.WikiPages.Add(previousRevision);
		_db.WikiPages.Add(currentRevision);
		await _db.SaveChangesAsync();
		_cache.AddPage(currentRevision);

		await _wikiPages.Delete(existingPageName, 2);

		// Revision 1 should be Current
		Assert.AreEqual(1, _db.WikiPages.ThatAreNotDeleted().Count());
		Assert.AreEqual(1, _db.WikiPages.ThatAreDeleted().Count());
		var current = _db.WikiPages
			.ThatAreNotDeleted()
			.ThatAreCurrent()
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
		Assert.AreEqual(1, _cache.PageCache().Count);
		Assert.AreEqual(1, _cache.PageCache().Single().Revision);

		// Referrers should be based on Revision 1
		Assert.AreEqual(1, _db.WikiReferrals.Count());
		var referrer = _db.WikiReferrals.Single();
		Assert.AreEqual(oldLink, referrer.Referral);
	}

	[TestMethod]
	public async Task DeleteRevision_DeleteCurrentWhenAlreadyALaterDeletedRevision()
	{
		var author = _db.AddUser(1, "_").Entity;

		const string pageName = "Exists";
		const string revision1Link = "Link1";
		const string revision2Link = "Link2";
		const string revision3Link = "Link3";
		var revision1 = new WikiPage { PageName = pageName, Revision = 1, Markup = $"[{revision1Link}]", AuthorId = author.Id, Author = author };
		var revision2 = new WikiPage { PageName = pageName, Revision = 2, Markup = $"[{revision2Link}]", AuthorId = author.Id, Author = author };
		var revision3 = new WikiPage { PageName = pageName, Revision = 3, IsDeleted = true, Markup = $"[{revision3Link}]", AuthorId = author.Id, Author = author };
		_db.WikiPages.Add(revision1);
		_db.WikiPages.Add(revision2);
		_db.WikiPages.Add(revision3);
		await _db.SaveChangesAsync();
		revision1.ChildId = revision2.Id;
		await _db.SaveChangesAsync();
		_cache.AddPage(revision2);

		await _wikiPages.Delete(pageName, 2);

		// Revision 1 should be Current
		Assert.AreEqual(1, _db.WikiPages.ThatAreNotDeleted().Count());
		Assert.AreEqual(2, _db.WikiPages.ThatAreDeleted().Count());
		var current = _db.WikiPages
			.ThatAreNotDeleted()
			.ThatAreCurrent()
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
		Assert.AreEqual(1, _cache.PageCache().Count);
		Assert.AreEqual(1, _cache.PageCache().Single().Revision);

		// Referrers should be based on Revision 1
		Assert.AreEqual(1, _db.WikiReferrals.Count());
		var referrer = _db.WikiReferrals.Single();
		Assert.AreEqual(revision1Link, referrer.Referral);
	}

	[TestMethod]
	public async Task DeleteRevision_DeleteCurrentWhenPreviousRevisionAlreadyDeletedRevision()
	{
		var author = _db.AddUser(1, "_").Entity;
		const string pageName = "Exists";
		const string revision1Link = "Link1";
		const string revision2Link = "Link2";
		const string revision3Link = "Link3";
		var revision1 = new WikiPage { PageName = pageName, Revision = 1, Markup = $"[{revision1Link}]", Author = author, AuthorId = author.Id };
		var revision2 = new WikiPage { PageName = pageName, Revision = 2, IsDeleted = true, Markup = $"[{revision2Link}]", Author = author, AuthorId = author.Id };
		var revision3 = new WikiPage { PageName = pageName, Revision = 3, Markup = $"[{revision3Link}]", Author = author, AuthorId = author.Id };
		_db.WikiPages.Add(revision1);
		_db.WikiPages.Add(revision2);
		_db.WikiPages.Add(revision3);
		await _db.SaveChangesAsync();
		revision1.ChildId = revision3.Id;
		await _db.SaveChangesAsync();
		_cache.AddPage(revision3);

		await _wikiPages.Delete(pageName, 3);

		// Revision 1 should be Current
		Assert.AreEqual(1, _db.WikiPages.ThatAreNotDeleted().Count());
		Assert.AreEqual(2, _db.WikiPages.ThatAreDeleted().Count());
		var current = _db.WikiPages
			.ThatAreNotDeleted()
			.ThatAreCurrent()
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
		Assert.AreEqual(1, _cache.PageCache().Count);
		Assert.AreEqual(1, _cache.PageCache().Single().Revision);

		// Referrers should be based on Revision 1
		Assert.AreEqual(1, _db.WikiReferrals.Count());
		var referrer = _db.WikiReferrals.Single();
		Assert.AreEqual(revision1Link, referrer.Referral);
	}

	[TestMethod]
	public async Task DeleteRevision_TrimsSlashes()
	{
		var author = _db.AddUser(1, "_").Entity;
		const string existingPageName = "Exists";
		var currentRevision = new WikiPage { PageName = existingPageName, Revision = 2, ChildId = null, Author = author, AuthorId = author.Id };
		var previousRevision = new WikiPage { PageName = existingPageName, Revision = 1, Child = currentRevision, Author = author, AuthorId = author.Id };
		_db.WikiPages.Add(previousRevision);
		_db.WikiPages.Add(currentRevision);
		await _db.SaveChangesAsync();
		_cache.AddPage(currentRevision);

		await _wikiPages.Delete("/" + existingPageName + "/", 1);

		Assert.AreEqual(1, _db.WikiPages.ThatAreNotDeleted().Count());
		Assert.AreEqual(existingPageName, _db.WikiPages.ThatAreNotDeleted().Single().PageName);
		Assert.AreEqual(1, _db.WikiPages.ThatAreDeleted().Single().Revision);
		Assert.AreEqual(2, _db.WikiPages.ThatAreNotDeleted().Single().Revision);

		var pageCache = _cache.PageCache();
		Assert.AreEqual(1, pageCache.Count);
		Assert.AreEqual(existingPageName, pageCache.Single().PageName);
		Assert.AreEqual(2, pageCache.Single().Revision);
	}

	#endregion

	#region Undelete

	[TestMethod]
	public async Task Undelete_PageDoesNotExist_NothingHappens()
	{
		var actual = await _wikiPages.Undelete("Does not exist");
		Assert.IsTrue(actual, "Page does not exist is considered successful");
		Assert.AreEqual(0, _db.WikiPages.Count());
		Assert.AreEqual(0, _cache.PageCache().Count);
	}

	[TestMethod]
	public async Task Undelete_ExistingPageThatIsNotDeleted_NothingHappens()
	{
		const string pageName = "Exists";
		AddPage(pageName, isDeleted: false, cache: true);

		var actual = await _wikiPages.Undelete(pageName);
		Assert.IsTrue(actual, "Page already exists considered successful");
		Assert.AreEqual(1, _db.WikiPages.Count());
		Assert.AreEqual(1, _cache.PageCache().Count);
		Assert.IsFalse(_db.WikiPages.Single().IsDeleted);
	}

	[TestMethod]
	public async Task Undelete_DeletedPage_UndeletesPage()
	{
		var author = _db.AddUser(1, "_").Entity;
		const string pageName = "Deleted";
		const string link = "AnotherPage";
		var page = new WikiPage { PageName = pageName, Markup = $"[{link}]", IsDeleted = true, Author = author, AuthorId = author.Id };
		_db.WikiPages.Add(page);
		await _db.SaveChangesAsync();

		var actual = await _wikiPages.Undelete(pageName);
		Assert.IsTrue(actual);
		Assert.AreEqual(1, _db.WikiPages.ThatAreNotDeleted().Count());
		Assert.AreEqual(1, _cache.PageCache().Count);
		Assert.AreEqual(1, _db.WikiReferrals.Count());
		Assert.AreEqual(pageName, _db.WikiReferrals.Single().Referrer);
		Assert.AreEqual(link, _db.WikiReferrals.Single().Referral);
	}

	[TestMethod]
	public async Task Undelete_OnlyLatestIsDeleted_SetsLatestToCurrent()
	{
		var author = _db.AddUser(1, "_").Entity;
		const string pageName = "Exists";
		const string oldLink = "OldLink";
		const string newLink = "NewLink";
		var revision1 = new WikiPage
		{
			PageName = pageName,
			Revision = 1,
			Markup = $"[{oldLink}]",
			Author = author,
			AuthorId = author.Id
		};
		var revision2 = new WikiPage
		{
			PageName = pageName,
			Revision = 2,
			Markup = $"[{newLink}]",
			IsDeleted = true,
			Author = author,
			AuthorId = author.Id
		};
		_db.WikiPages.Add(revision1);
		_db.WikiPages.Add(revision2);
		_db.WikiReferrals.Add(new WikiPageReferral { Referrer = pageName, Referral = oldLink });
		await _db.SaveChangesAsync();
		_cache.AddPage(revision1);

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
		Assert.AreEqual(1, _cache.PageCache().Count);
		var cached = _cache.PageCache().Single();
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
		var author = _db.AddUser(1, "_").Entity;
		const string pageName = "Exists";
		const string link1 = "Link1";
		const string link2 = "Link2";
		const string link3 = "Link3";
		var revision1 = new WikiPage { PageName = pageName, Revision = 1, Markup = $"[{link1}]", Author = author, AuthorId = author.Id };
		var revision2 = new WikiPage { PageName = pageName, Revision = 2, Markup = $"[{link2}]", IsDeleted = true, Author = author, AuthorId = author.Id };
		var revision3 = new WikiPage { PageName = pageName, Revision = 3, Markup = $"[{link3}]", IsDeleted = true, Author = author, AuthorId = author.Id };
		_db.WikiPages.Add(revision1);
		_db.WikiPages.Add(revision2);
		_db.WikiPages.Add(revision3);
		_db.WikiReferrals.Add(new WikiPageReferral { Referrer = pageName, Referral = link1 });
		await _db.SaveChangesAsync();
		_cache.AddPage(revision1);

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
		Assert.AreEqual(1, _cache.PageCache().Count);
		var cached = _cache.PageCache().Single();
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
		var author = _db.AddUser(1, "_").Entity;
		const string pageName = "Deleted";
		const string link1 = "AnotherPage";
		const string link2 = "YetAnotherPage";
		var revision1 = new WikiPage { PageName = pageName, Revision = 1, Markup = $"[{link1}]", IsDeleted = true, Author = author, AuthorId = author.Id };
		var revision2 = new WikiPage { PageName = pageName, Revision = 2, Markup = $"[{link2}]", IsDeleted = true, Author = author, AuthorId = author.Id };
		_db.WikiPages.Add(revision1);
		_db.WikiPages.Add(revision2);
		await _db.SaveChangesAsync();

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
		Assert.AreEqual(1, _cache.PageCache().Count);
		var cached = _cache.PageCache().Single();
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
		const string pageName = "Deleted";
		const string link = "AnotherPage";
		var page = new WikiPage { PageName = pageName, Markup = $"[{link}]", IsDeleted = true };
		_db.WikiPages.Add(page);
		await _db.SaveChangesAsync();
		_db.CreateConcurrentUpdateConflict();

		var actual = await _wikiPages.Undelete(pageName);

		Assert.IsFalse(actual);
		Assert.AreEqual(0, _db.WikiPages.ThatAreNotDeleted().Count());
		Assert.AreEqual(0, _cache.PageCache().Count);
		Assert.AreEqual(0, _db.WikiReferrals.Count());
	}

	[TestMethod]
	public async Task Undelete_TrimsSlashes()
	{
		var author = _db.AddUser(1, "_").Entity;
		const string pageName = "Deleted";
		const string link = "AnotherPage";
		var page = new WikiPage { PageName = pageName, Markup = $"[{link}]", IsDeleted = true, Author = author, AuthorId = author.Id };
		_db.WikiPages.Add(page);
		await _db.SaveChangesAsync();

		var actual = await _wikiPages.Undelete("/" + pageName + "/");
		Assert.IsTrue(actual);
		Assert.AreEqual(1, _db.WikiPages.ThatAreNotDeleted().Count());
		Assert.AreEqual(1, _cache.PageCache().Count);
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
		const string suffix = "Exists";
		const string systemPageName = "System/" + suffix;
		var page = new WikiPage { PageName = systemPageName };
		_db.WikiPages.Add(page);
		await _db.SaveChangesAsync();

		var actual = await _wikiPages.SystemPage(suffix);
		Assert.IsNotNull(actual);
		Assert.AreEqual(systemPageName, actual.PageName);
	}

	[TestMethod]
	public async Task SystemPage_DoesNotExists_ReturnsNull()
	{
		const string suffix = "Exists";
		const string systemPageName = "System/" + suffix;
		var page = new WikiPage { PageName = systemPageName };
		_db.WikiPages.Add(page);
		await _db.SaveChangesAsync();

		var actual = await _wikiPages.SystemPage("Does not exist");
		Assert.IsNull(actual);
	}

	[TestMethod]
	public async Task SystemPage_Empty_ReturnsSystem()
	{
		var page = new WikiPage { PageName = "System" };
		_db.WikiPages.Add(page);
		await _db.SaveChangesAsync();

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
		Assert.AreEqual(0, actual.Count);
	}

	[TestMethod]
	public async Task Orphans_DeletedPage_NotAnOrphan()
	{
		AddPage("Deleted", true);
		var actual = await _wikiPages.Orphans();

		Assert.IsNotNull(actual);
		Assert.AreEqual(0, actual.Count);
	}

	[TestMethod]
	public async Task Orphans_NoOrphans_ReturnsEmptyList()
	{
		// Two pages, that properly link each other
		const string parent = "Parent";
		const string child = "Child";
		AddPage(parent);
		AddPage(child);
		_db.WikiReferrals.Add(new WikiPageReferral { Referrer = parent, Referral = child });
		_db.WikiReferrals.Add(new WikiPageReferral { Referrer = child, Referral = parent });
		await _db.SaveChangesAsync();

		var actual = await _wikiPages.Orphans();

		Assert.IsNotNull(actual);
		Assert.AreEqual(0, actual.Count);
	}

	[TestMethod]
	public async Task Orphans_PageWithNoReferrers_ReturnsAsOrphan()
	{
		const string orphan = "Orphan";
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
		const string orphan = "Orphan";
		AddPage(orphan);
		_db.WikiReferrals.Add(new WikiPageReferral { Referrer = "Parent", Referral = "Not" + orphan });
		await _db.SaveChangesAsync();

		var actual = await _wikiPages.Orphans();

		Assert.IsNotNull(actual);
		var orphans = actual.ToList();
		Assert.AreEqual(1, orphans.Count);
		Assert.AreEqual(orphan, orphans.Single().PageName);
	}

	[TestMethod]
	public async Task Orphans_Subpages_NotConsideredOrphans()
	{
		const string parent = "Parent";
		AddPage(parent);
		AddPage(parent + "/Child");

		var actual = await _wikiPages.Orphans();

		// Parent should be an orphan but not child
		Assert.IsNotNull(actual);
		var orphans = actual.ToList();
		Assert.AreEqual(1, orphans.Count);
		Assert.AreEqual(parent, orphans.Single().PageName);
	}

	[TestMethod]
	[DataRow("MediaPosts")]
	[DataRow("InternalSystem")]
	public async Task Orphans_CorePages_NotConsideredOrphans(string page)
	{
		AddPage(page);
		var actual = await _wikiPages.Orphans();

		Assert.IsNotNull(actual);
		Assert.AreEqual(0, actual.Count);
	}

	#endregion

	#region Broken Links

	[TestMethod]
	public async Task BrokenLinks_NoReferrers_ReturnsEmptyList()
	{
		var actual = await _wikiPages.BrokenLinks();
		Assert.IsNotNull(actual);
		Assert.AreEqual(0, actual.Count);
	}

	[TestMethod]
	public async Task BrokenLinks_DoesNotConsiderLinksFromSandBox()
	{
		_db.WikiReferrals.Add(new WikiPageReferral { Referral = "DoesNotExist", Referrer = "SandBox" });
		await _db.SaveChangesAsync();

		var actual = await _wikiPages.BrokenLinks();

		Assert.IsNotNull(actual);
		Assert.AreEqual(0, actual.Count);
	}

	[TestMethod]
	public async Task BrokenLinks_NoBrokenLinks_ReturnsEmptyList()
	{
		const string page = "Parent";
		AddPage(page);
		_db.WikiReferrals.Add(new WikiPageReferral { Referral = "Parent", Referrer = "AnotherPage" });
		await _db.SaveChangesAsync();

		var actual = await _wikiPages.BrokenLinks();

		Assert.IsNotNull(actual);
		Assert.AreEqual(0, actual.Count);
	}

	[TestMethod]
	public async Task BrokenLinks_BrokenLink_ReturnsBrokenLink()
	{
		const string page = "PageWithLink";
		const string doesNotExist = "DoesNotExist";
		AddPage(page);
		_db.WikiReferrals.Add(new WikiPageReferral
		{
			Referrer = page,
			Referral = doesNotExist
		});
		await _db.SaveChangesAsync();

		var actual = await _wikiPages.BrokenLinks();

		Assert.IsNotNull(actual);
		var brokenLinks = actual.ToList();
		Assert.AreEqual(1, brokenLinks.Count);
		var brokenLink = brokenLinks.Single();
		Assert.AreEqual(page, brokenLink.Referrer);
		Assert.AreEqual(doesNotExist, brokenLink.Referral);
	}

	[TestMethod]
	[DataRow("Subs-")]
	[DataRow("Movies-")]
	public async Task BrokenLinks_CorePages_NotConsideredBrokenLinks(string referral)
	{
		_db.WikiReferrals.Add(new WikiPageReferral { Referrer = "Page", Referral = referral });
		var actual = await _wikiPages.BrokenLinks();

		Assert.IsNotNull(actual);
		Assert.AreEqual(0, actual.Count);
	}

	#endregion

	private int AddPage(string name, bool isDeleted = false, bool cache = false)
	{
		User author = _db.Users.FirstOrDefault()
			?? new User
			{
				Id = 1,
				UserName = $"Test User from {nameof(AddPage)}",
				NormalizedUserName = $"Test User from {nameof(AddPage)}",
				Email = $"Test User from {nameof(AddPage)}",
				NormalizedEmail = $"Test User from {nameof(AddPage)}"
			};
		var maxRevision = _db.WikiPages
			.ForPage(name)
			.Max(r => (int?)r.Revision) ?? 0;
		var wp = new WikiPage { PageName = name, IsDeleted = isDeleted, AuthorId = author.Id, Author = author, Revision = maxRevision + 1 };
		_db.Add(wp);
		_db.SaveChanges();

		if (cache)
		{
			_cache.AddPage(wp);
		}

		return wp.Id;
	}
}
