using TASVideos.Core.Services.Wiki;

namespace TASVideos.Pages.Wiki;

[RequirePermission(PermissionTo.MoveWikiPages)]
public class MoveModel(IWikiPages wikiPages, IExternalMediaPublisher publisher, IWikiRedirectService wikiRedirectService, ApplicationDbContext db) : BasePageModel
{
	[FromQuery]
	public string? Path { get; set; }

	[BindProperty]
	public string OriginalPageName { get; set; } = "";

	[BindProperty]
	[ValidWikiPageName]
	public string DestinationPageName { get; set; } = "";

	[BindProperty]
	public bool LeaveRedirect { get; init; } = true;

	public async Task<IActionResult> OnGet()
	{
		if (!string.IsNullOrWhiteSpace(Path))
		{
			Path = Path.Trim('/');
			if (await wikiPages.Exists(Path))
			{
				OriginalPageName = Path;
				DestinationPageName = Path;
				return Page();
			}
		}

		return NotFound();
	}

	public async Task<IActionResult> OnPost()
	{
		if (!ModelState.IsValid)
		{
			return Page();
		}

		OriginalPageName = OriginalPageName.Trim('/');
		DestinationPageName = DestinationPageName.Trim('/');

		if (!await wikiPages.CanMove(OriginalPageName, DestinationPageName))
		{
			ModelState.AddModelError("", "Either the original page does not exist, or the destination page already exists or has an invalid format.");
			return Page();
		}

		var transaction = await db.BeginTransactionAsync();

		if (LeaveRedirect)
		{
			var redirectResult = await wikiRedirectService.Add(new WikiRedirect
			{
				PageNameFrom = OriginalPageName,
				PageNameTo = DestinationPageName
			});

			switch (redirectResult)
			{
				case WikiRedirectAddEditResult.ChainedRedirectFrom:
					ModelState.AddModelError("", $"Another page already redirects to '{OriginalPageName}'. Avoid chaining redirects.");
					return Page();
				case WikiRedirectAddEditResult.ChainedRedirectTo:
					ModelState.AddModelError("", $"Page name '{DestinationPageName}' already redirects to a different page. Avoid chaining redirects.");
					return Page();
				case WikiRedirectAddEditResult.DuplicateSource:
					ModelState.AddModelError("", $"Redirect from '{OriginalPageName}' already exists. Avoid overlapping redirects.");
					return Page();
				case WikiRedirectAddEditResult.Fail:
					ErrorStatusMessage("Unable to edit redirects due to an unknown error");
					return Page();
			}

			SuccessStatusMessage("Redirect successfully created.");
		}

		var result = await wikiPages.Move(OriginalPageName, DestinationPageName, User.GetUserId());

		if (result)
		{
			await transaction.CommitAsync();

			await publisher.SendWiki(
				$"Page {OriginalPageName} moved to [{DestinationPageName}]({{0}}) by {User.Name()}",
				"",
				DestinationPageName);

			return BaseRedirect("/" + DestinationPageName);
		}

		ModelState.AddModelError("", "Unable to move page, the page may have been modified during the saving of this operation.");
		return Page();
	}
}
