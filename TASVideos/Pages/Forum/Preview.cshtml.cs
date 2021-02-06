using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TASVideos.Data.Entity;
using TASVideos.ForumEngine;

namespace TASVideos.Pages.Forum
{
	[RequirePermission(PermissionTo.CreateForumPosts)]
	[IgnoreAntiforgeryToken]
	public class PreviewModel : BasePageModel
	{
		public async Task<IActionResult> OnPost()
		{
			var text = await new StreamReader(Request.Body, Encoding.UTF8).ReadToEndAsync();
			var renderedText = RenderPost(text, true, false); // TODO: pass in bbcode flag
			return new ContentResult { Content = renderedText };
		}

		private string RenderPost(string text, bool useBbCode, bool useHtml)
		{
			var parsed = PostParser.Parse(text, useBbCode, useHtml);
			using var writer = new StringWriter();
			writer.Write("<div class=postbody>");
			parsed.WriteHtml(writer);
			writer.Write("</div>");
			return writer.ToString();
		}
	}
}
