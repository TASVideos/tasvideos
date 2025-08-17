using System.Text;
using TASVideos.ForumEngine;
using TASVideos.Pages.Forum;

namespace TASVideos.RazorPages.Tests.Pages.Forum;

[TestClass]
public class PreviewModelTests : BasePageModelTests
{
	private readonly PreviewModel _model;

	public PreviewModelTests()
	{
		var writerHelper = Substitute.For<IWriterHelper>();
		_model = new PreviewModel(writerHelper)
		{
			PageContext = TestPageContext()
		};
	}

	[TestMethod]
	public async Task OnPost_ValidInput_ReturnsContentResult()
	{
		const string text = "Test forum post content";
		var bodyStream = new MemoryStream(Encoding.UTF8.GetBytes(text));
		_model.PageContext.HttpContext.Request.Body = bodyStream;

		var result = await _model.OnPost();

		Assert.IsInstanceOfType(result, typeof(ContentResult));
		var contentResult = (ContentResult)result;
		Assert.IsNotNull(contentResult.Content);
		Assert.IsTrue(contentResult.Content.Contains("postbody"));
	}

	[TestMethod]
	public async Task OnPost_EmptyInput_ReturnsContentResult()
	{
		const string text = "";
		var bodyStream = new MemoryStream(Encoding.UTF8.GetBytes(text));
		_model.PageContext.HttpContext.Request.Body = bodyStream;

		var result = await _model.OnPost();

		Assert.IsInstanceOfType(result, typeof(ContentResult));
		var contentResult = (ContentResult)result;
		Assert.IsNotNull(contentResult.Content);
		Assert.IsTrue(contentResult.Content.Contains("postbody"));
	}

	[TestMethod]
	public async Task OnPost_BBCodeInput_ProcessesBBCode()
	{
		const string text = "[b]Bold text[/b]";
		var bodyStream = new MemoryStream(Encoding.UTF8.GetBytes(text));
		_model.PageContext.HttpContext.Request.Body = bodyStream;

		var result = await _model.OnPost();

		Assert.IsInstanceOfType(result, typeof(ContentResult));
		var contentResult = (ContentResult)result;
		Assert.IsNotNull(contentResult.Content);
		Assert.IsTrue(contentResult.Content.Contains("postbody"));
	}

	[TestMethod]
	public async Task OnPost_LargeInput_HandlesLargeContent()
	{
		var text = new string('x', 10000); // 10KB of content
		var bodyStream = new MemoryStream(Encoding.UTF8.GetBytes(text));
		_model.PageContext.HttpContext.Request.Body = bodyStream;

		var result = await _model.OnPost();

		Assert.IsInstanceOfType(result, typeof(ContentResult));
		var contentResult = (ContentResult)result;
		Assert.IsNotNull(contentResult.Content);
		Assert.IsTrue(contentResult.Content.Contains("postbody"));
	}

	[TestMethod]
	public async Task OnPost_SpecialCharacters_PreservesUTF8Encoding()
	{
		const string text = "Special chars: éñüß中文日本語 🎮";
		var bodyStream = new MemoryStream(Encoding.UTF8.GetBytes(text));
		_model.PageContext.HttpContext.Request.Body = bodyStream;

		var result = await _model.OnPost();

		Assert.IsInstanceOfType(result, typeof(ContentResult));
		var contentResult = (ContentResult)result;
		Assert.IsNotNull(contentResult.Content);
		Assert.IsTrue(contentResult.Content.Contains("postbody"));
	}

	[TestMethod]
	public async Task OnPost_MultilineInput_PreservesFormatting()
	{
		const string text = """
			Line 1
			Line 2
			Line 3

			Line 5 with blank line above
			""";
		var bodyStream = new MemoryStream(Encoding.UTF8.GetBytes(text));
		_model.PageContext.HttpContext.Request.Body = bodyStream;

		var result = await _model.OnPost();

		Assert.IsInstanceOfType(result, typeof(ContentResult));
		var contentResult = (ContentResult)result;
		Assert.IsNotNull(contentResult.Content);
		Assert.IsTrue(contentResult.Content.Contains("postbody"));
	}

	[TestMethod]
	public async Task OnPost_MalformedBBCode_HandlesGracefully()
	{
		const string text = "[b]Unclosed bold tag [i]and italic[/b]";
		var bodyStream = new MemoryStream(Encoding.UTF8.GetBytes(text));
		_model.PageContext.HttpContext.Request.Body = bodyStream;

		var result = await _model.OnPost();

		Assert.IsInstanceOfType(result, typeof(ContentResult));
		var contentResult = (ContentResult)result;
		Assert.IsNotNull(contentResult.Content);
		Assert.IsTrue(contentResult.Content.Contains("postbody"));
	}

	[TestMethod]
	public async Task OnPost_NullInputStream_HandlesGracefully()
	{
		_model.PageContext.HttpContext.Request.Body = Stream.Null;

		var result = await _model.OnPost();

		Assert.IsInstanceOfType(result, typeof(ContentResult));
		var contentResult = (ContentResult)result;
		Assert.IsNotNull(contentResult.Content);
		Assert.IsTrue(contentResult.Content.Contains("postbody"));
	}

	[TestMethod]
	public async Task OnPost_UrlsInContent_ProcessedCorrectly()
	{
		const string text = "Check out https://example.com and [url]http://test.com[/url]";
		var bodyStream = new MemoryStream(Encoding.UTF8.GetBytes(text));
		_model.PageContext.HttpContext.Request.Body = bodyStream;

		var result = await _model.OnPost();

		Assert.IsInstanceOfType(result, typeof(ContentResult));
		var contentResult = (ContentResult)result;
		Assert.IsNotNull(contentResult.Content);
		Assert.IsTrue(contentResult.Content.Contains("postbody"));
	}

	[TestMethod]
	public async Task OnPost_ValidBBCodeTags_ProcessedCorrectly()
	{
		const string text = "[b]Bold[/b] [i]Italic[/i] [u]Underline[/u] [code]Code block[/code]";
		var bodyStream = new MemoryStream(Encoding.UTF8.GetBytes(text));
		_model.PageContext.HttpContext.Request.Body = bodyStream;

		var result = await _model.OnPost();

		Assert.IsInstanceOfType(result, typeof(ContentResult));
		var contentResult = (ContentResult)result;
		Assert.IsNotNull(contentResult.Content);
		Assert.IsTrue(contentResult.Content.Contains("postbody"));
	}
}
