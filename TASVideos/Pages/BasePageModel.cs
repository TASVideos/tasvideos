using System.Collections.Specialized;
using System.Net.Mime;
using System.Web;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace TASVideos.Pages;

public class BasePageModel : PageModel
{
	[TempData]
	public string? Message { get; set; }

	[TempData]
	public string? MessageType { get; set; }

	public void SuccessStatusMessage(string message)
	{
		Message = message;
		MessageType = "success";
	}

	public void ErrorStatusMessage(string message)
	{
		Message = message;
		MessageType = "danger";
	}

	public void ClearStatusMessage()
	{
		Message = null;
		MessageType = null;
	}

	protected void SetMessage(SaveResult result, string successMessage, string failureMessage)
	{
		if (result.IsSuccess())
		{
			if (!string.IsNullOrWhiteSpace(successMessage))
			{
				SuccessStatusMessage(successMessage);
			}
		}
		else if (!string.IsNullOrWhiteSpace(failureMessage))
		{
			var addOn = result == SaveResult.ConcurrencyFailure
				? "The resource may have already been deleted or updated"
				: "The resource cannot be deleted or updated";
			ErrorStatusMessage($"{failureMessage}\n{addOn}");
		}
	}

	protected void SetMessage(bool success, string successMessage, string failureMessage)
	{
		if (success)
		{
			SuccessStatusMessage(successMessage);
		}
		else
		{
			ErrorStatusMessage(failureMessage);
		}
	}

	public string IpAddress => PageContext.HttpContext.ActualIpAddress()?.ToString() ?? "";

	protected bool UserCanSeeRestricted => User.Has(PermissionTo.SeeRestrictedForums);

	protected IActionResult Home() => RedirectToPage("/Index");

	protected IActionResult AccessDenied() => RedirectToPage("/Account/AccessDenied");

	protected IActionResult Login() => BasePageRedirect("/Account/Login");

	protected IActionResult BasePageRedirect(string page, object? routeValues = null)
		=> !string.IsNullOrWhiteSpace(Request.ReturnUrl())
			? BaseReturnUrlRedirect()
			: RedirectToPage(page, routeValues);

	protected IActionResult BaseRedirect(string page)
		=> !string.IsNullOrWhiteSpace(Request.ReturnUrl())
			? BaseReturnUrlRedirect()
			: Redirect(page);

	protected IActionResult BaseReturnUrlRedirect(NameValueCollection? additionalParams = null)
	{
		var returnUrl = Request.ReturnUrl();

		returnUrl = AddAdditionalParams(returnUrl, additionalParams);

		if (!string.IsNullOrWhiteSpace(returnUrl))
		{
			return Url.IsLocalUrl(returnUrl)
				? LocalRedirect(returnUrl)
				: Home();
		}

		return Home();
	}

	internal static string AddAdditionalParams(string relativeUrl, NameValueCollection? additionalParams = null)
	{
		if (additionalParams is null)
		{
			return relativeUrl;
		}

		try
		{
			var uri = new UriBuilder($"https://localhost/{relativeUrl.TrimStart('/')}");
			var returnQuery = HttpUtility.ParseQueryString(uri.Query);
			foreach (string? key in additionalParams.AllKeys)
			{
				returnQuery[key] = additionalParams[key];
			}

			uri.Query = returnQuery.ToString();
			relativeUrl = uri.Path + uri.Query;
		}
		catch
		{
		}

		return relativeUrl;
	}

	protected void AddErrors(IdentityResult result)
	{
		foreach (var error in result.Errors)
		{
			ModelState.AddModelError("", error.Description);
		}
	}

	public IEnumerable<SelectListItem> AvailablePermissions { get; } = PermissionUtil
		.AllPermissions()
		.ToDropDown()
		.OrderBy(p => p.Text);

	protected PartialViewResult ToDropdownResult(IEnumerable<SelectListItem> items, bool includeEmpty)
	{
		if (includeEmpty)
		{
			items = items.WithDefaultEntry();
		}

		return new PartialViewResult
		{
			ViewName = "_DropdownItems",
			ViewData = new ViewDataDictionary<IEnumerable<SelectListItem>>(ViewData, items)
		};
	}

	protected JsonResult Json(object? obj) => new(obj);

	protected IActionResult ZipFile(ZippedFile? file)
		=> file is not null
			? File(file.Data, MediaTypeNames.Application.Octet, $"{file.Path}.zip")
			: NotFound();

	protected PageResult Rss()
	{
		var pageResult = Page();
		pageResult.ContentType = "application/rss+xml; charset=utf-8";
		return pageResult;
	}
}
