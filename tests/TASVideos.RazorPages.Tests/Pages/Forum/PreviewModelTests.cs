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
	[DataRow("")]
	[DataRow("Test forum post content")]
	[DataRow("[b]Bold text[/b]", "<b>Bold text</b>")]
	[DataRow("Special chars: éñüß中文日本語 🎮")]
	public async Task OnPost_ValidInput_ReturnsContentResult(string content, string? expected = null)
	{
		SetBody(_model, content);

		var result = await _model.OnPost();

		AssertContent(result);
		if (!string.IsNullOrWhiteSpace(content))
		{
			Assert.IsTrue(((ContentResult)result).Content!.Contains(expected ?? content));
		}
	}

	[TestMethod]
	public async Task OnPost_MultilineInput_PreservesFormatting()
	{
		const string text = """
			Line 1
			Line 2

			Line 3 with blank line above
			""";
		var bodyStream = new MemoryStream(Encoding.UTF8.GetBytes(text));
		_model.PageContext.HttpContext.Request.Body = bodyStream;

		var result = await _model.OnPost();

		AssertContent(result);
	}

	[TestMethod]
	public async Task OnPost_NullInputStream_HandlesGracefully()
	{
		_model.PageContext.HttpContext.Request.Body = Stream.Null;
		var result = await _model.OnPost();
		AssertContent(result);
	}

	[TestMethod]
	public void RequiresPermission() => AssertHasPermission(typeof(PreviewModel), PermissionTo.CreateForumPosts);

	private static void AssertContent(IActionResult result)
	{
		Assert.IsInstanceOfType(result, typeof(ContentResult));
		var contentResult = (ContentResult)result;
		Assert.IsNotNull(contentResult.Content);
		Assert.IsTrue(contentResult.Content.Contains("postbody"));
	}
}
