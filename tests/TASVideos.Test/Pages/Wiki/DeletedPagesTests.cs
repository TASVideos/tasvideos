using System.Reflection;
using TASVideos.Core;
using TASVideos.Core.Services.ExternalMediaPublisher;
using TASVideos.Core.Services.Wiki;
using TASVideos.Data.Entity;
using TASVideos.Pages;
using TASVideos.Pages.Wiki;
using TASVideos.Services;
using TASVideos.Tests.Base;
using static TASVideos.RazorPages.Tests.RazorTestHelpers;

namespace TASVideos.RazorPages.Tests.Pages.Wiki;

[TestClass]
public class DeletedPagesTests : TestDbBase
{
	private readonly IWikiPages _wikiPages;
	private readonly DeletedPagesModel _page;
	private readonly IExternalMediaPublisher _publisher;

	public DeletedPagesTests()
	{
		_wikiPages = Substitute.For<IWikiPages>();
		_publisher = Substitute.For<IExternalMediaPublisher>();
		_page = new DeletedPagesModel(_wikiPages, _db, _publisher);
	}

	[TestMethod]
	public void RequiresPermission()
	{
		var attribute = typeof(DeletedPagesModel).GetCustomAttribute(typeof(RequirePermissionAttribute));
		Assert.IsNotNull(attribute);
		Assert.IsInstanceOfType<RequirePermissionAttribute>(attribute);
		var permissionAttribute = (RequirePermissionAttribute)attribute;
		Assert.IsTrue(permissionAttribute.RequiredPermissions.Contains(PermissionTo.SeeDeletedWikiPages));
	}

	[TestMethod]
	public async Task OnGet()
	{
		const string partiallyDeleted = "PageWithDeletedRevision";
		const string fullyDeleted = "PageFullyDeleted";
		_db.WikiPages.Add(new WikiPage { PageName = "ExistingPage", Revision = 1 });
		_db.WikiPages.Add(new WikiPage { PageName = partiallyDeleted, IsDeleted = false, Revision = 1 });
		_db.WikiPages.Add(new WikiPage { PageName = partiallyDeleted, IsDeleted = true, Revision = 2 });
		_db.WikiPages.Add(new WikiPage { PageName = fullyDeleted, IsDeleted = true, Revision = 1 });
		_db.WikiPages.Add(new WikiPage { PageName = fullyDeleted, IsDeleted = true, Revision = 2 });
		await _db.SaveChangesAsync();

		await _page.OnGet();

		Assert.AreEqual(2, _page.DeletedPages.Count);
		Assert.IsTrue(_page.DeletedPages.Any(p => p.PageName == partiallyDeleted));
		var partiallyDeletedResult = _page.DeletedPages.Single(p => p.PageName == partiallyDeleted);
		Assert.IsTrue(partiallyDeletedResult.HasExistingRevisions);
		Assert.AreEqual(1, partiallyDeletedResult.RevisionCount);

		Assert.IsTrue(_page.DeletedPages.Any(p => p.PageName == fullyDeleted));
		var fullyDeletedResult = _page.DeletedPages.Single(p => p.PageName == fullyDeleted);
		Assert.IsFalse(fullyDeletedResult.HasExistingRevisions);
		Assert.AreEqual(2, fullyDeletedResult.RevisionCount);
	}

	[TestMethod]
	public async Task OnPostDeletePage_RequiresPermission()
	{
		var actual = await _page.OnPostDeletePage("", "");
		Assert.IsNotNull(actual);
		Assert.IsInstanceOfType<RedirectToPageResult>(actual);
		var redirectResult = (RedirectToPageResult)actual;
		Assert.AreEqual("/Account/AccessDenied", redirectResult.PageName);
	}

	[TestMethod]
	public async Task OnPostDeletePage_NoPage_ModelError()
	{
		var user = new User { Id = 1, UserName = "User" };
		AddAuthenticatedUser(_page, user, [PermissionTo.DeleteWikiPages]);

		var actual = await _page.OnPostDeletePage("", "");

		Assert.IsInstanceOfType<PageResult>(actual);
		Assert.IsFalse(_page.ModelState.IsValid);
	}

