using Microsoft.AspNetCore.Mvc;
using TASVideos.Core.Services;
using TASVideos.Core.Services.ExternalMediaPublisher;
using TASVideos.Data.Entity;

namespace TASVideos.Pages.Submissions;

[RequirePermission(PermissionTo.DeleteSubmissions)]
public class DeleteModel : BasePageModel
{
	private readonly IQueueService _queueService;
	private readonly ExternalMediaPublisher _publisher;

	public DeleteModel(
		IQueueService queueService,
		ExternalMediaPublisher publisher)
	{
		_queueService = queueService;
		_publisher = publisher;
	}

	[FromRoute]
	public int Id { get; set; }

	public string Title { get; set; } = "";

	public async Task<IActionResult> OnGet()
	{
		var result = await _queueService.CanDeleteSubmission(Id);

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

		var result = await _queueService.DeleteSubmission(Id);

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
			await _publisher.AnnounceSubmissionDelete(result.SubmissionTitle, Id);
		}

		return BaseRedirect("/Subs-List");
	}
}
