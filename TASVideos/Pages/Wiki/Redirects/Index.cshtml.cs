namespace TASVideos.Pages.Wiki.Redirects;

[RequirePermission(PermissionTo.EditWikiRedirects)]
public class IndexModel(IWikiRedirectService wikiRedirectService) : BasePageModel
{
	public ICollection<WikiRedirect> Redirects { get; set; } = [];

	public async Task OnGet()
	{
		Redirects = await wikiRedirectService.GetAll();
	}

	public async Task<IActionResult> OnPostDelete(int id)
	{
		var redirect = await wikiRedirectService.GetById(id);
		if (redirect is null)
		{
			return NotFound();
		}

		var pageNameFrom = redirect.PageNameFrom;
		var pageNameTo = redirect.PageNameTo;
		var result = await wikiRedirectService.Delete(id);
		if (result == WikiRedirectDeleteResult.Success)
		{
			SuccessStatusMessage($"Redirect from '{pageNameFrom}' to '{pageNameTo}' deleted successfully");
		}
		else
		{
			ErrorStatusMessage($"Unable to delete redirect from '{pageNameFrom}' to '{pageNameTo}'");
		}

		return BasePageRedirect("Index");
	}
}