	[TestMethod]
	public async Task OnPostDelete_DeleteFails_ModelError()
	{
		const string pageName = "TestPage";
		var user = new User { Id = 1, UserName = "User" };
		AddAuthenticatedUser(_page, user, [PermissionTo.DeleteWikiPages]);
		_wikiPages.Delete(pageName).Returns(-1);
		_wikiPages.Page(pageName).Returns(Substitute.For<IWikiPage>());

		var actual = await _page.OnPostDeletePage(pageName, "");

		Assert.IsInstanceOfType<PageResult>(actual);
		Assert.IsFalse(_page.ModelState.IsValid);
	}

	[TestMethod]
	public async Task OnPostDeletePage_PageNotFound_ModelError()
	{
		const string pageName = "TestPage";
		var user = new User { Id = 1, UserName = "User" };
		AddAuthenticatedUser(_page, user, [PermissionTo.DeleteWikiPages]);
		_wikiPages.Page(pageName).Returns((IWikiPage?)null);

		var actual = await _page.OnPostDeletePage(pageName, "");

		Assert.IsInstanceOfType<PageResult>(actual);
		Assert.IsFalse(_page.ModelState.IsValid);
	}

	[TestMethod]
	public async Task OnPostDeletePage_Path_NoReason_Deletes()
	{
		const string pageName = "TestPage";
		var user = new User { Id = 1, UserName = "User" };
		AddAuthenticatedUser(_page, user, [PermissionTo.DeleteWikiPages]);
		_wikiPages.Delete(pageName).Returns(1);
		_wikiPages.Page(pageName).Returns(Substitute.For<IWikiPage>());

		var actual = await _page.OnPostDeletePage(pageName, "");
		Assert.IsInstanceOfType<RedirectToPageResult>(actual);
		var redirectResult = (RedirectToPageResult)actual;
		Assert.AreEqual("DeletedPages", redirectResult.PageName);
		await _publisher.Received(1).Send(Arg.Any<Post>());
	}

	[TestMethod]
	public async Task OnPostDeleteRevision_RequiresPermission()
	{
		var actual = await _page.OnPostDeletePage("", "");
		Assert.IsNotNull(actual);
		Assert.IsInstanceOfType<RedirectToPageResult>(actual);
		var redirectResult = (RedirectToPageResult)actual;
		Assert.AreEqual("/Account/AccessDenied", redirectResult.PageName);
	}

	[TestMethod]
	public async Task OnPostDeleteRevision_NoPath_ReturnsHome()
	{
		var user = new User { Id = 1, UserName = "User" };
		AddAuthenticatedUser(_page, user, [PermissionTo.DeleteWikiPages]);

		var actual = await _page.OnPostDeleteRevision("", 1);

		Assert.IsInstanceOfType<RedirectToPageResult>(actual);
		var redirectResult = (RedirectToPageResult)actual;
		Assert.AreEqual("/Index", redirectResult.PageName);
	}

	[TestMethod]
	public async Task OnPostDeleteRevision_NoRevision_ReturnsHome()
	{
		var user = new User { Id = 1, UserName = "User" };
		AddAuthenticatedUser(_page, user, [PermissionTo.DeleteWikiPages]);

		var actual = await _page.OnPostDeleteRevision("Page", 0);

		Assert.IsInstanceOfType<RedirectToPageResult>(actual);
		var redirectResult = (RedirectToPageResult)actual;
		Assert.AreEqual("/Index", redirectResult.PageName);
	}

