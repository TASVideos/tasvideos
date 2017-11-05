using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace TASVideos.Controllers
{
    public class BaseController : Controller
    {
		protected void AddErrors(IdentityResult result)
		{
			foreach (var error in result.Errors)
			{
				ModelState.AddModelError(string.Empty, error.Description);
			}
		}

		protected IActionResult RedirectToLocal(string returnUrl)
		{
			if (Url.IsLocalUrl(returnUrl))
			{
				return Redirect(returnUrl);
			}

			return RedirectToAction(nameof(HomeController.Index), "Home");
		}
	}
}