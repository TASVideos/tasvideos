using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TASVideos.Pages.Wiki.Redirects;

[RequirePermission(PermissionTo.EditWikiRedirects)]
public class EditModel(ApplicationDbContext db) : BasePageModel
{
	[FromRoute]
	public int Id { get; set; }

	[BindProperty]
	public WikiRedirect WikiRedirect { get; set; } = new();

	public async Task<IActionResult> OnGet()
	{
		var redirect = await db.WikiRedirects.FindAsync(Id);
		if (redirect == null)
		{
			return NotFound();
		}

		WikiRedirect = redirect;

		return Page();
	}

	public async Task<IActionResult> OnPost()
	{
		if (!ModelState.IsValid)
		{
			return Page();
		}

		if (WikiRedirect.PageNameFrom == WikiRedirect.PageNameTo)
		{
			ModelState.AddModelError("", "Page names must be different");
		}

		if (!WikiHelper.IsValidWikiPageName(WikiRedirect.PageNameFrom))
		{
			ModelState.AddModelError($"{nameof(WikiRedirect)}.{nameof(WikiRedirect.PageNameFrom)}", "Page name is not valid");
		}

		if (!WikiHelper.IsValidWikiPageName(WikiRedirect.PageNameTo))
		{
			ModelState.AddModelError($"{nameof(WikiRedirect)}.{nameof(WikiRedirect.PageNameTo)}", "Page name is not valid");
		}

		if (!ModelState.IsValid)
		{
			return Page();
		}

		if (await db.WikiRedirects.AnyAsync(r => r.Id != Id && r.PageNameFrom == WikiRedirect.PageNameTo))
		{
			ModelState.AddModelError($"{nameof(WikiRedirect)}.{nameof(WikiRedirect.PageNameTo)}", $"Page name '{WikiRedirect.PageNameTo}' already redirects to a different page. Avoid multiple redirects.");
			return Page();
		}

		var redirect = await db.WikiRedirects.FindAsync(Id);
		if (redirect == null)
		{
			return NotFound();
		}

		redirect.PageNameFrom = WikiRedirect.PageNameFrom;
		redirect.PageNameTo = WikiRedirect.PageNameTo;

		try
		{
			await db.SaveChangesAsync();
		}
		catch (DbUpdateConcurrencyException)
		{
			ErrorStatusMessage("Unable to edit redirect due to an unknown error");
			return Page();
		}
		catch (DbUpdateException ex)
		{
			if (ex.InnerException?.Message.Contains("unique constraint") ?? false)
			{
				ModelState.AddModelError($"{nameof(WikiRedirect)}.{nameof(WikiRedirect.PageNameFrom)}", $"Redirect from '{WikiRedirect.PageNameFrom}' already exists");
				return Page();
			}

			ErrorStatusMessage("Unable to edit redirect due to an unknown error");
			return Page();
		}

		SuccessStatusMessage("Redirect successfully updated.");
		return BasePageRedirect("Index");
	}
}
