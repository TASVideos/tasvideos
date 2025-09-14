using TASVideos.Core.Services;
using TASVideos.Pages.Systems;

namespace TASVideos.RazorPages.Tests.Pages.Systems;

[TestClass]
public class EditModelTests : BasePageModelTests
{
	private readonly IGameSystemService _systemService;
	private readonly EditModel _model;

	public EditModelTests()
	{
		_systemService = Substitute.For<IGameSystemService>();
		_model = new EditModel(_systemService)
		{
			PageContext = TestPageContext(),
			System = new(123, "NES", "Nintendo", [new FrameRatesResponse(456, 60, "NTSC", true, true)])
		};
	}

	[TestMethod]
	public async Task OnGet_NotFound_ReturnsNotFound()
	{
		_systemService.GetById(Arg.Any<int>()).Returns((SystemsResponse?)null);
		var result = await _model.OnGet();
		Assert.IsInstanceOfType<NotFoundResult>(result);
	}

	[TestMethod]
	public async Task OnGet_Success_ReturnsData()
	{
		_model.System = null!;
		var system = new SystemsResponse(123, "NES", "Nintendo", [new FrameRatesResponse(456, 60, "NTSC", true, true)]);
		_systemService.GetById(Arg.Any<int>()).Returns(system);
		_systemService.InUse(Arg.Any<int>()).Returns(true);

		var result = await _model.OnGet();

		Assert.IsInstanceOfType<PageResult>(result);
		Assert.AreEqual(123, system.Id);
		Assert.AreEqual("NES", system.Code);
		Assert.AreEqual("Nintendo", system.DisplayName);
		Assert.AreEqual(1, system.SystemFrameRates.Count());
		Assert.AreEqual(456, system.SystemFrameRates.First().Id);
	}

	[TestMethod]
	public async Task OnPost_InvalidModelState_ReturnsPage()
	{
		_model.ModelState.AddModelError("SomeProperty", "SomeProperty is required");

		var result = await _model.OnPost();

		Assert.IsInstanceOfType(result, typeof(PageResult));
		Assert.IsFalse(_model.ModelState.IsValid);
	}

	[TestMethod]
	public async Task OnPost_SuccessfulCreation_Redirects()
	{
		_systemService.Edit(Arg.Any<int>(), Arg.Any<string>(), Arg.Any<string>()).Returns(SystemEditResult.Success);

		var result = await _model.OnPost();

		AssertRedirect(result, "Index");
		Assert.AreEqual("success", _model.MessageType);
	}

	[TestMethod]
	public async Task OnPost_NotFound_ReturnsNotFound()
	{
		_systemService.Edit(Arg.Any<int>(), Arg.Any<string>(), Arg.Any<string>()).Returns(SystemEditResult.NotFound);
		var result = await _model.OnPost();
		Assert.IsInstanceOfType(result, typeof(NotFoundResult));
	}

	[TestMethod]
	public async Task OnPost_DuplicateCode_ReturnsModelError()
	{
		_systemService.Edit(Arg.Any<int>(), Arg.Any<string>(), Arg.Any<string>()).Returns(SystemEditResult.DuplicateCode);

		var result = await _model.OnPost();

		Assert.IsInstanceOfType(result, typeof(PageResult));
		Assert.IsNull(_model.MessageType);
		Assert.IsFalse(_model.ModelState.IsValid);
		Assert.IsTrue(_model.ModelState.ContainsKey("System.Code"));
	}

	[TestMethod]
	public async Task OnPost_Fails_ShowsErrorMessageAndReturnsPage()
	{
		_systemService.Edit(Arg.Any<int>(), Arg.Any<string>(), Arg.Any<string>()).Returns(SystemEditResult.Fail);

		var result = await _model.OnPost();

		Assert.IsInstanceOfType(result, typeof(PageResult));
		Assert.AreEqual("danger", _model.MessageType);
	}

	[TestMethod]
	public void RequiresPermission() => AssertHasPermission(typeof(CreateModel), PermissionTo.GameSystemMaintenance);
}
