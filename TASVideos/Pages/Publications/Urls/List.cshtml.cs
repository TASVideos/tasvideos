using TASVideos.Core.Services.Youtube;

namespace TASVideos.Pages.Publications.Urls;

[RequirePermission(PermissionTo.EditPublicationFiles)]
public class ListUrlsModel(
	IPublications publications,
	IExternalMediaPublisher publisher,
	IYoutubeSync youtubeSync,
	IPublicationMaintenanceLogger publicationMaintenanceLogger)
	: BasePageModel
{
	[FromRoute]
	public int PublicationId { get; set; }

	public string Title { get; set; } = "";

	public ICollection<PublicationUrl> CurrentUrls { get; set; } = [];

	public async Task<IActionResult> OnGet()
	{
		var title = await publications.GetTitle(PublicationId);
		if (title is null)
		{
			return NotFound();
		}

		Title = title;
		CurrentUrls = await publications.GetUrls(PublicationId);

		return Page();
	}

	public async Task<IActionResult> OnPostDelete(int urlId)
	{
		var url = await publications.RemoveUrl(urlId);
		if (url is not null)
		{
			var log = $"Deleted {url.DisplayName} {url.Type} URL {url.Url}";
			await publicationMaintenanceLogger.Log(url.PublicationId, User.GetUserId(), log);
			SuccessStatusMessage(log);
			await publisher.SendPublicationEdit(User.Name(), PublicationId, $"Deleted {url.Type} URL");
			await youtubeSync.UnlistVideo(url.Url!);
		}

		return RedirectToPage("List", new { PublicationId });
	}
}
