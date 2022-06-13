using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

	public async Task<IViewComponentResult> InvokeAsync(int? maxDays)
	{
		var movieHistory = await GetRecentPublications(maxDays ?? 60);

		// merge movie history entries so every date holds multiple publications (as applicable)
		var mergedHistoryModel = new MovieHistoryModel
		{
			MovieHistory = movieHistory
				.GroupBy(gkey => gkey.Date)
				.Select(group => new MovieHistoryModel.MovieHistoryEntry
				{
					Date = group.Key,
					Pubs = group.SelectMany(item => item.Pubs).Distinct().ToList()
				}).ToList()
		};

		return View("Default", mergedHistoryModel);
	}

	private async Task<IList<MovieHistoryModel.MovieHistoryEntry>> GetRecentPublications(int maxDays)
	{
		var minTimestamp = DateTime.UtcNow.AddDays(-maxDays);
		var results = await _db.Publications
			.Where(p => p.CreateTimestamp >= minTimestamp)
			.Select(p => new MovieHistoryModel.MovieHistoryEntry
			{
				Date = p.CreateTimestamp.Date,
				Pubs = new List<MovieHistoryModel.PublicationEntry>
				{
					new ()
					{
						Id = p.Id,
						Name = p.Title,
						IsNewGame = p.Game != null && p.Game.Publications.FirstOrDefault() == p,
						IsNewBranch = p.ObsoletedMovies.Count == 0
					}
				}
			})
			.ToListAsync();

		return results;
	}
}
