using System.IO;
using System.Net;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using TASVideos.ForumEngine;

namespace TASVideos.Pages
{
	public class BasePageModel : PageModel
	{
		public string BaseUrl => $"{Request.Scheme}://{Request.Host}{Request.PathBase}";

		protected IPAddress IpAddress => Request.HttpContext.Connection.RemoteIpAddress;

		protected IActionResult Home()
		{
			return RedirectToPage("/Index");
		}

		protected IActionResult AccessDenied()
		{
			return RedirectToPage("/Account/AccessDenied");
		}

		protected IActionResult Login()
		{
			return new RedirectToPageResult("Login");
		}

		protected IActionResult RedirectToLocal(string returnUrl)
		{
			if (Url.IsLocalUrl(returnUrl))
			{
				return LocalRedirect(returnUrl);
			}

			return Home();
		}

		protected void AddErrors(IdentityResult result)
		{
			foreach (var error in result.Errors)
			{
				ModelState.AddModelError(string.Empty, error.Description);
			}
		}

		protected string RenderPost(string text, bool useBbCode, bool useHtml)
		{
			var parsed = PostParser.Parse(text, useBbCode, useHtml);
			using (var writer = new StringWriter())
			{
				parsed.WriteHtml(writer);
				return writer.ToString();
			}
		}

		protected string RenderHtml(string text)
		{
			return RenderPost(text, false, true);
		}

		protected string RenderBbcode(string text)
		{
			return RenderPost(text, true, false);
		}

		protected string RenderSignature(string text)
		{
			return RenderBbcode(text); // Bbcode on, Html off hardcoded, do we want this to be configurable?
		}
	}
}
