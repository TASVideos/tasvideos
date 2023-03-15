using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TASVideos.Core;
using TASVideos.Data;
using TASVideos.WikiEngine;

namespace TASVideos.ViewComponents;

[WikiModule(WikiModules.MovieChangeLog)]
public class MovieChangeLog : ViewComponent
{
	private readonly ApplicationDbContext _db;

	public MovieChangeLog(ApplicationDbContext db)
	{
		_db = db;
	}

	public async Task<IViewComponentResult> InvokeAsync(string pubClass)
	{
		var paging = this.GetPagingModel();

		var query = _db.Publications.AsQueryable();

		var publicationClass = await _db.PublicationClasses.FirstOrDefaultAsync(c => c.Name == pubClass);
		if (publicationClass is not null)
		{
			query = query.Where(p => p.PublicationClassId == publicationClass.Id);
		}

		var model = await query
			.OrderByDescending(p => p.CreateTimestamp)
			.Select(p => new MovieHistoryModel
			{
				Date = p.CreateTimestamp.Date,
				Pubs = new List<MovieHistoryModel.PublicationEntry>
				{
					new ()
					{
						Id = p.Id,
						Name = p.Title,
						IsNewGame = p.Game != null && p.Game.Publications.OrderBy(gp => gp.CreateTimestamp).FirstOrDefault() == p,
						IsNewBranch = p.ObsoletedMovies.Count == 0,
						ClassIconPath = p.PublicationClass!.IconPath
					}
				}
			})
			.PageOf(paging);

		this.SetPagingToViewData(paging);
		return View("Default", model);
	}
}
