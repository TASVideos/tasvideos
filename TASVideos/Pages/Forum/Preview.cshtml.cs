using System.Text;
using Microsoft.AspNetCore.Mvc;
using TASVideos.Common;
using TASVideos.Data.Entity;
using TASVideos.ForumEngine;

namespace TASVideos.Pages.Forum;

[RequirePermission(PermissionTo.CreateForumPosts)]
[IgnoreAntiforgeryToken]
public class PreviewModel : BasePageModel
{
	private readonly IWriterHelper _helper;

	public PreviewModel(IWriterHelper helper)
	{
		_helper = helper;
	}

	public async Task<IActionResult> OnPost()
	{
		var text = await new StreamReader(Request.Body, Encoding.UTF8).ReadToEndAsync();
		var renderedText = await RenderPost(text, true, false); // New posts are always bbcode = true, html = false
		return new ContentResult { Content = renderedText };
	}

	private async Task<string> RenderPost(string text, bool useBbCode, bool useHtml)
	{
		var parsed = PostParser.Parse(text, useBbCode, useHtml);
		await using var writer = new StringWriter();
		var htmlWriter = new HtmlWriter(writer);
		htmlWriter.OpenTag("div");
		htmlWriter.Attribute("class", "postbody");
		await parsed.WriteHtml(htmlWriter, _helper);
		htmlWriter.CloseTag("div");
		htmlWriter.AssertFinished();
		return writer.ToString();
	}
}
