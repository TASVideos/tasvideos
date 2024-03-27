namespace TASVideos.Pages.Wiki.Legacy;

[AllowAnonymous]
public class PrivilegesModel : BasePageModel
{
	public IActionResult OnGet()
	{
		return RedirectToPage("/Permissions/Index");
	}
}
