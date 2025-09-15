using TASVideos.Core.Services;
using TASVideos.Pages.Systems;

namespace TASVideos.RazorPages.Tests.Pages.Systems;

[TestClass]
public class CreateModelTests : BasePageModelTests
{
	private readonly IGameSystemService _systemService;
	private readonly CreateModel _model;

	public CreateModelTests()
	{
		_systemService = Substitute.For<IGameSystemService>();
		_model = new CreateModel(_systemService)
		{
			PageContext = TestPageContext()
		};
	}

	[TestMethod]
	public async Task OnGet_SetsId()
	{
		_systemService.NextId().Returns(123);
		await _model.OnGet();
		Assert.AreEqual(123, _model.System.Id);
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
	public async Task OnPost_SuccessfulCreation_RedirectsToEdit()
	{
		_systemService.Add(Arg.Any<int>(), Arg.Any<string>(), Arg.Any<string>()).Returns(SystemEditResult.Success);

		var result = await _model.OnPost();

		AssertRedirect(result, "Edit");
		Assert.AreEqual("success", _model.MessageType);
	}

	[TestMethod]
	public async Task OnPost_DuplicateId_AddsModelErrorAndReturnsPage()
	{
		_systemService.Add(Arg.Any<int>(), Arg.Any<string>(), Arg.Any<string>()).Returns(SystemEditResult.DuplicateId);

		var result = await _model.OnPost();

		Assert.IsInstanceOfType(result, typeof(PageResult));
		Assert.IsFalse(_model.ModelState.IsValid);
		Assert.IsTrue(_model.ModelState.ContainsKey("System.Id"));
		Assert.IsNull(_model.MessageType);
	}

	[TestMethod]
	public async Task OnPost_AddFails_ShowsErrorMessageAndReturnsPage()
	{
		_systemService.Add(Arg.Any<int>(), Arg.Any<string>(), Arg.Any<string>()).Returns(SystemEditResult.Fail);

		var result = await _model.OnPost();

		Assert.IsInstanceOfType(result, typeof(PageResult));
		Assert.AreEqual("danger", _model.MessageType);
	}

	[TestMethod]
	public void RequiresPermission() => AssertHasPermission(typeof(CreateModel), PermissionTo.GameSystemMaintenance);
}
