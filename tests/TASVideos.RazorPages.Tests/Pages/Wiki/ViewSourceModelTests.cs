using TASVideos.Core.Services.Wiki;
using TASVideos.Pages.Wiki;

namespace TASVideos.RazorPages.Tests.Pages.Wiki;

[TestClass]
public class ViewSourceModelTests : BasePageModelTests
{
	private readonly IWikiPages _mockWikiPages;
	private readonly ViewSourceModel _model;

	public ViewSourceModelTests()
	{
		_mockWikiPages = Substitute.For<IWikiPages>();
		_model = new ViewSourceModel(_mockWikiPages)
		{
			PageContext = TestPageContext()
		};
	}

	[TestMethod]
	[DataRow(null)]
	[DataRow("")]
	[DataRow(" ")]
	[DataRow("/")]
	[DataRow("/ ")]
	public async Task OnGet_NullOrWhitespacePath_ReturnsNotFound(string path)
	{
		_model.Path = null;
		var result = await _model.OnGet();
		Assert.IsInstanceOfType(result, typeof(NotFoundResult));
	}

	[TestMethod]
	public async Task OnGet_PathWithSlashes_TrimsSlashes()
	{
		const string originalPath = "/TestPage/";
		const string expectedPath = "TestPage";

		var mockPage = Substitute.For<IWikiPage>();
		_mockWikiPages.Page(expectedPath).Returns(mockPage);

		_model.Path = originalPath;

		var result = await _model.OnGet();

		Assert.AreEqual(expectedPath, _model.Path);
		Assert.IsInstanceOfType(result, typeof(PageResult));
		Assert.AreSame(mockPage, _model.WikiPage);
		await _mockWikiPages.Received(1).Page(expectedPath);
	}

	[TestMethod]
	public async Task OnGet_PageExists_ReturnsPageResult()
	{
		const string path = "ExistingPage";
		var mockPage = Substitute.For<IWikiPage>();
		_mockWikiPages.Page(path).Returns(mockPage);

		_model.Path = path;

		var result = await _model.OnGet();

		Assert.IsInstanceOfType(result, typeof(PageResult));
		Assert.AreSame(mockPage, _model.WikiPage);
		await _mockWikiPages.Received(1).Page(path);
	}

	[TestMethod]
	public async Task OnGet_PageDoesNotExist_ReturnsNotFound()
	{
		const string path = "NonExistentPage";
		_mockWikiPages.Page(path).Returns(null as IWikiPage);

		_model.Path = path;

		var result = await _model.OnGet();

		Assert.IsInstanceOfType(result, typeof(NotFoundResult));
		await _mockWikiPages.Received(1).Page(path);
	}

	[TestMethod]
	public async Task OnGet_WithRevision_PassesRevisionToService()
	{
		const string path = "TestPage";
		const int revision = 5;
		var mockPage = Substitute.For<IWikiPage>();
		_mockWikiPages.Page(path, revision).Returns(mockPage);

		_model.Path = path;
		_model.Revision = revision;

		var result = await _model.OnGet();

		Assert.IsInstanceOfType(result, typeof(PageResult));
		Assert.AreSame(mockPage, _model.WikiPage);
		await _mockWikiPages.Received(1).Page(path, revision);
	}

	[TestMethod]
	public async Task OnGet_WithRevisionButPageNotFound_ReturnsNotFound()
	{
		const string path = "TestPage";
		const int revision = 999;
		_mockWikiPages.Page(path, revision).Returns(null as IWikiPage);

		_model.Path = path;
		_model.Revision = revision;

		var result = await _model.OnGet();

		Assert.IsInstanceOfType(result, typeof(NotFoundResult));
		await _mockWikiPages.Received(1).Page(path, revision);
	}

	[TestMethod]
	public async Task OnGet_NoRevisionSpecified_PassesNullRevision()
	{
		const string path = "TestPage";
		var mockPage = Substitute.For<IWikiPage>();
		_mockWikiPages.Page(path).Returns(mockPage);

		_model.Path = path;
		_model.Revision = null;

		var result = await _model.OnGet();

		Assert.IsInstanceOfType(result, typeof(PageResult));
		Assert.AreSame(mockPage, _model.WikiPage);
		await _mockWikiPages.Received(1).Page(path);
	}

	[TestMethod]
	public async Task OnGet_PathWithSpecialCharacters_PassesPathCorrectly()
	{
		const string specialPath = "Page/With Special-Characters_123";
		var mockPage = Substitute.For<IWikiPage>();
		_mockWikiPages.Page(specialPath).Returns(mockPage);

		_model.Path = specialPath;

		var result = await _model.OnGet();

		Assert.IsInstanceOfType(result, typeof(PageResult));
		Assert.AreSame(mockPage, _model.WikiPage);
		await _mockWikiPages.Received(1).Page(specialPath);
	}

	[TestMethod]
	public async Task OnGet_MultipleSlashes_TrimsAllSlashes()
	{
		const string pathWithSlashes = "///TestPage///";
		const string expectedPath = "TestPage";
		var mockPage = Substitute.For<IWikiPage>();
		_mockWikiPages.Page(expectedPath).Returns(mockPage);

		_model.Path = pathWithSlashes;

		var result = await _model.OnGet();

		Assert.AreEqual(expectedPath, _model.Path);
		Assert.IsInstanceOfType(result, typeof(PageResult));
		await _mockWikiPages.Received(1).Page(expectedPath);
	}

	[TestMethod]
	public async Task OnGet_PathAndRevisionCombination_CallsServiceWithBothParameters()
	{
		const string path = "TestPage";
		const int revision = 42;
		var mockPage = Substitute.For<IWikiPage>();
		_mockWikiPages.Page(path, revision).Returns(mockPage);

		_model.Path = path;
		_model.Revision = revision;

		var result = await _model.OnGet();

		Assert.AreEqual(path, _model.Path);
		Assert.AreEqual(revision, _model.Revision);
		Assert.IsInstanceOfType(result, typeof(PageResult));
		Assert.AreSame(mockPage, _model.WikiPage);
		await _mockWikiPages.Received(1).Page(path, revision);
	}
}
