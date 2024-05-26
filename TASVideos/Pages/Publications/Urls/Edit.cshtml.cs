using Microsoft.AspNetCore.Mvc.Rendering;
using TASVideos.Core.Services.ExternalMediaPublisher;
using TASVideos.Core.Services.Wiki;
using TASVideos.Core.Services.Youtube;

namespace TASVideos.Pages.Publications.Urls;

[RequirePermission(PermissionTo.EditPublicationFiles)]
public class EditUrlsModel(
	ApplicationDbContext db,
	ExternalMediaPublisher publisher,
	IYoutubeSync youtubeSync,
	IPublicationMaintenanceLogger publicationMaintenanceLogger,
	IWikiPages wikiPages)
	: BasePageModel
{
	private static readonly IEnumerable<PublicationUrlType> PublicationUrlTypes = Enum.GetValues<PublicationUrlType>();

	public List<SelectListItem> AvailableTypes => PublicationUrlTypes.ToDropDown();

	[FromRoute]
	public int PublicationId { get; set; }

	[FromRoute]
	public int? UrlId { get; set; }

	[BindProperty]
	public string Title { get; set; } = "";

	public ICollection<PublicationUrl> CurrentUrls { get; set; } = [];

	[StringLength(100)]
	[Display(Name = "Alt Title")]
	[BindProperty]
	public string? DisplayName { get; set; }

	[BindProperty]
	[Url]
	[Display(Name = "URL")]
	public string CurrentUrl { get; set; } = "";

	[BindProperty]
	[Display(Name = "Type")]
	public PublicationUrlType UrlType { get; set; }

	public async Task<IActionResult> OnGet()
	{
		var title = await db.Publications
			.Where(p => p.Id == PublicationId)
			.Select(p => p.Title)
			.SingleOrDefaultAsync();

		if (title is null)
		{
			return NotFound();
		}

		Title = title;
		CurrentUrls = await db.PublicationUrls
			.Where(u => u.PublicationId == PublicationId)
			.ToListAsync();

		if (!UrlId.HasValue)
		{
			return Page();
		}

		var url = CurrentUrls.SingleOrDefault(u => u.Id == UrlId.Value);

		if (url?.Url is null)
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
		var publication = await db.Publications
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

		var publicationWiki = await wikiPages.PublicationPage(PublicationId);

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

		PublicationUrl url;
		if (UrlId.HasValue)
		{
			url = CurrentUrls.Single(u => u.Id == UrlId.Value);
			logWording = ["Chang", "chang"];
		}
		else
		{
			url = db.PublicationUrls.Add(new PublicationUrl()).Entity;
			logWording = ["Add", "add"];
		}

		url.PublicationId = PublicationId;
		url.DisplayName = DisplayName;
		url.Type = UrlType;
		url.Url = CurrentUrl;

		string log = $"{logWording[0]}ed {DisplayName} {UrlType} URL {CurrentUrl}";
		await publicationMaintenanceLogger.Log(PublicationId, User.GetUserId(), log);
		var result = await db.TrySaveChanges();
		SetMessage(result, log, $"Unable to {logWording[1]} URL.");
		if (result.IsSuccess())
		{
			await publisher.SendPublicationEdit(
				User.Name(), PublicationId, $"{logWording[0]}ed {UrlType} URL | {Title}");

			if (UrlType == PublicationUrlType.Streaming && youtubeSync.IsYoutubeUrl(CurrentUrl))
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
				await youtubeSync.SyncYouTubeVideo(video);
			}
		}

		return RedirectToPage("List", new { PublicationId });
	}
}
