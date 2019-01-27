using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using TASVideos.Services;
using TASVideos.Tasks;

namespace TASVideos.Pages.Forum.Legacy
{
	// Handles legacy forum links to viewForum.php
	[AllowAnonymous]
	public class ForumModel : BasePageModel
	{
		public ForumModel(
			UserManager userManager) 
			: base(userManager)
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
