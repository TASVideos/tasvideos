﻿using Microsoft.AspNetCore.Mvc;
using TASVideos.Core.Services;
using TASVideos.Core.Services.ExternalMediaPublisher;
using TASVideos.Core.Services.Youtube;
using TASVideos.Data;
using TASVideos.Data.Entity;

namespace TASVideos.Pages.Publications.Urls;

[RequirePermission(PermissionTo.EditPublicationFiles)]
public class ListUrlsModel(
	ApplicationDbContext db,
	ExternalMediaPublisher publisher,
	IYoutubeSync youtubeSync,
	IPublicationMaintenanceLogger publicationMaintenanceLogger)
	: BasePageModel
{
	[FromRoute]
	public int PublicationId { get; set; }

	public string Title { get; set; } = "";

	public ICollection<PublicationUrl> CurrentUrls { get; set; } = new List<PublicationUrl>();

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
		var url = await db.PublicationUrls
			.SingleOrDefaultAsync(pf => pf.Id == urlId);

		if (url != null)
		{
			db.PublicationUrls.Remove(url);
			string log = $"Deleted {url.DisplayName} {url.Type} URL {url.Url}";
			await publicationMaintenanceLogger.Log(url.PublicationId, User.GetUserId(), log);
			var result = await ConcurrentSave(db, log, "Unable to remove URL.");
			if (result)
			{
				await publisher.SendPublicationEdit(
					$"{PublicationId}M edited by {User.Name()}",
					$"[{PublicationId}M]({{0}}) edited by {User.Name()}",
					$"Deleted {url.Type} URL",
					$"{PublicationId}M");

				await youtubeSync.UnlistVideo(url.Url!);
			}
		}

		return RedirectToPage("List", new { PublicationId });
	}
}
