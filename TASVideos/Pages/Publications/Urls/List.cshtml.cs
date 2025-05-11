using TASVideos.Core.Services.Youtube;

namespace TASVideos.Pages.Publications.Urls;

[RequirePermission(PermissionTo.EditPublicationFiles)]
public class ListUrlsModel(
	ApplicationDbContext db,
	IExternalMediaPublisher publisher,
	IYoutubeSync youtubeSync,
	IPublicationMaintenanceLogger publicationMaintenanceLogger)
	: BasePageModel
{
	[FromRoute]
	public int PublicationId { get; set; }

	public string Title { get; set; } = "";

	public List<PublicationUrl> CurrentUrls { get; set; } = [];

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

		return Page();
	}

	public async Task<IActionResult> OnPostDelete(int urlId)
	{
		var url = await db.PublicationUrls.FindAsync(urlId);

		if (url is not null)
		{
			db.PublicationUrls.Remove(url);
			string log = $"Deleted {url.DisplayName} {url.Type} URL {url.Url}";
			await publicationMaintenanceLogger.Log(url.PublicationId, User.GetUserId(), log);
			var result = await db.TrySaveChanges();
			SetMessage(result, log, "Unable to remove URL.");
			if (result.IsSuccess())
			{
				await publisher.SendPublicationEdit(User.Name(), PublicationId, $"Deleted {url.Type} URL");
				await youtubeSync.UnlistVideo(url.Url!);
			}
		}

		return RedirectToPage("List", new { PublicationId });
	}
}
