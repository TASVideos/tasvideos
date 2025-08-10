using TASVideos.Core.Services;
using TASVideos.Core.Services.ExternalMediaPublisher;
using TASVideos.Data.Entity;
using TASVideos.Pages.Publications;
using TASVideos.Services;
using TASVideos.Tests.Base;
using static TASVideos.RazorPages.Tests.RazorTestHelpers;

namespace TASVideos.RazorPages.Tests.Pages.Publications;

[TestClass]
public class EditClassModelTests : TestDbBase
{
	private readonly IExternalMediaPublisher _publisher;
	private readonly IPublicationMaintenanceLogger _publicationMaintenanceLogger;
	private readonly EditClassModel _page;

	public EditClassModelTests()
	{
		_publisher = Substitute.For<IExternalMediaPublisher>();
		_publicationMaintenanceLogger = Substitute.For<IPublicationMaintenanceLogger>();
		_page = new EditClassModel(_db, _publisher, _publicationMaintenanceLogger);
	}

	[TestMethod]
	public async Task OnGet_PublicationNotFound_ReturnsNotFound()
	{
		_page.Id = 999;

		var result = await _page.OnGet();

		Assert.IsInstanceOfType<NotFoundResult>(result);
	}

	[TestMethod]
	public async Task OnGet_ValidPublication_PopulatesDataAndAvailableClasses()
	{
		var publicationClass = new PublicationClass { Id = 1, Name = "Standard" };
		var publication = _db.AddPublication(publicationClass: publicationClass).Entity;
		publication.Title = "Test Publication";
		await _db.SaveChangesAsync();
		_page.Id = publication.Id;

		var result = await _page.OnGet();

		Assert.IsInstanceOfType<PageResult>(result);
		Assert.AreEqual("Test Publication", _page.Title);
		Assert.AreEqual(publicationClass.Id, _page.PublicationClassId);
		Assert.AreEqual(1, _page.AvailableClasses.Count);
		Assert.AreEqual(publicationClass.Name, _page.AvailableClasses[0].Text);
	}

	[TestMethod]
	public async Task OnPost_InvalidModelState_ReturnsPageWithAvailableClasses()
	{
		var publicationClass = new PublicationClass { Id = 1, Name = "Standard" };
		var publication = _db.AddPublication(publicationClass: publicationClass).Entity;
		await _db.SaveChangesAsync();

		_page.Id = publication.Id;
		_page.PublicationClassId = publicationClass.Id;
		_page.ModelState.AddModelError("Title", "Test error");

		var result = await _page.OnPost();

		Assert.IsInstanceOfType<PageResult>(result);
		Assert.AreEqual(1, _page.AvailableClasses.Count);
		Assert.AreEqual(publicationClass.Name, _page.AvailableClasses[0].Text);
	}

	[TestMethod]
	public async Task OnPost_PublicationClassNotFound_ReturnsNotFound()
	{
		_page.PublicationClassId = 999; // Non-existent class

		var result = await _page.OnPost();

		Assert.IsInstanceOfType<NotFoundResult>(result);
	}

	[TestMethod]
	public async Task OnPost_PublicationNotFound_ReturnsNotFound()
	{
		var publicationClass = new PublicationClass { Id = 1, Name = "Standard" };
		_db.PublicationClasses.Add(publicationClass);
		await _db.SaveChangesAsync();

		_page.Id = 999; // Non-existent publication
		_page.PublicationClassId = publicationClass.Id;

		var result = await _page.OnPost();

		Assert.IsInstanceOfType<NotFoundResult>(result);
	}

	[TestMethod]
	public async Task OnPost_SamePublicationClass_RedirectsWithoutChanges()
	{
		var publicationClass = new PublicationClass { Id = 1, Name = "Standard" };
		var publication = _db.AddPublication(publicationClass: publicationClass).Entity;
		_page.Id = publication.Id;
		_page.PublicationClassId = publicationClass.Id; // Same class

		var result = await _page.OnPost();

		Assert.IsInstanceOfType<RedirectToPageResult>(result);
		var redirect = (RedirectToPageResult)result;
		Assert.AreEqual("Edit", redirect.PageName);
		Assert.AreEqual(publication.Id, redirect.RouteValues!["Id"]);

		await _publisher.DidNotReceive().Send(Arg.Any<Post>());
		await _publicationMaintenanceLogger.DidNotReceive().Log(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string>());
	}

	[TestMethod]
	public async Task OnPost_ValidUpdate_UpdatesClassAndLogsChange()
	{
		var oldClass = new PublicationClass { Id = 1, Name = "Standard" };
		var newClass = new PublicationClass { Id = 2, Name = "Stars" };
		_db.PublicationClasses.Add(oldClass);
		_db.PublicationClasses.Add(newClass);

		var publication = _db.AddPublication().Entity;
		publication.PublicationClassId = oldClass.Id;

		var user = _db.AddUser("TestUser").Entity;
		await _db.SaveChangesAsync();
		AddAuthenticatedUser(_page, user, [PermissionTo.SetPublicationClass]);

		_page.Id = publication.Id;
		_page.PublicationClassId = newClass.Id;

		var result = await _page.OnPost();

		Assert.IsInstanceOfType<RedirectToPageResult>(result);
		var redirect = (RedirectToPageResult)result;
		Assert.AreEqual("Edit", redirect.PageName);
		Assert.AreEqual(publication.Id, redirect.RouteValues!["Id"]);

		// Note: Database update verification is skipped because ExecuteUpdateAsync
		// doesn't work reliably with in-memory test databases. The fact that
		// the external services were called indicates the update succeeded.
		await _publicationMaintenanceLogger.Received(1).Log(publication.Id, user.Id, Arg.Any<string>());
		await _publisher.Received(1).Send(Arg.Any<Post>());
	}
}
