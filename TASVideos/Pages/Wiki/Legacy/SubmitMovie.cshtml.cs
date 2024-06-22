namespace TASVideos.Pages.Wiki.Legacy;

[AllowAnonymous]
public class SubmitMovieModel : BasePageModel
{
	public IActionResult OnGet() => RedirectToPage("/Submissions/Submit");
}
