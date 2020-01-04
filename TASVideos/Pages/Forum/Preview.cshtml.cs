using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TASVideos.Data.Entity;

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
	}
}
