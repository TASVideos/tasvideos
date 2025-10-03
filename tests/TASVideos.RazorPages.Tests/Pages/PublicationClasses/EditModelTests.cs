using TASVideos.Core.Services;
using TASVideos.Pages.PublicationClasses;

namespace TASVideos.RazorPages.Tests.Pages.PublicationClasses;

[TestClass]
public class EditModelTests : BasePageModelTests
{
	private readonly IClassService _classService;
	private readonly EditModel _model;

	public EditModelTests()
	{
		_classService = Substitute.For<IClassService>();
		_model = new EditModel(_classService)
		{
			PageContext = TestPageContext()
		};
	}

	[TestMethod]
	public async Task OnGet_InvalidId_ReturnsNotFound()
	{
		_classService.GetById(Arg.Any<int>()).Returns((PublicationClass?)null);
		var result = await _model.OnGet();
		Assert.IsInstanceOfType(result, typeof(NotFoundResult));
	}

	[TestMethod]
	public async Task OnGet_ValidId_ReturnsPageWithPublicationClass()
	{
		var pubClass = _db.AddPublicationClass("Test Class").Entity;
		pubClass.IconPath = "Test Class";
		pubClass.Link = "test-link";

		_classService.GetById(1).Returns(pubClass);
		_classService.InUse(1).Returns(true);
		_model.Id = 1;

		var result = await _model.OnGet();

		Assert.IsInstanceOfType(result, typeof(PageResult));
		Assert.AreEqual(pubClass.Name, _model.PublicationClass.Name);
		Assert.AreEqual(pubClass.IconPath, _model.PublicationClass.IconPath);
		Assert.AreEqual(pubClass.Link, _model.PublicationClass.Link);
		Assert.IsTrue(_model.InUse);
		await _classService.Received(1).GetById(1);
		await _classService.Received(1).InUse(1);
	}

	[TestMethod]
	public async Task OnPost_InvalidModelState_ReturnsPage()
	{
		_model.ModelState.AddModelError("PublicationClass.Name", "Name is required");

		var result = await _model.OnPost();

		Assert.IsInstanceOfType(result, typeof(PageResult));
		Assert.IsFalse(_model.ModelState.IsValid);
	}

	[TestMethod]
	public async Task OnPost_ValidEdit_Success_RedirectsToIndex()
	{
		_classService.Edit(Arg.Any<int>(), Arg.Any<PublicationClass>()).Returns(ClassEditResult.Success);

		var result = await _model.OnPost();

		AssertRedirect(result, "Index");
		Assert.AreEqual("success", _model.MessageType);
	}

	[TestMethod]
	public async Task OnPost_NotFound_ReturnsNotFound()
	{
		_classService.Edit(Arg.Any<int>(), Arg.Any<PublicationClass>()).Returns(ClassEditResult.NotFound);
		var result = await _model.OnPost();
		Assert.IsInstanceOfType(result, typeof(NotFoundResult));
	}

	[TestMethod]
	public async Task OnPost_DuplicateName_AddsModelErrorAndReturnsPage()
	{
		_classService.Edit(Arg.Any<int>(), Arg.Any<PublicationClass>()).Returns(ClassEditResult.DuplicateName);

		var result = await _model.OnPost();

		Assert.IsInstanceOfType(result, typeof(PageResult));
		Assert.IsFalse(_model.ModelState.IsValid);
		Assert.IsTrue(_model.ModelState.ContainsKey("PublicationClass.Name"));
		Assert.IsNull(_model.MessageType);
	}

	[TestMethod]
	public async Task OnPost_EditFails_ShowsErrorMessageAndReturnsPage()
	{
		_classService.Edit(Arg.Any<int>(), Arg.Any<PublicationClass>()).Returns(ClassEditResult.Fail);

		var result = await _model.OnPost();

		Assert.IsInstanceOfType(result, typeof(PageResult));
		Assert.AreEqual("danger", _model.MessageType);
	}

	[TestMethod]
	public async Task OnPostDelete_InUse_ShowsErrorMessageAndRedirects()
	{
		_classService.Delete(Arg.Any<int>()).Returns(ClassDeleteResult.InUse);

		var result = await _model.OnPostDelete();

		AssertRedirect(result, "Index");
		Assert.AreEqual("danger", _model.MessageType);
	}

	[TestMethod]
	public async Task OnPostDelete_Success_ShowsSuccessMessageAndRedirects()
	{
		_classService.Delete(Arg.Any<int>()).Returns(ClassDeleteResult.Success);

		var result = await _model.OnPostDelete();

		AssertRedirect(result, "Index");
		Assert.AreEqual("success", _model.MessageType);
	}

	[TestMethod]
	public async Task OnPostDelete_NotFound_ShowsErrorMessageAndRedirects()
	{
		_classService.Delete(Arg.Any<int>()).Returns(ClassDeleteResult.NotFound);

		var result = await _model.OnPostDelete();

		AssertRedirect(result, "Index");
		Assert.AreEqual("danger", _model.MessageType);
	}

	[TestMethod]
	public async Task OnPostDelete_Fail_ShowsErrorMessageAndRedirects()
	{
		_classService.Delete(Arg.Any<int>()).Returns(ClassDeleteResult.Fail);

		var result = await _model.OnPostDelete();

		AssertRedirect(result, "Index");
		Assert.AreEqual("danger", _model.MessageType);
	}

	[TestMethod]
	public void RequiresPermission() => AssertHasPermission(typeof(EditModel), PermissionTo.ClassMaintenance);
}
