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
public class EditUrlsModel : BasePageModel
{
	private readonly ApplicationDbContext _db;
	private readonly ExternalMediaPublisher _publisher;
	private readonly IYoutubeSync _youtubeSync;
	private readonly IPublicationMaintenanceLogger _publicationMaintenanceLogger;
	private readonly IWikiPages _wikiPages;

	private static readonly List<PublicationUrlType> PublicationUrlTypes = Enum
		.GetValues(typeof(PublicationUrlType))
		.Cast<PublicationUrlType>()
		.ToList();

	public EditUrlsModel(
		ApplicationDbContext db,
		ExternalMediaPublisher publisher,
		IYoutubeSync youtubeSync,
		IPublicationMaintenanceLogger publicationMaintenanceLogger,
		IWikiPages wikiPages)
	{
		_db = db;
		_publisher = publisher;
		_youtubeSync = youtubeSync;
		_publicationMaintenanceLogger = publicationMaintenanceLogger;
		_wikiPages = wikiPages;
	}

	public IEnumerable<SelectListItem> AvailableTypes =
		PublicationUrlTypes
			.Select(t => new SelectListItem
			{
				Text = t.ToString(),
				Value = ((int)t).ToString()
			});

	[FromRoute]
	public int PublicationId { get; set; }

	[FromRoute]
	public int? UrlId { get; set; }

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
	public string CurrentUrl { get; set; } = "";

	[Required]
	[BindProperty]
	[Display(Name = "Type")]
	public PublicationUrlType UrlType { get; set; }

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

		if (!UrlId.HasValue)
		{
			return Page();
		}

		var url = CurrentUrls
			.SingleOrDefault(u => u.Id == UrlId.Value);

		if (url is null || url.Url is null)
		{
			return NotFound();
		}

		PublicationId = url.PublicationId;
		DisplayName = url.DisplayName;
		UrlType = url.Type;
		CurrentUrl = url.Url;

		return Page();
	}

	public async Task<IActionResult> OnPost()
	{
		var publication = await _db.Publications
			.Where(p => p.Id == PublicationId)
			.Select(p => new
			{
				Title,
				p.CreateTimestamp,
				p.PublicationUrls,
				SystemCode = p.System!.Code,
				Authors = p.Authors.OrderBy(pa => pa.Ordinal).Select(pa => pa.Author!.UserName),
				p.ObsoletedById
			})
			.SingleOrDefaultAsync();

		if (publication is null)
		{
			return NotFound();
		}

		var publicationWiki = await _wikiPages.PublicationPage(PublicationId);

		CurrentUrls = publication.PublicationUrls;

		if (CurrentUrls.Any(u => u.Type == UrlType && u.Url == CurrentUrl && u.Id != UrlId))
		{
			ModelState.AddModelError($"{nameof(CurrentUrl)}", $"The {UrlType} URL: {CurrentUrl} already exists");
		}

		if (!ModelState.IsValid)
		{
			return Page();
		}

		string[] logWording;

		if (UrlId.HasValue)
		{
			var url = CurrentUrls
				.Single(u => u.Id == UrlId.Value);

			url.PublicationId = PublicationId;
			url.DisplayName = DisplayName;
			url.Type = UrlType;
			url.Url = CurrentUrl;

			logWording = new[] { "Add", "add" };
		}
		else
		{
			_db.PublicationUrls.Add(new PublicationUrl
			{
				PublicationId = PublicationId,
				Url = CurrentUrl,
				Type = UrlType,
				DisplayName = DisplayName
			});

			logWording = new[] { "Change", "change" };
		}

		string log = $"{logWording[0]}ed {DisplayName} {UrlType} URL {CurrentUrl}";
		await _publicationMaintenanceLogger.Log(PublicationId, User.GetUserId(), log);
		var result = await ConcurrentSave(_db, log, $"Unable to {logWording[1]} URL.");
		if (result)
		{
			await _publisher.SendPublicationEdit(
				$"{PublicationId}M edited by {User.Name()}",
				$"[{PublicationId}M]({{0}}) edited by {User.Name()}",
				$"{logWording[0]}ed {UrlType} URL | {Title}",
				$"{PublicationId}M");

			if (UrlType == PublicationUrlType.Streaming && _youtubeSync.IsYoutubeUrl(CurrentUrl))
			{
				YoutubeVideo video = new(
					PublicationId,
					publication.CreateTimestamp,
					CurrentUrl,
					DisplayName,
					publication.Title,
					publicationWiki!,
					publication.SystemCode,
					publication.Authors,
					publication.ObsoletedById);
				await _youtubeSync.SyncYouTubeVideo(video);
			}
		}

		return RedirectToPage("List", new { PublicationId });
	}
}
