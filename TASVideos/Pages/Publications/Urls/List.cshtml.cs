using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TASVideos.Core.Services;
using TASVideos.Core.Services.ExternalMediaPublisher;
using TASVideos.Core.Services.Wiki;
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
	public int Id { get; set; }

	[BindProperty]
	public string Title { get; set; } = "";

	public ICollection<PublicationUrl> CurrentUrls { get; set; } = new List<PublicationUrl>();

	[StringLength(100)]
	[Display(Name = "Alt Title")]
	[BindProperty]
	public string? DisplayName { get; set; }

	[Required]
	[BindProperty]
	[Url]
	[Display(Name = "URL")]
	public string PublicationUrl { get; set; } = "";

	[Required]
	[BindProperty]
	[Display(Name = "Type")]
	public PublicationUrlType UrlType { get; set; }

	public async Task<IActionResult> OnGet()
	{
		var title = await _db.Publications
			.Where(p => p.Id == Id)
			.Select(p => p.Title)
			.SingleOrDefaultAsync();

		if (title is null)
		{
			return NotFound();
		}

		Title = title;
		CurrentUrls = await _db.PublicationUrls
			.Where(u => u.PublicationId == Id)
			.ToListAsync();

		return Page();
	}

	public async Task<IActionResult> OnPostDelete(int publicationUrlId)
	{
		var url = await _db.PublicationUrls
			.SingleOrDefaultAsync(pf => pf.Id == publicationUrlId);

		if (url != null)
		{
			_db.PublicationUrls.Remove(url);
			string log = $"Deleted {url.DisplayName} {url.Type} URL {url.Url}";
			await _publicationMaintenanceLogger.Log(url.PublicationId, User.GetUserId(), log);
			var result = await ConcurrentSave(_db, log, "Unable to remove URL.");
			if (result)
			{
				await _publisher.SendPublicationEdit(
					$"{Id}M edited by {User.Name()}",
					$"[{Id}M]({{0}}) edited by {User.Name()}",
					$"Deleted {url.Type} URL",
					$"{Id}M");

				await _youtubeSync.UnlistVideo(url.Url!);
			}
		}

		return RedirectToPage("List", new { Id });
	}
}
