using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TASVideos.Core.Services;
using TASVideos.Core.Services.ExternalMediaPublisher;
using TASVideos.Core.Services.Wiki;
using TASVideos.Core.Services.Youtube;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Data.Entity.Game;
using TASVideos.Pages.Games.Versions.Models;
using static TASVideos.Core.Services.AwardAssignment;

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
	public int Id { get; set; }

	[FromRoute]
	public int? publicationUrlId { get; set; }

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

		if (!publicationUrlId.HasValue)
		{
			return Page();
		}

		var url = CurrentUrls
			.Where(u => u.Id == publicationUrlId.Value)
			.SingleOrDefault();

		if (url is null || url.Url is null)
		{
			return NotFound();
		}

		Id = url.PublicationId;
		DisplayName = url.DisplayName;
		UrlType = url.Type;
		PublicationUrl = url.Url;

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
				Authors = p.Authors.OrderBy(pa => pa.Ordinal).Select(pa => pa.Author!.UserName),
				p.ObsoletedById
			})
			.SingleOrDefaultAsync();

		if (publication is null)
		{
			return NotFound();
		}

		var publicationWiki = await _wikiPages.PublicationPage(Id);

		CurrentUrls = publication.PublicationUrls;

		if (!publicationUrlId.HasValue && CurrentUrls.Any(u => u.Type == UrlType && u.Url == PublicationUrl))
		{
			ModelState.AddModelError($"{nameof(PublicationUrl)}", $"The {UrlType} URL: {PublicationUrl} already exists");
		}

		if (!ModelState.IsValid)
		{
			return Page();
		}

		string[] logwording;

		if (publicationUrlId.HasValue)
		{
			var url = CurrentUrls
				.Where(u => u.Id == publicationUrlId.Value)
				.Single();

			url.PublicationId = Id;
			url.DisplayName = DisplayName;
			url.Type = UrlType;
			url.Url = PublicationUrl;

			logwording = new string[2] { "Add", "add" };
		}
		else
		{
			_db.PublicationUrls.Add(new PublicationUrl
			{
				PublicationId = Id,
				Url = PublicationUrl,
				Type = UrlType,
				DisplayName = DisplayName
			});

			logwording = new string[2] { "Change", "change" };
		}

		string log = $"{logwording[0]}ed {DisplayName} {UrlType} url {PublicationUrl}";
		await _publicationMaintenanceLogger.Log(Id, User.GetUserId(), log);
		var result = await ConcurrentSave(_db, log, $"Unable to {logwording[1]} URL.");
		if (result)
		{
			await _publisher.SendPublicationEdit(
				$"{Id}M edited by {User.Name()}",
				$"[{Id}M]({{0}}) edited by {User.Name()}",
				$"{logwording[0]}ed {UrlType} url | {Title}",
				$"{Id}M");

			if (UrlType == PublicationUrlType.Streaming && _youtubeSync.IsYoutubeUrl(PublicationUrl))
			{
				YoutubeVideo video = new(
					Id,
					publication.CreateTimestamp,
					PublicationUrl,
					DisplayName,
					publication.Title,
					publicationWiki!,
					publication.SystemCode,
					publication.Authors,
					publication.ObsoletedById);
				await _youtubeSync.SyncYouTubeVideo(video);
			}
		}

		return RedirectToPage("List", new { Id });
	}
}
