namespace TASVideos.Pages.Wiki.Legacy;

[AllowAnonymous]
public class PrivilegesModel : BasePageModel
{
	public IActionResult OnGet() => RedirectToPage("/Permissions/Index");
}
