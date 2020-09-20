using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TASVideos.Pages.Submissions
{
	// Handles legacy queue.cgi links
	[AllowAnonymous]
	public class LegacyQueueModel : PageModel
	{
		[FromQuery]
		public string? Mode { get; set; }

		[FromQuery]
		public string? Type { get; set; }

		public IActionResult OnGet()
		{
			if (string.Equals(Mode, "submit", StringComparison.CurrentCultureIgnoreCase))
			{
				return RedirectToPage("/Submissions/Submit");
			}

			if (string.Equals(Mode, "list", StringComparison.CurrentCultureIgnoreCase))
			{
				string? user = null;
				if (Type == "own" && User.Identity.IsAuthenticated)
				{
					user = User.Identity.Name;
				}

				return RedirectToPage("/Submissions/Index", new { User = user });
			}

			return NotFound();
		}
	}
}
