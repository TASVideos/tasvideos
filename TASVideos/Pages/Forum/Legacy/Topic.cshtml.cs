using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using TASVideos.Data.Entity;
using TASVideos.Tasks;

namespace TASVideos.Pages.Forum.Legacy
{
	// Handles legacy forum links to viewTopic.php
	[AllowAnonymous]
	public class TopicModel : BasePageModel
	{
		private readonly ForumTasks _forumTasks;

		public TopicModel(ForumTasks forumTasks)
		{
			_forumTasks = forumTasks;
		}

		[FromQuery]
		public int? P { get; set; }

		[FromQuery]
		public int? T { get; set; }

		public async Task<IActionResult> OnGet()
		{
			if (!P.HasValue && !T.HasValue)
			{
				return NotFound();
			}

			if (P.HasValue)
			{
				var model = await _forumTasks.GetPostPosition(P.Value, User.Has(PermissionTo.SeeRestrictedForums));
				if (model == null)
				{
					return NotFound();
				}

				return RedirectToPage(
					"/Forum/Topics/Index", 
					new
					{
						Id = model.TopicId, 
						Highlight = P, 
						CurrentPage = model.Page
					});
			}

			return RedirectToPage("/Forum/Topics/Index", new { Id = T });
		}
	}
}
