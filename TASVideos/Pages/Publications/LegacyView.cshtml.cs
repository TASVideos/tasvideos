using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TASVideos.Pages.Publications
{
	// Handles legacy {id}M.html links
	[AllowAnonymous]
	public class LegacyViewPageModel : PageModel
	{
		[FromRoute]
		public int Id { get; set; }

		public IActionResult OnGet()
		{
			return RedirectPermanent($"{Id}M");
		}
	}
}
