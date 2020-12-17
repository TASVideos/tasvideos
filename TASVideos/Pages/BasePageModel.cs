using System.IO;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using TASVideos.ForumEngine;

namespace TASVideos.Pages
{
	public class BasePageModel : PageModel
	{
		protected string IpAddress => Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "";

		protected IActionResult Home() => RedirectToPage("/Index");

		protected IActionResult AccessDenied() => RedirectToPage("/Account/AccessDenied");

		protected IActionResult Login() => new RedirectToPageResult("Login");

		protected IActionResult RedirectToLocal(string? returnUrl)
		{
			returnUrl ??= "";
			return Url.IsLocalUrl(returnUrl)
				? LocalRedirect(returnUrl)
				: Home();
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
			using var writer = new StringWriter();
			parsed.WriteHtml(writer);
			return writer.ToString();
		}

		protected string RenderBbcode(string text)
			=> RenderPost(text, true, false);

		protected string RenderSignature(string? text)
		{
			// Bbcode on, Html off hardcoded, do we want this to be configurable?
			return RenderBbcode(text ?? "");
		}
	}
}
