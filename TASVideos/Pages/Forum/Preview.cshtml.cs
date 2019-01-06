using System.IO;
using System.Text;

using Microsoft.AspNetCore.Mvc;

using TASVideos.Data.Entity;
using TASVideos.Tasks;

namespace TASVideos.Pages.Forum
{
	[RequirePermission(PermissionTo.CreateForumPosts)]
	[IgnoreAntiforgeryToken]
	public class PreviewModel : BasePageModel
	{
		public PreviewModel(
			UserTasks userTasks)
			: base(userTasks)
		{
		}

		public IActionResult OnPost()
		{
			var text = new StreamReader(Request.Body, Encoding.UTF8).ReadToEnd();
			var renderedText = RenderPost(text, true, false); // TODO: pass in bbcode flag
			return new ContentResult { Content = renderedText };
		}
	}
}
