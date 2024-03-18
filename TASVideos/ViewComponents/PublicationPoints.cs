using Microsoft.AspNetCore.Mvc;
using TASVideos.Core.Services;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.WikiEngine;

namespace TASVideos.ViewComponents;

[WikiModule(WikiModules.PublicationPoints)]
public class PublicationPoints(ApplicationDbContext db, IPointsService pointsService) : ViewComponent
{
	public async Task<IViewComponentResult> InvokeAsync()
	{
		var publications = await db.Publications
			.ThatAreCurrent()
			.Select(p => new PublicationPointsModel
			{
				Id = p.Id,
				Title = p.Title,
			})
			.ToListAsync();

		foreach (var pub in publications)
		{
			pub.Points = await pointsService.PlayerPointsForPublication(pub.Id);
		}

		var sortedPublications = publications
			.OrderByDescending(u => u.Points)
			.ToList();

		int counter = 0;
		foreach (var pub in sortedPublications)
		{
			pub.Position = ++counter;
		}

		return View(sortedPublications);
	}
}
