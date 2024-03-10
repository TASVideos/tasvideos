using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TASVideos.Pages.Wiki.Legacy;

[AllowAnonymous]
public class PrivilegesModel : BasePageModel
{
	public IActionResult OnGet()
	{
		return RedirectToPage("/Permissions/Index");
	}
}
