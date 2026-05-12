namespace TASVideos.Pages.Wiki.Redirects;

[RequirePermission(PermissionTo.EditWikiRedirects)]
public class EditModel(IWikiRedirectService wikiRedirectService) : BasePageModel
{
	[FromRoute]
	public int Id { get; set; }

	[BindProperty]
	public WikiRedirect WikiRedirect { get; set; } = new();

	public async Task<IActionResult> OnGet()
	{
		var redirect = await wikiRedirectService.GetById(Id);
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

		var result = await wikiRedirectService.Edit(Id, WikiRedirect);
		switch (result)
		{
			case WikiRedirectAddEditResult.NotFound:
				return NotFound();
			case WikiRedirectAddEditResult.SamePageName:
				ModelState.AddModelError("", "Page names must be different");
				return Page();
			case WikiRedirectAddEditResult.InvalidPageNameFrom:
				ModelState.AddModelError($"{nameof(WikiRedirect)}.{nameof(WikiRedirect.PageNameFrom)}", "Page name is not valid");
				return Page();
			case WikiRedirectAddEditResult.InvalidPageNameTo:
				ModelState.AddModelError($"{nameof(WikiRedirect)}.{nameof(WikiRedirect.PageNameTo)}", "Page name is not valid");
				return Page();
			case WikiRedirectAddEditResult.ChainedRedirectTo:
				ModelState.AddModelError($"{nameof(WikiRedirect)}.{nameof(WikiRedirect.PageNameTo)}", $"Page name '{WikiRedirect.PageNameTo}' already redirects to a different page. Avoid chaining redirects.");
				return Page();
			case WikiRedirectAddEditResult.ChainedRedirectFrom:
				ModelState.AddModelError($"{nameof(WikiRedirect)}.{nameof(WikiRedirect.PageNameFrom)}", $"Another page already redirects to '{WikiRedirect.PageNameFrom}'. Avoid chaining redirects.");
				return Page();
			case WikiRedirectAddEditResult.DuplicateSource:
				ModelState.AddModelError($"{nameof(WikiRedirect)}.{nameof(WikiRedirect.PageNameFrom)}", $"Redirect from '{WikiRedirect.PageNameFrom}' already exists");
				return Page();
			case WikiRedirectAddEditResult.Fail:
				ErrorStatusMessage("Unable to edit redirect due to an unknown error");
				return Page();
		}

		SuccessStatusMessage("Redirect successfully updated.");
		return BasePageRedirect("Index");
	}
}
