using System.IO;
using System.Net;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
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

		protected string RenderHtml(string text)
		{
			return RenderPost(text, false, true);
		}

		protected string RenderBbcode(string text)
		{
			return RenderPost(text, true, false);
		}

		protected string RenderSignature(string? text)
		{
			return RenderBbcode(text ?? ""); // Bbcode on, Html off hardcoded, do we want this to be configurable?
		}

		protected async Task<byte[]> FormFileToBytes(IFormFile formFile)
		{
			await using var memoryStream = new MemoryStream();
			await formFile.CopyToAsync(memoryStream);
			return memoryStream.ToArray();
		}
	}
}
