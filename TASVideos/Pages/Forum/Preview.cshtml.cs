using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TASVideos.Data.Entity;
using TASVideos.ForumEngine;

namespace TASVideos.RazorPages.Pages.Forum
{
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
			writer.Write("<div class=postbody>");
			await parsed.WriteHtml(writer, _helper);
			writer.Write("</div>");
			return writer.ToString();
		}
	}
}
