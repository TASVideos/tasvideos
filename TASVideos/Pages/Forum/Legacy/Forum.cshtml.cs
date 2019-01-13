using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using TASVideos.Tasks;

namespace TASVideos.Pages.Forum.Legacy
{
	// Handles legacy forum links to viewForum.php
	[AllowAnonymous]
	public class ForumModel : BasePageModel
	{
		public ForumModel(
			UserTasks userTasks) 
			: base(userTasks)
		{
		}

		[FromQuery]
		public int? F { get; set; }

		public IActionResult OnGet()
		{
			if (!F.HasValue)
			{
				return NotFound();
			}

			return RedirectToPage("/Forum/Subforum/Index", new { Id = F.Value });
		}
	}
}
