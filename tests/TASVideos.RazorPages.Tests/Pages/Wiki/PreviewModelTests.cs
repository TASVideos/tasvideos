using System.Text;
using TASVideos.Core.Services.Wiki;
using TASVideos.Pages.Wiki;

namespace TASVideos.RazorPages.Tests.Pages.Wiki;

[TestClass]
public class PreviewModelTests : BasePageModelTests
{
	private readonly IWikiPages _mockWikiPages;
	private readonly PreviewModel _model;

	public PreviewModelTests()
	{
		_mockWikiPages = Substitute.For<IWikiPages>();
		_model = new PreviewModel(_mockWikiPages)
		{
			PageContext = TestPageContext()
		};
	}

	[TestMethod]
	public async Task OnPost_NoPath_ReadsMarkupFromRequestBody()
	{
		const string markup = "Test markup content";
		var bodyStream = new MemoryStream(Encoding.UTF8.GetBytes(markup));
		_model.PageContext.HttpContext.Request.Body = bodyStream;

		var result = await _model.OnPost();

		Assert.IsInstanceOfType(result, typeof(PageResult));
		Assert.AreEqual(markup, _model.Markup);
	}

	[TestMethod]
	public async Task OnPost_PathProvided_FetchesPageData()
	{
		const string path = "TestPage";
		const string markup = "Test markup content";

		var mockPageData = Substitute.For<IWikiPage>();
		_mockWikiPages.Page(path).Returns(mockPageData);

		var bodyStream = new MemoryStream(Encoding.UTF8.GetBytes(markup));
		_model.PageContext.HttpContext.Request.Body = bodyStream;
		_model.Path = path;

		var result = await _model.OnPost();

		Assert.IsInstanceOfType(result, typeof(PageResult));
		Assert.AreEqual(markup, _model.Markup);
		Assert.AreSame(mockPageData, _model.PageData);
		await _mockWikiPages.Received(1).Page(path);
	}

	[TestMethod]
	public async Task OnPost_PathProvidedButPageNotFound_ReturnsNotFound()
	{
		const string path = "NonExistentPage";
		const string markup = "Test markup content";

		_mockWikiPages.Page(path).Returns(null as IWikiPage);

		var bodyStream = new MemoryStream(Encoding.UTF8.GetBytes(markup));
		_model.PageContext.HttpContext.Request.Body = bodyStream;
		_model.Path = path;

		var result = await _model.OnPost();

		Assert.IsInstanceOfType(result, typeof(NotFoundResult));
		Assert.AreEqual(markup, _model.Markup);
		await _mockWikiPages.Received(1).Page(path);
	}

	[TestMethod]
	public async Task OnPost_EmptyMarkup_HandlesEmptyInput()
	{
		const string markup = "";
		var bodyStream = new MemoryStream(Encoding.UTF8.GetBytes(markup));
		_model.PageContext.HttpContext.Request.Body = bodyStream;

		var result = await _model.OnPost();

		Assert.IsInstanceOfType(result, typeof(PageResult));
		Assert.AreEqual(markup, _model.Markup);
	}

	[TestMethod]
	public async Task OnPost_LargeMarkup_HandlesLargeInput()
	{
		var markup = new string('x', 10000); // 10KB of content
		var bodyStream = new MemoryStream(Encoding.UTF8.GetBytes(markup));
		_model.PageContext.HttpContext.Request.Body = bodyStream;

		var result = await _model.OnPost();

		Assert.IsInstanceOfType(result, typeof(PageResult));
		Assert.AreEqual(markup, _model.Markup);
	}

	[TestMethod]
	public async Task OnPost_MarkupWithSpecialCharacters_PreservesCharacters()
	{
		const string markup = "Test with special chars: éñüß中文日本語 [[WikiLink]]";
		var bodyStream = new MemoryStream(Encoding.UTF8.GetBytes(markup));
		_model.PageContext.HttpContext.Request.Body = bodyStream;

		var result = await _model.OnPost();

		Assert.IsInstanceOfType(result, typeof(PageResult));
		Assert.AreEqual(markup, _model.Markup);
	}

