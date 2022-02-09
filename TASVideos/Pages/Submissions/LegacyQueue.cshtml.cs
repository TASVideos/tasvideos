using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TASVideos.Pages.Submissions;

// Handles legacy queue.cgi links
[AllowAnonymous]
public class LegacyQueueModel : PageModel
{
	[FromQuery]
	public string? Mode { get; set; }

	[FromQuery]
	public string? Type { get; set; }

	[FromQuery]
	public int? Id { get; set; }

	public IActionResult OnGet()
	{
		if (string.Equals(Mode, "submit", StringComparison.CurrentCultureIgnoreCase))
		{
			return RedirectToPage("/Submissions/Submit");
		}

		if (string.Equals(Mode, "list", StringComparison.CurrentCultureIgnoreCase))
		{
			string? user = null;
			if (Type == "own" && User.IsLoggedIn())
			{
				user = User.Name();
			}

			return RedirectToPage("/Submissions/Index", new { User = user });
		}

		if (string.Equals(Mode, "edit", StringComparison.CurrentCultureIgnoreCase)
			&& Id.HasValue)
		{
			return RedirectToPage("/Submissions/Edit", new { Id = Id.Value });
		}

		// mode=view is optional
		// http://tasvideos.org/queue.cgi?id=1
		// http://tasvideos.org/queue.cgi?mode=view&id=1
		if ((string.Equals(Mode, "view", StringComparison.CurrentCultureIgnoreCase) || string.IsNullOrWhiteSpace(Mode))
			&& Id.HasValue)
		{
			return RedirectToPage("/Submissions/View", new { Id = Id.Value });
		}

		return RedirectToPage("/Submissions/Index");
	}
}
