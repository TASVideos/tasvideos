namespace TASVideos.Pages.Wiki.Legacy;

[AllowAnonymous]
public class SubmitMovieModel : BasePageModel
{
	public IActionResult OnGet()
	{
		return RedirectToPage("/Submissions/Submit");
	}
}
