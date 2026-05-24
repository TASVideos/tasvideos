using TASVideos.Data.Entity;

namespace TASVideos.Core.Tests.Services;

[TestClass]
public class WikiRedirectServiceTests : TestDbBase
{
	private readonly WikiRedirectService _wikiRedirectService;

	public WikiRedirectServiceTests()
	{
		_wikiRedirectService = new WikiRedirectService(_db);
	}

	[TestMethod]
	public async Task GetAll_EmptyDb_ReturnsEmpty()
	{
		var result = await _wikiRedirectService.GetAll();
		Assert.IsNotNull(result);
		Assert.IsEmpty(result);
	}

	[TestMethod]
	public async Task GetAll_ReturnsAll()
	{
		_db.WikiRedirects.Add(new WikiRedirect { PageNameFrom = "APage", PageNameTo = "BPage" });
		_db.WikiRedirects.Add(new WikiRedirect { PageNameFrom = "CPage", PageNameTo = "DPage" });
		await _db.SaveChangesAsync();

		var result = await _wikiRedirectService.GetAll();
		Assert.IsNotNull(result);
		Assert.HasCount(2, result);
	}

	[TestMethod]
	public async Task GetAll_OrdersByPageNameFrom()
	{
		_db.WikiRedirects.Add(new WikiRedirect { PageNameFrom = "ZPage", PageNameTo = "BPage" });
		_db.WikiRedirects.Add(new WikiRedirect { PageNameFrom = "APage", PageNameTo = "DPage" });
		await _db.SaveChangesAsync();

		var result = await _wikiRedirectService.GetAll();
		Assert.IsNotNull(result);
		Assert.AreEqual("APage", result.First().PageNameFrom);
		Assert.AreEqual("ZPage", result.Last().PageNameFrom);
	}

	[TestMethod]
	public async Task GetById_DoesNotExist_ReturnsNull()
	{
		var result = await _wikiRedirectService.GetById(-1);
		Assert.IsNull(result);
	}

	[TestMethod]
	public async Task GetById_Exists_ReturnsRedirect()
	{
		var redirect = new WikiRedirect { PageNameFrom = "APage", PageNameTo = "BPage" };
		_db.WikiRedirects.Add(redirect);
		await _db.SaveChangesAsync();

		var result = await _wikiRedirectService.GetById(redirect.Id);
		Assert.IsNotNull(result);
		Assert.AreEqual("APage", result.PageNameFrom);
		Assert.AreEqual("BPage", result.PageNameTo);
	}

	[TestMethod]
	public async Task Add_Success_AddsRedirect()
	{
		var redirect = new WikiRedirect { PageNameFrom = "APage", PageNameTo = "BPage" };

		var result = await _wikiRedirectService.Add(redirect);

		Assert.AreEqual(WikiRedirectAddEditResult.Success, result);
		Assert.AreEqual(1, _db.WikiRedirects.Count());
	}

	[TestMethod]
	public async Task Add_InvalidPageNameFrom_ReturnsInvalidPageNameFrom()
	{
		var redirect = new WikiRedirect { PageNameFrom = "invalid page", PageNameTo = "BPage" };

		var result = await _wikiRedirectService.Add(redirect);

		Assert.AreEqual(WikiRedirectAddEditResult.InvalidPageNameFrom, result);
		Assert.AreEqual(0, _db.WikiRedirects.Count());
	}

	[TestMethod]
	public async Task Add_InvalidPageNameTo_ReturnsInvalidPageNameTo()
	{
		var redirect = new WikiRedirect { PageNameFrom = "APage", PageNameTo = "invalid page" };

		var result = await _wikiRedirectService.Add(redirect);

		Assert.AreEqual(WikiRedirectAddEditResult.InvalidPageNameTo, result);
		Assert.AreEqual(0, _db.WikiRedirects.Count());
	}

	[TestMethod]
	public async Task Add_SamePageName_ReturnsSamePageName()
	{
		var redirect = new WikiRedirect { PageNameFrom = "APage", PageNameTo = "APage" };

		var result = await _wikiRedirectService.Add(redirect);

		Assert.AreEqual(WikiRedirectAddEditResult.SamePageName, result);
		Assert.AreEqual(0, _db.WikiRedirects.Count());
	}

	[TestMethod]
	public async Task Add_ChainedRedirectTo_ReturnsChainedRedirectTo()
	{
		// Existing: BPage -> CPage. Adding APage -> BPage would chain: APage -> BPage -> CPage
		_db.WikiRedirects.Add(new WikiRedirect { PageNameFrom = "BPage", PageNameTo = "CPage" });
		await _db.SaveChangesAsync();

		var redirect = new WikiRedirect { PageNameFrom = "APage", PageNameTo = "BPage" };
		var result = await _wikiRedirectService.Add(redirect);

		Assert.AreEqual(WikiRedirectAddEditResult.ChainedRedirectTo, result);
	}

	[TestMethod]
	public async Task Add_ChainedRedirectFrom_ReturnsChainedRedirectFrom()
	{
		// Existing: APage -> BPage. Adding BPage -> CPage would chain: APage -> BPage -> CPage
		_db.WikiRedirects.Add(new WikiRedirect { PageNameFrom = "APage", PageNameTo = "BPage" });
		await _db.SaveChangesAsync();

		var redirect = new WikiRedirect { PageNameFrom = "BPage", PageNameTo = "CPage" };
		var result = await _wikiRedirectService.Add(redirect);

		Assert.AreEqual(WikiRedirectAddEditResult.ChainedRedirectFrom, result);
	}

