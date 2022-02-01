using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TASVideos.Core.Services;
using TASVideos.Core.Services.ExternalMediaPublisher;
using TASVideos.Core.Services.Youtube;
using TASVideos.Data;
using TASVideos.Data.Entity;

namespace TASVideos.Pages.Publications;

[RequirePermission(PermissionTo.Unpublish)]
public class UnpublishModel : BasePageModel
{
	private readonly ApplicationDbContext _db;
	private readonly IPublicationMaintenanceLogger _publicationMaintenanceLogger;
	private readonly IYoutubeSync _youtubeSync;
	private readonly ExternalMediaPublisher _publisher;
	private readonly ISubmissionService _queueService;

	public UnpublishModel(
		ApplicationDbContext db,
		IPublicationMaintenanceLogger publicationMaintenanceLogger,
		IYoutubeSync youtubeSync,
		ExternalMediaPublisher publisher,
		ISubmissionService queueService)
	{
		_db = db;
		_publicationMaintenanceLogger = publicationMaintenanceLogger;
		_youtubeSync = youtubeSync;
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
		var publication = await _db.Publications
			.Include(p => p.PublicationAwards)
			.Include(p => p.Authors)
			.Include(p => p.Files)
			.Include(p => p.PublicationFlags)
			.Include(p => p.PublicationRatings)
			.Include(p => p.PublicationTags)
			.Include(p => p.PublicationUrls)
			.Include(p => p.Submission)
			.Include(p => p.ObsoletedMovies)
			.SingleOrDefaultAsync(p => p.Id == Id);

		if (publication == null)
		{
			ErrorStatusMessage($"Publication {Id} not found");
			return RedirectToPage("View", new { Id });
		}

		if (publication.PublicationAwards.Any())
		{
			ErrorStatusMessage($"Publication {Id} has awards and cannot be unpublished");
			return RedirectToPage("View", new { Id });
		}

		publication.Authors.Clear();
		publication.Files.Clear();
		publication.PublicationFlags.Clear();
		publication.PublicationRatings.Clear();
		publication.PublicationTags.Clear();

		var youtubeUrls = publication.PublicationUrls
			.Select(pu => pu.Url)
			.Where(url => _youtubeSync.IsYoutubeUrl(url))
			.ToList();

		// Add to submission status history
		// Reset the submission status
		// TVA post?
		// Youtube sync - set urls to unlisted
		// Youtube sync - if there was an obsoleted movie, sync it
		publication.PublicationUrls.Clear();

		var result = await ConcurrentSave(_db, $"{publication.Title} Unpublished", $"Unable to unpublish {Title}");
		if (result)
		{
			await _publicationMaintenanceLogger.Log(Id, User.GetUserId(), "Unpublished");
			await _publisher.AnnounceUnpublish(publication.Title, publication.Id);
		}

		return RedirectToPage("View", new { Id });
	}
}