	[TestMethod]
	public async Task OnPostDeleteRevision_DeletesRevisionAndRedirects()
	{
		const string pageName = "TestPage";
		var user = new User { Id = 1, UserName = "User" };
		AddAuthenticatedUser(_page, user, [PermissionTo.DeleteWikiPages]);

		var actual = await _page.OnPostDeleteRevision(pageName, 1);

		await _wikiPages.Received(1).Delete(pageName, 1);
		await _publisher.Received(1).Send(Arg.Any<Post>());
		Assert.IsInstanceOfType<RedirectResult>(actual);
		var redirectResult = (RedirectResult)actual;
		Assert.AreEqual("/" + pageName, redirectResult.Url);
	}

	[TestMethod]
	public async Task OnPostDeleteRevision_Trims()
	{
		const string pageName = "TestPage";
		var user = new User { Id = 1, UserName = "User" };
		AddAuthenticatedUser(_page, user, [PermissionTo.DeleteWikiPages]);

		var actual = await _page.OnPostDeleteRevision("/" + pageName, 1);

		await _wikiPages.Received(1).Delete(pageName, 1);
		await _publisher.Received(1).Send(Arg.Any<Post>());
		Assert.IsInstanceOfType<RedirectResult>(actual);
		var redirectResult = (RedirectResult)actual;
		Assert.AreEqual("/" + pageName, redirectResult.Url);
	}

	[TestMethod]
	public async Task OnPostUndelete_RequiresPermission()
	{
		var actual = await _page.OnPostUndelete("");
		Assert.IsNotNull(actual);
		Assert.IsInstanceOfType<RedirectToPageResult>(actual);
		var redirectResult = (RedirectToPageResult)actual;
		Assert.AreEqual("/Account/AccessDenied", redirectResult.PageName);
	}

	[TestMethod]
	public async Task OnPostUndeleteRevision_NoPath_ReturnsHome()
	{
		var user = new User { Id = 1, UserName = "User" };
		AddAuthenticatedUser(_page, user, [PermissionTo.DeleteWikiPages]);

		var actual = await _page.OnPostUndelete("");

		Assert.IsInstanceOfType<RedirectToPageResult>(actual);
		var redirectResult = (RedirectToPageResult)actual;
		Assert.AreEqual("/Index", redirectResult.PageName);
	}

	[TestMethod]
	public async Task OnPostUndeleteRevision_UndeleteFails_ModelError()
	{
		const string pageName = "TestPage";
		var user = new User { Id = 1, UserName = "User" };
		AddAuthenticatedUser(_page, user, [PermissionTo.DeleteWikiPages]);
		_wikiPages.Undelete(pageName).Returns(false);

		var actual = await _page.OnPostUndelete(pageName);

		Assert.IsInstanceOfType<PageResult>(actual);
		Assert.IsFalse(_page.ModelState.IsValid);
	}

	[TestMethod]
	public async Task OnPostUndeleteRevision_UndeletesAndRedirects()
	{
		const string pageName = "TestPage";
		var user = new User { Id = 1, UserName = "User" };
		AddAuthenticatedUser(_page, user, [PermissionTo.DeleteWikiPages]);
		_wikiPages.Undelete(pageName).Returns(true);

		var actual = await _page.OnPostUndelete(pageName);

		await _wikiPages.Received(1).Undelete(pageName);
		await _publisher.Received(1).Send(Arg.Any<Post>());
		Assert.IsInstanceOfType<RedirectResult>(actual);
		var redirectResult = (RedirectResult)actual;
		Assert.AreEqual("/" + pageName, redirectResult.Url);
	}

	[TestMethod]
	public async Task OnPostUndeleteRevision_Trims()
	{
		const string pageName = "TestPage";
		var user = new User { Id = 1, UserName = "User" };
		AddAuthenticatedUser(_page, user, [PermissionTo.DeleteWikiPages]);
		_wikiPages.Undelete(pageName).Returns(true);

		var actual = await _page.OnPostUndelete("/" + pageName);

		await _wikiPages.Received(1).Undelete(pageName);
		await _publisher.Received(1).Send(Arg.Any<Post>());
		Assert.IsInstanceOfType<RedirectResult>(actual);
		var redirectResult = (RedirectResult)actual;
		Assert.AreEqual("/" + pageName, redirectResult.Url);
	}
}
