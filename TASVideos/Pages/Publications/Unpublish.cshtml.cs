using Microsoft.AspNetCore.Mvc;
using TASVideos.Core.Services;
using TASVideos.Core.Services.ExternalMediaPublisher;
using TASVideos.Data.Entity;

namespace TASVideos.Pages.Publications;

[RequirePermission(PermissionTo.Unpublish)]
public class UnpublishModel : BasePageModel
{
	private readonly IPublicationMaintenanceLogger _publicationMaintenanceLogger;
	private readonly ExternalMediaPublisher _publisher;
	private readonly ISubmissionService _queueService;

	public UnpublishModel(
		IPublicationMaintenanceLogger publicationMaintenanceLogger,
		ExternalMediaPublisher publisher,
		ISubmissionService queueService)
	{
		_publicationMaintenanceLogger = publicationMaintenanceLogger;
		_publisher = publisher;
		_queueService = queueService;
	}

	[FromRoute]
	public int Id { get; set; }

	public string Title { get; set; } = "";

	public async Task<IActionResult> OnGet()
	{
		var result = await _queueService.CanUnpublish(Id);

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
		// TODO: ask for a reason on publication form for pub maintenance logs
		var result = await _queueService.Unpublish(Id);

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
			await _publicationMaintenanceLogger.Log(Id, User.GetUserId(), "Unpublished");
			await _publisher.AnnounceUnpublish(result.PublicationTitle, Id);
		}

		return RedirectToPage("View", new { Id });
	}
}
