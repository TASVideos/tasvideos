using System.Net.Mime;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
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

	public string IpAddress => PageContext.HttpContext.ActualIpAddress()?.ToString() ?? "";

	protected bool UserCanSeeRestricted => User.Has(PermissionTo.SeeRestrictedForums);

	protected IActionResult Home() => RedirectToPage("/Index");

	protected IActionResult AccessDenied() => RedirectToPage("/Account/AccessDenied");

	protected IActionResult Login() => BasePageRedirect("/Account/Login");

	protected IActionResult BasePageRedirect(string page, object? routeValues = null)
	{
		return !string.IsNullOrWhiteSpace(Request.ReturnUrl())
			? BaseReturnUrlRedirect()
			: RedirectToPage(page, routeValues);
	}

	protected IActionResult BaseRedirect(string page)
	{
		return !string.IsNullOrWhiteSpace(Request.ReturnUrl())
			? BaseReturnUrlRedirect()
			: Redirect(page);
	}

	protected IActionResult BaseReturnUrlRedirect(string? additionalParam = null)
	{
		var returnUrl = Request.ReturnUrl() + additionalParam;
		if (!string.IsNullOrWhiteSpace(returnUrl))
		{
			return Url.IsLocalUrl(returnUrl)
				? LocalRedirect(returnUrl)
				: Home();
		}

		return Home();
	}

	protected void AddErrors(IdentityResult result)
	{
		foreach (var error in result.Errors)
		{
			ModelState.AddModelError("", error.Description);
		}
	}

	protected async Task<bool> ConcurrentSave(ApplicationDbContext db, string successMessage, string errorMessage)
	{
		try
		{
			await db.SaveChangesAsync();
			if (!string.IsNullOrWhiteSpace(successMessage))
			{
				SuccessStatusMessage(successMessage);
			}

			return true;
		}
		catch (DbUpdateConcurrencyException)
		{
			if (!string.IsNullOrWhiteSpace(errorMessage))
			{
				ErrorStatusMessage(errorMessage + "\nThe resource may have already been deleted or updated");
			}

			return false;
		}
		catch (DbUpdateException)
		{
			if (!string.IsNullOrWhiteSpace(errorMessage))
			{
				ErrorStatusMessage(errorMessage + "\nThe resource cannot be deleted or updated");
			}

			return false;
		}
	}

	public List<SelectListItem> AvailablePermissions { get; } = PermissionUtil
		.AllPermissions()
		.ToDropDown()
		.OrderBy(p => p.Text)
		.WithDefaultEntry();

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

	protected JsonResult Json(object? obj)
	{
		return new JsonResult(obj);
	}

	protected IActionResult ZipFile(ZippedFile? file)
	{
		return file is not null
			? File(file.Data, MediaTypeNames.Application.Octet, $"{file.Path}.zip")
			: NotFound();
	}

	protected PageResult Rss()
	{
		var pageResult = Page();
		pageResult.ContentType = "application/rss+xml; charset=utf-8";
		return pageResult;
	}
}
