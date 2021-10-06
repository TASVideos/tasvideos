using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TASVideos.Pages.Submissions
{
	// Handles legacy {id}S.html links
	[AllowAnonymous]
	public class LegacyViewPageModel : PageModel
	{
		[FromRoute]
		public int Id { get; set; }

		public IActionResult OnGet()
		{
			return RedirectPermanent($"{Id}S");
		}
	}
}
