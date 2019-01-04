using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

using TASVideos.Data.Entity;
using TASVideos.Filter;
using TASVideos.Models;
using TASVideos.Services.ExternalMediaPublisher;
using TASVideos.Tasks;

namespace TASVideos.Controllers
{
	public class ForumController : BaseController
	{
		private readonly ForumTasks _forumTasks;

		public ForumController(
			ForumTasks forumTasks,
			UserTasks userTasks)
			: base(userTasks)
		{
			_forumTasks = forumTasks;
		}

		// TODO: this can be done with page routing
		[AllowAnonymous]
		public async Task<IActionResult> LegacyPost(int p)
		{
			return await Post(p);
		}

		// TODO: how to do this without a redirect
		[AllowAnonymous]
		public async Task<IActionResult> Post(int id)
		{
			var model = await _forumTasks.GetPostPosition(id, UserHas(PermissionTo.SeeRestrictedForums));
			if (model == null)
			{
				return NotFound();
			}

			return RedirectToPage("/Forum/Topics/Index", new { Id = model.TopicId, Highlight = id });
			//return await Topic(new TopicRequest
			//{
			//	Id = model.TopicId,
			//	Highlight = id
			//});
		}

		[HttpPost]
		[RequirePermission(PermissionTo.CreateForumPosts)]
		public IActionResult GeneratePreview()
		{
			var text = new StreamReader(Request.Body, Encoding.UTF8).ReadToEnd();
			var renderedText = RenderPost(text, true, false); // TODO: pass in bbcode flag

			return new ContentResult { Content = renderedText };
		}
	}
}
