using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TASVideos.Pages.Forum.Legacy
{
	// Handles legacy forum links to viewForum.php
	[AllowAnonymous]
	public class ForumModel : BasePageModel
	{
		[FromQuery]
		public int? F { get; set; }

		[FromRoute]
		public int? Id { get; set; }

		public IActionResult OnGet()
		{
			if (!F.HasValue && !Id.HasValue)
			{
				return NotFound();
			}

			return RedirectToPage("/Forum/Subforum/Index", new { Id = F ?? Id!.Value });
		}
	}
}
