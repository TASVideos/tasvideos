namespace TASVideos.Pages.Publications;

[RequirePermission(PermissionTo.Unpublish)]
public class UnpublishModel(IExternalMediaPublisher publisher, IQueueService queueService) : BasePageModel
{
	[FromRoute]
	public int Id { get; set; }

	public string Title { get; set; } = "";

	[StringLength(250)]
	[BindProperty]
	public string Reason { get; set; } = "";

	public async Task<IActionResult> OnGet()
	{
		var result = await queueService.CanUnpublish(Id);

		switch (result.Status)
		{
			case UnpublishResult.UnpublishStatus.NotFound:
				return NotFound();
			case UnpublishResult.UnpublishStatus.NotAllowed:
				return BadRequest(result.ErrorMessage);
			default:
				Title = result.PublicationTitle;
				return Page();
		}
	}

	public async Task<IActionResult> OnPost()
	{
		if (!ModelState.IsValid)
		{
			return Page();
		}

		var result = await queueService.Unpublish(Id);

		if (result.Status == UnpublishResult.UnpublishStatus.NotFound)
		{
			ErrorStatusMessage($"Publication {Id} not found");
			return RedirectToPage("View", new { Id });
		}

		if (result.Status == UnpublishResult.UnpublishStatus.NotAllowed)
		{
			ErrorStatusMessage(result.ErrorMessage);
			return RedirectToPage("View", new { Id });
		}

		if (result.Status == UnpublishResult.UnpublishStatus.Success)
		{
			await publisher.AnnounceUnpublish(result.PublicationTitle, Id, Reason);
		}

		return BaseRedirect("/Subs-List");
	}
}
