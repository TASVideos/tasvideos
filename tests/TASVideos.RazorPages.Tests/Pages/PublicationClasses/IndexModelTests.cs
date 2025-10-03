using TASVideos.Core.Services;
using TASVideos.Pages.PublicationClasses;

namespace TASVideos.RazorPages.Tests.Pages.PublicationClasses;

[TestClass]
public class IndexModelTests : BasePageModelTests
{
	private readonly IClassService _classService;
	private readonly IndexModel _model;

	public IndexModelTests()
	{
		_classService = Substitute.For<IClassService>();
		_model = new IndexModel(_classService)
		{
			PageContext = TestPageContext()
		};
	}

	[TestMethod]
	public async Task OnGet_EmptyList_SetsEmptyClasses()
	{
		_classService.GetAll().Returns([]);
		await _model.OnGet();
		Assert.AreEqual(0, _model.Classes.Count);
	}

	[TestMethod]
	public async Task OnGet_SingleClass_SetsClassesCorrectly()
	{
		var pubClass = _db.AddPublicationClass("Test Class").Entity;
		pubClass.IconPath = "/icons/test.png";
		pubClass.Link = "test-link";

		_classService.GetAll().Returns([pubClass]);

		await _model.OnGet();

		Assert.AreEqual(1, _model.Classes.Count);
		var retrievedClass = _model.Classes.First();
		Assert.AreEqual("Test Class", retrievedClass.Name);
		Assert.AreEqual("/icons/test.png", retrievedClass.IconPath);
		Assert.AreEqual("test-link", retrievedClass.Link);
	}

	[TestMethod]
	public void RequiresPermission() => AssertHasPermission(typeof(IndexModel), PermissionTo.ClassMaintenance);
}
