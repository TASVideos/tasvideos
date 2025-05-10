namespace TASVideos.Pages.Submissions;

[RequirePermission(PermissionTo.DeleteSubmissions)]
public class DeleteModel(IQueueService queueService, IExternalMediaPublisher publisher) : BasePageModel
{
	[FromRoute]
	public int Id { get; set; }

	public string Title { get; set; } = "";

	public async Task<IActionResult> OnGet()
	{
		var result = await queueService.CanDeleteSubmission(Id);

		switch (result.Status)
		{
			case DeleteSubmissionResult.DeleteStatus.NotFound:
				return NotFound();
			case DeleteSubmissionResult.DeleteStatus.NotAllowed:
				return BadRequest(result.ErrorMessage);
			default:
				Title = result.SubmissionTitle;
				return Page();
		}
	}

	public async Task<IActionResult> OnPost()
	{
		if (!ModelState.IsValid)
		{
			return Page();
		}

		var result = await queueService.DeleteSubmission(Id);

		if (result.Status == DeleteSubmissionResult.DeleteStatus.NotFound)
		{
			ErrorStatusMessage($"Submission {Id} not found");
			return RedirectToPage("View", new { Id });
		}

		if (result.Status == DeleteSubmissionResult.DeleteStatus.NotAllowed)
		{
			ErrorStatusMessage(result.ErrorMessage);
			return RedirectToPage("View", new { Id });
		}

		if (result.Status == DeleteSubmissionResult.DeleteStatus.Success)
		{
			await publisher.AnnounceSubmissionDelete(result.SubmissionTitle, Id);
		}

		return BaseRedirect("/Subs-List");
	}
}
