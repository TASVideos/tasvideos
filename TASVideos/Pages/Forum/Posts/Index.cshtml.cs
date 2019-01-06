using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using TASVideos.Data.Entity;
using TASVideos.Tasks;

namespace TASVideos.Pages.Forum.Posts
{
	// TODO: how to do this without a redirect
	[AllowAnonymous]
	public class IndexModel : BasePageModel
	{
		private readonly ForumTasks _forumTasks;

		public IndexModel(
			ForumTasks forumTasks,
			UserTasks userTasks) 
			: base(userTasks)
		{
			_forumTasks = forumTasks;
		}

		[FromRoute]
		public int Id { get; set; }

		public async Task<IActionResult> OnGet()
		{
			var model = await _forumTasks.GetPostPosition(Id, UserHas(PermissionTo.SeeRestrictedForums));
			if (model == null)
			{
				return NotFound();
			}

			return RedirectToPage(
				"/Forum/Topics/Index", 
				new
				{
					Id = model.TopicId, 
					Highlight = Id, 
					CurrentPage = model.Page
				});
		}
	}
}
