using System.Text;
using TASVideos.Common;
using TASVideos.ForumEngine;

namespace TASVideos.Pages.Forum;

[RequirePermission(PermissionTo.CreateForumPosts)]
[IgnoreAntiforgeryToken]
public class PreviewModel(IWriterHelper helper) : BasePageModel
{
	public async Task<IActionResult> OnPost()
	{
		var text = await new StreamReader(Request.Body, Encoding.UTF8).ReadToEndAsync();
		var renderedText = await RenderPost(text, true, false); // New posts are always bbcode = true, html = false
		return Content(renderedText);
	}

	private async Task<string> RenderPost(string text, bool useBbCode, bool useHtml)
	{
		var parsed = PostParser.Parse(text, useBbCode, useHtml);
		await using var writer = new StringWriter();
		var htmlWriter = new HtmlWriter(writer);
		htmlWriter.OpenTag("div");
		htmlWriter.Attribute("class", "postbody");
		await parsed.WriteHtml(htmlWriter, helper);
		htmlWriter.CloseTag("div");
		htmlWriter.AssertFinished();
		return writer.ToString();
	}
}
