using Microsoft.AspNetCore.Mvc.ViewComponents;
using TASVideos.Core.Services.PublicationChain;
using TASVideos.WikiEngine;

namespace TASVideos.WikiModules;

[WikiModule(ModuleNames.PublicationHistory)]
public class PublicationHistory(ApplicationDbContext db, IPublicationHistory history) : WikiViewComponent
{
	public PublicationHistoryGroup History { get; set; } = new();

	public async Task<IViewComponentResult> InvokeAsync(int publicationId)
	{
		var publication = await db.Publications.SingleOrDefaultAsync(p => p.Id == publicationId);
		if (publication is null)
		{
			return new ContentViewComponentResult($"Invalid publication id: {publicationId}");
		}

		var gameHistory = await history.ForGame(publication.GameId);
		if (gameHistory is null)
		{
			return new ContentViewComponentResult($"Invalid publication id: {publicationId}");
		}

		History = gameHistory;
		ViewData["Highlight"] = publicationId;
		ViewData["HighlightClass"] = "fw-bold fst-italic border border-info p-1";
		return View();
	}
}