	[TestMethod]
	public async Task OnPost_PathWithSpecialCharacters_CallsWikiPagesService()
	{
		const string path = "Test/Page With Spaces";
		const string markup = "Test content";

		var mockPageData = Substitute.For<IWikiPage>();
		_mockWikiPages.Page(path).Returns(mockPageData);

		var bodyStream = new MemoryStream(Encoding.UTF8.GetBytes(markup));
		_model.PageContext.HttpContext.Request.Body = bodyStream;
		_model.Path = path;

		var result = await _model.OnPost();

		Assert.IsInstanceOfType(result, typeof(PageResult));
		await _mockWikiPages.Received(1).Page(path);
	}

	[TestMethod]
	public async Task OnPost_PathIsNull_DoesNotCallWikiPagesService()
	{
		const string markup = "Test content";
		var bodyStream = new MemoryStream(Encoding.UTF8.GetBytes(markup));
		_model.PageContext.HttpContext.Request.Body = bodyStream;
		_model.Path = null;

		var result = await _model.OnPost();

		Assert.IsInstanceOfType(result, typeof(PageResult));
		await _mockWikiPages.DidNotReceive().Page(Arg.Any<string>());
	}

	[TestMethod]
	public async Task OnPost_PathIsEmpty_CallsWikiPagesServiceAndReturnsNotFound()
	{
		const string markup = "Test content";
		var bodyStream = new MemoryStream(Encoding.UTF8.GetBytes(markup));
		_model.PageContext.HttpContext.Request.Body = bodyStream;
		_model.Path = "";

		_mockWikiPages.Page("").Returns(null as IWikiPage);

		var result = await _model.OnPost();

		Assert.IsInstanceOfType(result, typeof(NotFoundResult));
		await _mockWikiPages.Received(1).Page("");
	}

	[TestMethod]
	public async Task OnPost_WikiPagesServiceThrows_PropagatesException()
	{
		const string path = "TestPage";
		const string markup = "Test content";

		_mockWikiPages.Page(path).Returns<IWikiPage?>(_ => throw new InvalidOperationException("Service error"));

		var bodyStream = new MemoryStream(Encoding.UTF8.GetBytes(markup));
		_model.PageContext.HttpContext.Request.Body = bodyStream;
		_model.Path = path;

		await Assert.ThrowsExactlyAsync<InvalidOperationException>(async () =>
		{
			await _model.OnPost();
		});
	}

	[TestMethod]
	public async Task OnPost_UTF8Encoding_CorrectlyDecodesContent()
	{
		const string markup = "UTF-8 content: 你好世界 🌍";
		var bodyBytes = Encoding.UTF8.GetBytes(markup);
		var bodyStream = new MemoryStream(bodyBytes);
		_model.PageContext.HttpContext.Request.Body = bodyStream;

		var result = await _model.OnPost();

		Assert.IsInstanceOfType(result, typeof(PageResult));
		Assert.AreEqual(markup, _model.Markup);
	}

	[TestMethod]
	public async Task OnPost_MultilineMarkup_PreservesFormatting()
	{
		const string markup = """
							Line 1
							Line 2
							Line 3

							Line 5 with blank line above
							""";
		var bodyStream = new MemoryStream(Encoding.UTF8.GetBytes(markup));
		_model.PageContext.HttpContext.Request.Body = bodyStream;

		var result = await _model.OnPost();

		Assert.IsInstanceOfType(result, typeof(PageResult));
		Assert.AreEqual(markup, _model.Markup);
	}

	[TestMethod]
	public async Task OnPost_PathIsWhitespace_CallsWikiPagesServiceAndReturnsNotFound()
	{
		const string markup = "Test content";
		var bodyStream = new MemoryStream(Encoding.UTF8.GetBytes(markup));
		_model.PageContext.HttpContext.Request.Body = bodyStream;
		_model.Path = "   ";

		_mockWikiPages.Page("   ").Returns(null as IWikiPage);

		var result = await _model.OnPost();

		Assert.IsInstanceOfType(result, typeof(NotFoundResult));
		await _mockWikiPages.Received(1).Page("   ");
	}
}
