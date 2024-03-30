using Microsoft.AspNetCore.Mvc.ViewComponents;
using TASVideos.Core.Services.PublicationChain;
using TASVideos.WikiEngine;

namespace TASVideos.WikiModules;

[WikiModule(ModuleNames.PublicationHistory)]
public class PublicationHistory(ApplicationDbContext db, IPublicationHistory history) : ViewComponent
{
	public async Task<IViewComponentResult> InvokeAsync(int publicationId)
	{
		var publication = await db.Publications.SingleOrDefaultAsync(p => p.Id == publicationId);
		if (publication is null)
		{
			return new ContentViewComponentResult($"Invalid publication id: {publicationId}");
		}

		var history1 = await history.ForGame(publication.GameId);
		if (history1 is null)
		{
			return new ContentViewComponentResult($"Invalid publication id: {publicationId}");
		}

		ViewData["Highlight"] = publicationId;
		return View(history1);
	}
}
