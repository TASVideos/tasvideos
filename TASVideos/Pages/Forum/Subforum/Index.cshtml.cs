using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TASVideos.Data.Entity;
using TASVideos.Models;
using TASVideos.Tasks;

namespace TASVideos.Pages.Forum.Subforum
{
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

		[FromQuery]
		public ForumRequest Search { get; set; }

		[FromRoute]
		public int Id { get; set; }

		public ForumModel Forum { get; set; }

		public async Task<IActionResult> OnGet()
		{
			Forum = await _forumTasks
				.GetForumForDisplay(Id, Search, UserHas(PermissionTo.SeeRestrictedForums));

			if (Forum == null)
			{
				return NotFound();
			}

			Forum.Description = RenderHtml(Forum.Description);

			return Page();
		}
	}
}
