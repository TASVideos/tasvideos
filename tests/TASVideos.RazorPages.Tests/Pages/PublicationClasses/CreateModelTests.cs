using TASVideos.Core.Services;
using TASVideos.Pages.PublicationClasses;

namespace TASVideos.RazorPages.Tests.Pages.PublicationClasses;

[TestClass]
public class CreateModelTests : BasePageModelTests
{
	private readonly IClassService _classService;
	private readonly CreateModel _model;

	public CreateModelTests()
	{
		_classService = Substitute.For<IClassService>();
		_model = new CreateModel(_classService)
		{
			PageContext = TestPageContext()
		};
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
	public async Task OnPost_ValidPublicationClass_SuccessfulCreation_RedirectsToIndex()
	{
		_classService.Add(Arg.Any<PublicationClass>()).Returns((1, ClassEditResult.Success));

		var result = await _model.OnPost();

		AssertRedirect(result, "Index");
		Assert.AreEqual("success", _model.MessageType);
	}

	[TestMethod]
	public async Task OnPost_DuplicateName_AddsModelErrorAndReturnsPage()
	{
		_classService.Add(Arg.Any<PublicationClass>()).Returns((null, ClassEditResult.DuplicateName));

		var result = await _model.OnPost();

		Assert.IsInstanceOfType(result, typeof(PageResult));
		Assert.IsFalse(_model.ModelState.IsValid);
		Assert.IsTrue(_model.ModelState.ContainsKey("PublicationClass.Name"));
		Assert.IsNull(_model.MessageType);
	}

	[TestMethod]
	public async Task OnPost_CreateFails_ShowsErrorMessageAndReturnsPage()
	{
		_classService.Add(Arg.Any<PublicationClass>()).Returns((null, ClassEditResult.Fail));

		var result = await _model.OnPost();

		Assert.IsInstanceOfType(result, typeof(PageResult));
		Assert.AreEqual("danger", _model.MessageType);
	}

	[TestMethod]
	public void RequiresPermission() => AssertHasPermission(typeof(CreateModel), PermissionTo.ClassMaintenance);
}
