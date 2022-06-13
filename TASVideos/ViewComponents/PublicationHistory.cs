using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Microsoft.EntityFrameworkCore;
using TASVideos.Core.Services.PublicationChain;
using TASVideos.Data;
using TASVideos.WikiEngine;

namespace TASVideos.ViewComponents;

[WikiModule(WikiModules.PublicationHistory)]
public class PublicationHistory : ViewComponent
{
	private readonly ApplicationDbContext _db;
	private readonly IPublicationHistory _history;

	public PublicationHistory(ApplicationDbContext db, IPublicationHistory history)
	{
		_db = db;
		_history = history;
	}

	public async Task<IViewComponentResult> InvokeAsync(int publicationId)
	{
		var publication = await _db.Publications.SingleOrDefaultAsync(p => p.Id == publicationId);
		if (publication is null)
		{
			return new ContentViewComponentResult($"Invalid publication id: {publicationId}");
		}

		var history = await _history.ForGame(publication.GameId);
		if (history is null)
		{
			return new ContentViewComponentResult($"Invalid publication id: {publicationId}");
		}

		return View(history);
	}
}
