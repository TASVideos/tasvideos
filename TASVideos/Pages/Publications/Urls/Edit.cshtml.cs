using TASVideos.Core.Services.Wiki;
using TASVideos.Core.Services.Youtube;

namespace TASVideos.Pages.Publications.Urls;

[RequirePermission(PermissionTo.EditPublicationFiles)]
public class EditUrlsModel(
	ApplicationDbContext db,
	IExternalMediaPublisher publisher,
	IYoutubeSync youtubeSync,
	IPublicationMaintenanceLogger publicationMaintenanceLogger,
	IWikiPages wikiPages)
	: BasePageModel
{
	public static List<SelectListItem> AvailableTypes => Enum.GetValues<PublicationUrlType>().ToDropDown();

	[FromRoute]
	public int PublicationId { get; set; }

	[FromRoute]
	public int? UrlId { get; set; }

	[BindProperty]
	public string Title { get; set; } = "";

	public ICollection<PublicationUrl> CurrentUrls { get; set; } = [];

	[StringLength(100)]
	[BindProperty]
	public string? AltTitle { get; set; }

	[BindProperty]
	[Url]
	public string CurrentUrl { get; set; } = "";

	[BindProperty]
	public PublicationUrlType Type { get; set; }

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
		AltTitle = url.DisplayName;
		Type = url.Type;
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

		CurrentUrls = publication.PublicationUrls;

		if (CurrentUrls.Any(u => u.Type == Type && u.Url == CurrentUrl && u.Id != UrlId))
		{
			ModelState.AddModelError($"{nameof(CurrentUrl)}", $"The {Type} URL: {CurrentUrl} already exists");
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
		url.DisplayName = AltTitle;
		url.Type = Type;
		url.Url = CurrentUrl;

		string log = $"{logWording[0]}ed {AltTitle} {Type} URL {CurrentUrl}";
		await publicationMaintenanceLogger.Log(PublicationId, User.GetUserId(), log);
		var result = await db.TrySaveChanges();
		SetMessage(result, log, $"Unable to {logWording[1]} URL.");
		if (result.IsSuccess())
		{
			await publisher.SendPublicationEdit(
				User.Name(), PublicationId, $"{logWording[0]}ed {Type} URL | {Title}");

			if (Type == PublicationUrlType.Streaming && youtubeSync.IsYoutubeUrl(CurrentUrl))
			{
				var publicationWiki = await wikiPages.PublicationPage(PublicationId);
				YoutubeVideo video = new(
					PublicationId,
					publication.CreateTimestamp,
					CurrentUrl,
					AltTitle,
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
