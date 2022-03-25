using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TASVideos.Core.Services;
using TASVideos.Core.Services.ExternalMediaPublisher;
using TASVideos.Core.Services.Youtube;
using TASVideos.Data;
using TASVideos.Data.Entity;

namespace TASVideos.Pages.Publications;

[RequirePermission(PermissionTo.EditPublicationFiles)]
public class EditUrlsModel : BasePageModel
{
	private readonly ApplicationDbContext _db;
	private readonly ExternalMediaPublisher _publisher;
	private readonly IYoutubeSync _youtubeSync;
	private readonly IPublicationMaintenanceLogger _publicationMaintenanceLogger;

	private static readonly List<PublicationUrlType> PublicationUrlTypes = Enum
		.GetValues(typeof(PublicationUrlType))
		.Cast<PublicationUrlType>()
		.ToList();

	public EditUrlsModel(
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

	public IEnumerable<SelectListItem> AvailableTypes =
		PublicationUrlTypes
			.Select(t => new SelectListItem
			{
				Text = t.ToString(),
				Value = ((int)t).ToString()
			});

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
	[Display(Name = "Url")]
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

	public async Task<IActionResult> OnPost()
	{
		var publication = await _db.Publications
			.Where(p => p.Id == Id)
			.Select(p => new
			{
				Title,
				p.CreateTimestamp,
				p.PublicationUrls,
				SystemCode = p.System!.Code,
				p.WikiContent,
				Authors = p.Authors.OrderBy(pa => pa.Ordinal).Select(pa => pa.Author!.UserName),
				p.Game!.SearchKey,
				p.ObsoletedById
			})
			.SingleOrDefaultAsync();

		if (publication is null)
		{
			return NotFound();
		}

		CurrentUrls = publication.PublicationUrls;

		if (CurrentUrls.Any(u => u.Type == UrlType && u.Url == PublicationUrl))
		{
			ModelState.AddModelError($"{nameof(PublicationUrl)}", $"The {UrlType} url: {PublicationUrl} already exists");
		}

		if (!ModelState.IsValid)
		{
			return Page();
		}

		var publicationUrl = new PublicationUrl
		{
			PublicationId = Id,
			Url = PublicationUrl,
			Type = UrlType,
			DisplayName = DisplayName
		};

		_db.PublicationUrls.Add(publicationUrl);

		string log = $"Added {DisplayName} {UrlType} url {PublicationUrl}";
		await _publicationMaintenanceLogger.Log(Id, User.GetUserId(), log);
		var result = await ConcurrentSave(_db, log, "Unable to add url.");
		if (result)
		{
			await _publisher.SendPublicationEdit(
				$"{Id}M edited by {User.Name()}",
				$"Added {UrlType} url | {Title}",
				$"{Id}M");

			if (UrlType == PublicationUrlType.Streaming && _youtubeSync.IsYoutubeUrl(PublicationUrl))
			{
				YoutubeVideo video = new(
					Id,
					publication.CreateTimestamp,
					PublicationUrl,
					DisplayName,
					publication.Title,
					publication.WikiContent!,
					publication.SystemCode,
					publication.Authors,
					publication.SearchKey,
					publication.ObsoletedById);
				await _youtubeSync.SyncYouTubeVideo(video);
			}
		}

		return RedirectToPage("EditUrls", new { Id });
	}

	public async Task<IActionResult> OnPostDelete(int publicationUrlId)
	{
		var url = await _db.PublicationUrls
			.SingleOrDefaultAsync(pf => pf.Id == publicationUrlId);

		if (url != null)
		{
			_db.PublicationUrls.Remove(url);
			string log = $"Deleted {url.DisplayName} {url.Type} url {url.Url}";
			await _publicationMaintenanceLogger.Log(url.PublicationId, User.GetUserId(), log);
			var result = await ConcurrentSave(_db, log, "Unable to remove url.");
			if (result)
			{
				await _publisher.SendPublicationEdit(
					$"{Id}M edited by {User.Name()}",
					$"Deleted {url.Type} url",
					$"{Id}M");

				await _youtubeSync.UnlistVideo(url.Url!);
			}
		}

		return RedirectToPage("EditUrls", new { Id });
	}
}
