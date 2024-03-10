using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TASVideos.Pages.Wiki.Legacy;

[AllowAnonymous]
public class SubmitMovieModel : BasePageModel
{
	public IActionResult OnGet()
	{
		return RedirectToPage("/Submissions/Submit");
	}
}
