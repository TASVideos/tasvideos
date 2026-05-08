namespace TASVideos.Pages.Wiki.Redirects;

[RequirePermission(PermissionTo.EditWikiRedirects)]
public class CreateModel(ApplicationDbContext db) : BasePageModel
{
	[BindProperty]
	public WikiRedirect WikiRedirect { get; set; } = new();

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

		if (await db.WikiRedirects.AnyAsync(r => r.PageNameFrom == WikiRedirect.PageNameTo))
		{
			ModelState.AddModelError($"{nameof(WikiRedirect)}.{nameof(WikiRedirect.PageNameTo)}", $"Page name '{WikiRedirect.PageNameTo}' already redirects to a different page. Avoid multiple redirects.");
			return Page();
		}

		db.WikiRedirects.Add(WikiRedirect);

		try
		{
			await db.SaveChangesAsync();
		}
		catch (DbUpdateConcurrencyException)
		{
			ErrorStatusMessage("Unable to create redirect due to an unknown error");
			return Page();
		}
		catch (DbUpdateException ex)
		{
			if (ex.InnerException?.Message.Contains("unique constraint") ?? false)
			{
				ModelState.AddModelError($"{nameof(WikiRedirect)}.{nameof(WikiRedirect.PageNameFrom)}", $"Redirect from '{WikiRedirect.PageNameFrom}' already exists");
				return Page();
			}

			ErrorStatusMessage("Unable to create redirect due to an unknown error");
			return Page();
		}

		SuccessStatusMessage("Redirect successfully created.");
		return BasePageRedirect("Index");
	}
}
