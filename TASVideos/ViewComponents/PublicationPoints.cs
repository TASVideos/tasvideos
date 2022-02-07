using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TASVideos.Core.Services;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.WikiEngine;

namespace TASVideos.ViewComponents;

[WikiModule(WikiModules.PublicationPoints)]
public class PublicationPoints : ViewComponent
{
	private readonly ApplicationDbContext _db;
	private readonly IPointsService _pointsService;

	public PublicationPoints(ApplicationDbContext db, IPointsService pointsService)
	{
		_db = db;
		_pointsService = pointsService;
	}

	public async Task<IViewComponentResult> InvokeAsync()
	{
		var publications = await _db.Publications
			.ThatAreCurrent()
			.Select(p => new PublicationPointsModel
			{
				Id = p.Id,
				Title = p.Title,
			})
			.ToListAsync();

		foreach (var pub in publications)
		{
			pub.Points = await _pointsService.PlayerPointsForPublication(pub.Id);
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
