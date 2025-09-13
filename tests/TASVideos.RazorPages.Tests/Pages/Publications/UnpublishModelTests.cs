using TASVideos.Core.Services;
using TASVideos.Core.Services.ExternalMediaPublisher;
using TASVideos.Pages.Publications;
using TASVideos.Services;
using TASVideos.Tests.Base;

namespace TASVideos.RazorPages.Tests.Pages.Publications;

[TestClass]
public class UnpublishModelTests : TestDbBase
{
	private readonly IExternalMediaPublisher _publisher;
	private readonly IPublications _publications;
	private readonly UnpublishModel _page;

	public UnpublishModelTests()
	{
		_publisher = Substitute.For<IExternalMediaPublisher>();
		_publications = Substitute.For<IPublications>();
		_page = new UnpublishModel(_publisher, _publications);
	}

	[TestMethod]
	public async Task OnGet_PublicationNotFound_ReturnsNotFound()
	{
		_page.Id = 999;
		_publications.CanUnpublish(999).Returns(new UnpublishResult(UnpublishResult.UnpublishStatus.NotFound, "", ""));

		var result = await _page.OnGet();

		Assert.IsInstanceOfType<NotFoundResult>(result);
		await _publications.Received(1).CanUnpublish(999);
	}

	[TestMethod]
	public async Task OnGet_UnpublishNotAllowed_ReturnsBadRequest()
	{
		_page.Id = 123;
		_publications.CanUnpublish(123).Returns(new UnpublishResult(UnpublishResult.UnpublishStatus.NotAllowed, "Test Publication", "Test Error"));

		var result = await _page.OnGet();

		Assert.IsInstanceOfType<BadRequestObjectResult>(result);
		await _publications.Received(1).CanUnpublish(123);
	}

	[TestMethod]
	public async Task OnGet_ValidPublication_ReturnsPageWithTitle()
	{
		_page.Id = 456;
		const string publicationTitle = "Super Mario Bros.";
		_publications.CanUnpublish(456).Returns(new UnpublishResult(UnpublishResult.UnpublishStatus.Success, publicationTitle, ""));

		var result = await _page.OnGet();

		Assert.IsInstanceOfType<PageResult>(result);
		Assert.AreEqual(publicationTitle, _page.Title);
		await _publications.Received(1).CanUnpublish(456);
	}

	[TestMethod]
	public async Task OnPost_InvalidModelState_ReturnsPage()
	{
		_page.Id = 123;
		_page.ModelState.AddModelError("Reason", "Required");

		var result = await _page.OnPost();

		Assert.IsInstanceOfType<PageResult>(result);
	}

	[TestMethod]
	public async Task OnPost_PublicationNotFound_RedirectsWithErrorMessage()
	{
		var user = _db.AddUser("TestUser").Entity;
		AddAuthenticatedUser(_page, user, [PermissionTo.Unpublish]);

		_page.Id = 999;
		_page.Reason = "Test reason";
		_publications.Unpublish(999).Returns(new UnpublishResult(UnpublishResult.UnpublishStatus.NotFound, "", ""));

		var result = await _page.OnPost();

		Assert.IsInstanceOfType<RedirectToPageResult>(result);
		var redirect = (RedirectToPageResult)result;
		Assert.AreEqual("View", redirect.PageName);
		Assert.AreEqual(999, redirect.RouteValues!["Id"]);
		Assert.AreEqual("Publication 999 not found", _page.Message);
		await _publications.Received(1).Unpublish(999);
	}

	[TestMethod]
	public async Task OnPost_UnpublishNotAllowed_RedirectsWithErrorMessage()
	{
		var user = _db.AddUser("TestUser").Entity;
		AddAuthenticatedUser(_page, user, [PermissionTo.Unpublish]);

		_page.Id = 123;
		_page.Reason = "Test reason";
		const string errorMessage = "Cannot unpublish a publication that has awards";
		_publications.Unpublish(123).Returns(new UnpublishResult(UnpublishResult.UnpublishStatus.NotAllowed, "Test Publication", errorMessage));

		var result = await _page.OnPost();

		Assert.IsInstanceOfType<RedirectToPageResult>(result);
		var redirect = (RedirectToPageResult)result;
		Assert.AreEqual("View", redirect.PageName);
		Assert.AreEqual(123, redirect.RouteValues!["Id"]);
		Assert.AreEqual(errorMessage, _page.Message);
		await _publications.Received(1).Unpublish(123);
	}

	[TestMethod]
	public async Task OnPost_SuccessfulUnpublish_AnnouncesAndRedirectsToSubsList()
	{
		var user = _db.AddUser("TestUser").Entity;
		AddAuthenticatedUser(_page, user, [PermissionTo.Unpublish]);

		_page.Id = 456;
		_page.Reason = "Obsolete due to improved version";
		const string publicationTitle = "Super Mario Bros.";
		_publications.Unpublish(456).Returns(new UnpublishResult(UnpublishResult.UnpublishStatus.Success, publicationTitle, ""));

		var result = await _page.OnPost();

		Assert.IsInstanceOfType<RedirectResult>(result);
		var redirect = (RedirectResult)result;
		Assert.AreEqual("/Subs-List", redirect.Url);
		await _publications.Received(1).Unpublish(456);
		await _publisher.Received(1).Send(Arg.Any<Post>());
	}

	[TestMethod]
	public async Task OnPost_SuccessfulUnpublishWithEmptyReason_AnnouncesWithEmptyString()
	{
		var user = _db.AddUser("TestUser").Entity;
		AddAuthenticatedUser(_page, user, [PermissionTo.Unpublish]);

		_page.Id = 789;
		_page.Reason = "";
		_publications.Unpublish(789).Returns(new UnpublishResult(UnpublishResult.UnpublishStatus.Success, "SMB", ""));

		var result = await _page.OnPost();

		Assert.IsInstanceOfType<RedirectResult>(result);
		var redirect = (RedirectResult)result;
		Assert.AreEqual("/Subs-List", redirect.Url);
		await _publications.Received(1).Unpublish(789);
		await _publisher.Received(1).Send(Arg.Any<Post>());
	}

	[TestMethod]
	public async Task OnPost_SuccessfulUnpublishWithLongReason_AnnouncesWithFullReason()
	{
		var user = _db.AddUser("TestUser").Entity;
		AddAuthenticatedUser(_page, user, [PermissionTo.Unpublish]);
		_page.Id = 321;
		_page.Reason = "This publication has been removed due to a significant improvement in the TAS techniques that makes this obsolete. The new version provides better entertainment value.";
		_publications.Unpublish(321).Returns(new UnpublishResult(UnpublishResult.UnpublishStatus.Success, "SMB", ""));

		var result = await _page.OnPost();

		Assert.IsInstanceOfType<RedirectResult>(result);
		var redirect = (RedirectResult)result;
		Assert.AreEqual("/Subs-List", redirect.Url);
		await _publications.Received(1).Unpublish(321);
		await _publisher.Received(1).Send(Arg.Any<Post>());
	}

	[TestMethod]
	public void RequiresPermission() => AssertHasPermission(typeof(UnpublishModel), PermissionTo.Unpublish);
}