	[TestMethod]
	public async Task Edit_NotFound_ReturnsNotFound()
	{
		var redirect = new WikiRedirect { PageNameFrom = "APage", PageNameTo = "BPage" };

		var result = await _wikiRedirectService.Edit(-1, redirect);

		Assert.AreEqual(WikiRedirectAddEditResult.NotFound, result);
	}

	[TestMethod]
	public async Task Edit_Success_UpdatesRedirect()
	{
		var existing = new WikiRedirect { PageNameFrom = "APage", PageNameTo = "BPage" };
		_db.WikiRedirects.Add(existing);
		await _db.SaveChangesAsync();

		var updated = new WikiRedirect { PageNameFrom = "APage", PageNameTo = "CPage" };
		var result = await _wikiRedirectService.Edit(existing.Id, updated);

		Assert.AreEqual(WikiRedirectAddEditResult.Success, result);
		var inDb = _db.WikiRedirects.Single();
		Assert.AreEqual("CPage", inDb.PageNameTo);
	}

	[TestMethod]
	public async Task Edit_InvalidPageNameFrom_ReturnsInvalidPageNameFrom()
	{
		var existing = new WikiRedirect { PageNameFrom = "APage", PageNameTo = "BPage" };
		_db.WikiRedirects.Add(existing);
		await _db.SaveChangesAsync();

		var updated = new WikiRedirect { PageNameFrom = "invalid page", PageNameTo = "BPage" };
		var result = await _wikiRedirectService.Edit(existing.Id, updated);

		Assert.AreEqual(WikiRedirectAddEditResult.InvalidPageNameFrom, result);
	}

	[TestMethod]
	public async Task Edit_InvalidPageNameTo_ReturnsInvalidPageNameTo()
	{
		var existing = new WikiRedirect { PageNameFrom = "APage", PageNameTo = "BPage" };
		_db.WikiRedirects.Add(existing);
		await _db.SaveChangesAsync();

		var updated = new WikiRedirect { PageNameFrom = "APage", PageNameTo = "invalid page" };
		var result = await _wikiRedirectService.Edit(existing.Id, updated);

		Assert.AreEqual(WikiRedirectAddEditResult.InvalidPageNameTo, result);
	}

	[TestMethod]
	public async Task Edit_SamePageName_ReturnsSamePageName()
	{
		var existing = new WikiRedirect { PageNameFrom = "APage", PageNameTo = "BPage" };
		_db.WikiRedirects.Add(existing);
		await _db.SaveChangesAsync();

		var updated = new WikiRedirect { PageNameFrom = "APage", PageNameTo = "APage" };
		var result = await _wikiRedirectService.Edit(existing.Id, updated);

		Assert.AreEqual(WikiRedirectAddEditResult.SamePageName, result);
	}

	[TestMethod]
	public async Task Edit_ChainedRedirectTo_ReturnsChainedRedirectTo()
	{
		// Existing redirects: APage -> XPage, BPage -> CPage
		// Editing APage -> XPage to APage -> BPage would chain: APage -> BPage -> CPage
		var existing = new WikiRedirect { PageNameFrom = "APage", PageNameTo = "XPage" };
		_db.WikiRedirects.Add(existing);
		_db.WikiRedirects.Add(new WikiRedirect { PageNameFrom = "BPage", PageNameTo = "CPage" });
		await _db.SaveChangesAsync();

		var updated = new WikiRedirect { PageNameFrom = "APage", PageNameTo = "BPage" };
		var result = await _wikiRedirectService.Edit(existing.Id, updated);

		Assert.AreEqual(WikiRedirectAddEditResult.ChainedRedirectTo, result);
	}

	[TestMethod]
	public async Task Edit_ChainedRedirectFrom_ReturnsChainedRedirectFrom()
	{
		// Existing redirects: APage -> BPage, XPage -> YPage
		// Editing XPage -> YPage to BPage -> YPage would chain: APage -> BPage -> YPage
		var existing = new WikiRedirect { PageNameFrom = "XPage", PageNameTo = "YPage" };
		_db.WikiRedirects.Add(existing);
		_db.WikiRedirects.Add(new WikiRedirect { PageNameFrom = "APage", PageNameTo = "BPage" });
		await _db.SaveChangesAsync();

		var updated = new WikiRedirect { PageNameFrom = "BPage", PageNameTo = "YPage" };
		var result = await _wikiRedirectService.Edit(existing.Id, updated);

		Assert.AreEqual(WikiRedirectAddEditResult.ChainedRedirectFrom, result);
	}

	[TestMethod]
	public async Task Delete_NotFound_ReturnsNotFound()
	{
		var result = await _wikiRedirectService.Delete(-1);

		Assert.AreEqual(WikiRedirectDeleteResult.NotFound, result);
	}

	[TestMethod]
	public async Task Delete_Success_RemovesRedirect()
	{
		var existing = new WikiRedirect { PageNameFrom = "APage", PageNameTo = "BPage" };
		_db.WikiRedirects.Add(existing);
		await _db.SaveChangesAsync();

		var result = await _wikiRedirectService.Delete(existing.Id);

		Assert.AreEqual(WikiRedirectDeleteResult.Success, result);
		Assert.AreEqual(0, _db.WikiRedirects.Count());
	}
}
