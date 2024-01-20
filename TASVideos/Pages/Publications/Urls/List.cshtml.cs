using Microsoft.AspNetCore.Mvc;
using TASVideos.Core.Services;
using TASVideos.Core.Services.ExternalMediaPublisher;
using TASVideos.Core.Services.Youtube;
using TASVideos.Data;
using TASVideos.Data.Entity;

namespace TASVideos.Pages.Publications.Urls;

[RequirePermission(PermissionTo.EditPublicationFiles)]
public class ListUrlsModel : BasePageModel
{
	private readonly ApplicationDbContext _db;
	private readonly ExternalMediaPublisher _publisher;
	private readonly IYoutubeSync _youtubeSync;
	private readonly IPublicationMaintenanceLogger _publicationMaintenanceLogger;

	public ListUrlsModel(
		ApplicationDbContext db,
		ExternalMediaPublisher publisher,
		IYoutubeSync youtubeSync,
		IPublicationMaintenanceLogger publicationMaintenanceLogger)
	{
		_db = db;
		_publisher = publisher;
		_youtubeSync = youtubeSync;
		_publicationMaintenanceLogger = publicationMaintenanceLogger;
	}

	[FromRoute]
	public int PublicationId { get; set; }

	public string Title { get; set; } = "";

	public ICollection<PublicationUrl> CurrentUrls { get; set; } = new List<PublicationUrl>();

	public async Task<IActionResult> OnGet()
	{
		var title = await _db.Publications
			.Where(p => p.Id == PublicationId)
			.Select(p => p.Title)
			.SingleOrDefaultAsync();

		if (title is null)
		{
			return NotFound();
		}

		Title = title;
		CurrentUrls = await _db.PublicationUrls
			.Where(u => u.PublicationId == PublicationId)
			.ToListAsync();

		return Page();
	}

	public async Task<IActionResult> OnPostDelete(int urlId)
	{
		var url = await _db.PublicationUrls
			.SingleOrDefaultAsync(pf => pf.Id == urlId);

		if (url != null)
		{
			_db.PublicationUrls.Remove(url);
			string log = $"Deleted {url.DisplayName} {url.Type} URL {url.Url}";
			await _publicationMaintenanceLogger.Log(url.PublicationId, User.GetUserId(), log);
			var result = await ConcurrentSave(_db, log, "Unable to remove URL.");
			if (result)
			{
				await _publisher.SendPublicationEdit(
					$"{PublicationId}M edited by {User.Name()}",
					$"[{PublicationId}M]({{0}}) edited by {User.Name()}",
					$"Deleted {url.Type} URL",
					$"{PublicationId}M");

				await _youtubeSync.UnlistVideo(url.Url!);
			}
		}

		return RedirectToPage("List", new { PublicationId });
	}
}
