using TASVideos.Core.Services;
using TASVideos.Pages.Systems;

namespace TASVideos.RazorPages.Tests.Pages.Systems;

[TestClass]
public class IndexModelTests : BasePageModelTests
{
	private readonly IGameSystemService _systemService;
	private readonly IndexModel _model;

	public IndexModelTests()
	{
		_systemService = Substitute.For<IGameSystemService>();
		_model = new IndexModel(_systemService)
		{
			PageContext = TestPageContext()
		};
	}

	[TestMethod]
	public async Task OnGet_EmptyList_SetsEmptyClasses()
	{
		_systemService.GetAll().Returns([]);
		await _model.OnGet();
		Assert.AreEqual(0, _model.Systems.Count);
	}

	[TestMethod]
	public async Task OnGet_SingleClass_SetsClassesCorrectly()
	{
		var system = new SystemsResponse(123, "NES", "Nintendo", [new FrameRatesResponse(456, 60, "NTSC", true, true)]);
		_systemService.GetAll().Returns([system]);

		await _model.OnGet();

		Assert.AreEqual(1, _model.Systems.Count);
		var retrievedSystem = _model.Systems.First();
		Assert.AreEqual(123, retrievedSystem.Id);
		Assert.AreEqual("NES", retrievedSystem.Code);
		Assert.AreEqual("Nintendo", retrievedSystem.DisplayName);
		Assert.AreEqual(1, retrievedSystem.SystemFrameRates.Count());
		Assert.AreEqual(456, retrievedSystem.SystemFrameRates.First().Id);
	}

	[TestMethod]
	public void AllowsAnonymousAttribute() => AssertAllowsAnonymousUsers(typeof(IndexModel));
}
