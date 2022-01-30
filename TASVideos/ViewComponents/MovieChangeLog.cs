using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Microsoft.EntityFrameworkCore;
using TASVideos.Core.Services.PublicationChain;
using TASVideos.Data;
using TASVideos.WikiEngine;

namespace TASVideos.ViewComponents;

[WikiModule(WikiModules.MovieChangeLog)]
public class MovieChangeLog : ViewComponent
{
	private readonly ApplicationDbContext _db;
	private readonly IPublicationHistory _history;

	public MovieChangeLog(ApplicationDbContext db, IPublicationHistory history)
	{
		_db = db;
		_history = history;
	}

	public async Task<IViewComponentResult> InvokeAsync(int? maxdays, int? seed)
	{
		if (seed.HasValue)
		{
			return await Seed(seed.Value);
		}

		return await MaxDays(maxdays ?? 60);
	}

	public async Task<IViewComponentResult> Seed(int publicationId)
	{
		var publication = await _db.Publications.SingleOrDefaultAsync(p => p.Id == publicationId);
		if (publication == null)
		{
			return new ContentViewComponentResult($"Invalid publication id: {publicationId}");
		}

		var history = await _history.ForGame(publication.GameId);
		if (history == null)
		{
			return new ContentViewComponentResult($"Invalid publication id: {publicationId}");
		}

		return View("Seed", history);
	}

	public async Task<IViewComponentResult> MaxDays(int maxDays)
	{
		var movieHistory = await GetRecentPublications(maxDays);

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
