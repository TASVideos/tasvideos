using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using TASVideos.Data.Entity;
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
		//[AllowAnonymous]
		//public async Task<IActionResult> LegacyPost(int p)
		//{
		//	return await Post(p);
		//}

		//// TODO: how to do this without a redirect
		//[AllowAnonymous]
		//public async Task<IActionResult> Post(int id)
		//{
		//	var model = await _forumTasks.GetPostPosition(id, UserHas(PermissionTo.SeeRestrictedForums));
		//	if (model == null)
		//	{
		//		return NotFound();
		//	}

		//	return RedirectToPage("/Forum/Topics/Index", new { Id = model.TopicId, Highlight = id, CurrentPage = model.Page });
		//	//return await Topic(new TopicRequest
		//	//{
		//	//	Id = model.TopicId,
		//	//	Highlight = id
		//	//});
		//}
	}
}
